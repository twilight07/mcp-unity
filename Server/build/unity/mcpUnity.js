import { Socket } from 'net';
import { v4 as uuidv4 } from 'uuid';
import { McpUnityError, ErrorType } from '../utils/errors.js';
export class McpUnity {
    logger;
    port;
    socket = null;
    pendingRequests = new Map();
    receivedData = '';
    PING_TIMEOUT = 1000; // 1 second timeout for ping
    CONNECTION_TIMEOUT = 5000; // 5 second timeout for connection
    connecting = false;
    constructor(logger) {
        this.logger = logger;
        // Initialize port from environment variable or use default
        const envPort = process.env.UNITY_PORT;
        this.port = envPort ? parseInt(envPort, 10) : 8090;
        // Log the port being used
        this.logger.info(`Using port: ${this.port} for Unity TCP server`);
    }
    /**
     * Start the Unity connection
     */
    async start() {
        // Try to establish the initial connection
        try {
            await this.connect();
            this.logger.info('Successfully connected to Unity TCP server');
        }
        catch (error) {
            this.logger.warn('Could not connect to Unity TCP server. Will retry on next request.');
        }
        return Promise.resolve();
    }
    /**
     * Connect to the Unity server
     */
    async connect() {
        // If already connected or connecting, return or wait
        if (this.isConnected) {
            return Promise.resolve();
        }
        if (this.connecting) {
            return new Promise((resolve, reject) => {
                const checkInterval = setInterval(() => {
                    if (this.isConnected) {
                        clearInterval(checkInterval);
                        resolve();
                    }
                    else if (!this.connecting) {
                        clearInterval(checkInterval);
                        reject(new McpUnityError(ErrorType.CONNECTION, 'Failed to connect to Unity'));
                    }
                }, 100);
                // Set a timeout to prevent waiting forever
                setTimeout(() => {
                    clearInterval(checkInterval);
                    reject(new McpUnityError(ErrorType.CONNECTION, 'Connection attempt timed out'));
                }, this.CONNECTION_TIMEOUT);
            });
        }
        this.connecting = true;
        try {
            // Close any existing connection
            this.disconnect();
            // Create a new socket
            const socket = new Socket();
            // Setup socket
            socket.on('data', this.handleData.bind(this));
            socket.on('error', this.handleError.bind(this));
            socket.on('close', this.handleClose.bind(this));
            socket.setKeepAlive(true, 10000);
            // Connect to Unity
            await new Promise((resolve, reject) => {
                const connectionTimeout = setTimeout(() => {
                    socket.destroy();
                    reject(new McpUnityError(ErrorType.CONNECTION, 'Unity TCP server connection timeout'));
                }, this.CONNECTION_TIMEOUT);
                socket.once('connect', () => {
                    clearTimeout(connectionTimeout);
                    resolve();
                });
                socket.once('error', (err) => {
                    clearTimeout(connectionTimeout);
                    reject(new McpUnityError(ErrorType.CONNECTION, `Unity TCP server not available: ${err.message}`));
                });
                socket.connect(this.port, 'localhost');
            });
            this.socket = socket;
            // Do a ping test to verify the connection works
            await this.ping();
        }
        catch (error) {
            this.disconnect();
            throw error;
        }
        finally {
            this.connecting = false;
        }
    }
    /**
     * Handle data received from Unity
     */
    handleData(data) {
        // Accumulate received data
        this.receivedData += data.toString('utf8');
        try {
            // Parse the JSON response
            const response = JSON.parse(this.receivedData);
            // Reset received data after successful parse
            this.receivedData = '';
            // Process the response if we have a matching request
            if (response.id && this.pendingRequests.has(response.id)) {
                const request = this.pendingRequests.get(response.id);
                clearTimeout(request.timeout);
                this.pendingRequests.delete(response.id);
                if (response.error) {
                    request.reject(new McpUnityError(ErrorType.TOOL_EXECUTION, response.error.message || 'Unknown error', response.error.details));
                }
                else {
                    request.resolve(response.result);
                }
            }
        }
        catch (e) {
            // Partial data, wait for more
        }
    }
    /**
     * Handle socket errors
     */
    handleError(err) {
        this.logger.error(`Socket error: ${err.message}`);
        this.disconnect();
    }
    /**
     * Handle socket close
     */
    handleClose() {
        this.logger.debug('Socket closed');
        this.disconnect();
    }
    /**
     * Disconnect from Unity and clean up
     */
    disconnect() {
        if (this.socket) {
            this.socket.removeAllListeners();
            this.socket.destroy();
            this.socket = null;
            // Reject all pending requests
            for (const [id, request] of this.pendingRequests.entries()) {
                clearTimeout(request.timeout);
                request.reject(new McpUnityError(ErrorType.CONNECTION, 'Connection closed'));
                this.pendingRequests.delete(id);
            }
        }
    }
    /**
     * Stop the Unity connection
     */
    async stop() {
        this.disconnect();
        this.logger.info('Unity TCP client stopped');
        return Promise.resolve();
    }
    /**
     * Sends a ping to Unity server to check connectivity
     */
    async ping() {
        try {
            // Create a ping request according to JSON-RPC 2.0 and MCP spec
            const pingRequest = {
                jsonrpc: "2.0",
                id: uuidv4(),
                method: "ping",
                params: {}
            };
            // Send the ping and wait for response
            await this.sendRequest(pingRequest);
            return true;
        }
        catch (error) {
            this.logger.debug(`Ping failed: ${error instanceof Error ? error.message : String(error)}`);
            return false;
        }
    }
    /**
     * Send a request to the Unity server
     */
    async sendRequest(request) {
        // Make sure we're connected before sending
        if (!this.isConnected) {
            try {
                await this.connect();
            }
            catch (error) {
                throw new McpUnityError(ErrorType.CONNECTION, 'No connection to Unity');
            }
        }
        const requestId = request.id || uuidv4();
        // Format message according to JSON-RPC 2.0
        const message = {
            jsonrpc: "2.0",
            id: requestId,
            method: request.method,
            params: request.params
        };
        return new Promise((resolve, reject) => {
            // Double-check that we're still connected
            if (!this.socket || this.socket.destroyed) {
                reject(new McpUnityError(ErrorType.CONNECTION, 'Connection to Unity lost'));
                return;
            }
            // Set timeout for request
            const timeout = setTimeout(() => {
                if (this.pendingRequests.has(requestId)) {
                    this.pendingRequests.delete(requestId);
                    reject(new McpUnityError(ErrorType.TIMEOUT, `Request timeout: ${request.method}`));
                    // Connection might be stuck, so disconnect for next request to reconnect
                    this.disconnect();
                }
            }, request.method === 'ping' ? this.PING_TIMEOUT : 5000);
            // Store pending request
            this.pendingRequests.set(requestId, {
                resolve,
                reject,
                timeout
            });
            // Send the request
            this.logger.debug(`Sending request to Unity: ${JSON.stringify(message)}`);
            this.socket.write(JSON.stringify(message), (err) => {
                if (err) {
                    clearTimeout(timeout);
                    this.pendingRequests.delete(requestId);
                    reject(new McpUnityError(ErrorType.CONNECTION, `Failed to send request: ${err.message}`));
                    this.disconnect();
                }
            });
        });
    }
    /**
     * Check if connected to Unity
     */
    get isConnected() {
        return this.socket !== null &&
            !this.socket.destroyed &&
            this.socket.readyState === 'open';
    }
}
