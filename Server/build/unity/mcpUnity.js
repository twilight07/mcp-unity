import WebSocket from 'ws';
import { v4 as uuidv4 } from 'uuid';
import { McpUnityError, ErrorType } from '../utils/errors.js';
export class McpUnity {
    logger;
    port;
    ws = null;
    pendingRequests = new Map();
    REQUEST_TIMEOUT = 10000;
    lastPongTime = 0;
    pingInterval = null;
    PING_INTERVAL = 30000; // 30 seconds
    PONG_TIMEOUT = 10000; // 10 seconds
    constructor(logger) {
        this.logger = logger;
        // Initialize port from environment variable or use default
        const envPort = process.env.UNITY_PORT;
        this.port = envPort ? parseInt(envPort, 10) : 8090;
        this.logger.info(`Using port: ${this.port} for Unity WebSocket connection`);
    }
    /**
     * Start the Unity connection
     */
    async start() {
        try {
            this.logger.info('Attempting to connect to Unity WebSocket...');
            await this.connect();
            this.logger.info('Successfully connected to Unity WebSocket');
        }
        catch (error) {
            this.logger.warn(`Could not connect to Unity WebSocket: ${error instanceof Error ? error.message : String(error)}`);
            this.logger.warn('Will retry connection on next request');
        }
        return Promise.resolve();
    }
    /**
     * Connect to the Unity WebSocket
     */
    async connect() {
        if (this.isConnected) {
            this.logger.debug('Already connected to Unity WebSocket');
            return Promise.resolve();
        }
        // First, properly close any existing WebSocket connection
        this.disconnect();
        return new Promise((resolve, reject) => {
            const wsUrl = `ws://localhost:${this.port}/McpUnity`;
            this.logger.debug(`Connecting to ${wsUrl}...`);
            // Create a new WebSocket
            this.ws = new WebSocket(wsUrl);
            const connectionTimeout = setTimeout(() => {
                if (this.ws && (this.ws.readyState === WebSocket.CONNECTING)) {
                    this.logger.warn('Connection timeout, terminating WebSocket');
                    // If connection is taking too long, terminate it
                    // Use terminate() not close() for CONNECTING state
                    this.ws.terminate();
                    this.ws = null;
                    reject(new McpUnityError(ErrorType.CONNECTION, 'Connection timeout'));
                }
            }, this.REQUEST_TIMEOUT);
            this.ws.onopen = () => {
                clearTimeout(connectionTimeout);
                this.logger.debug('WebSocket connected');
                this.lastPongTime = Date.now(); // Initialize pong time on connection
                this.startPingInterval();
                resolve();
            };
            this.ws.onerror = (err) => {
                clearTimeout(connectionTimeout);
                this.logger.error(`WebSocket error: ${err.message || 'Unknown error'}`);
                reject(new McpUnityError(ErrorType.CONNECTION, `Connection failed: ${err.message || 'Unknown error'}`));
                this.disconnect();
            };
            this.ws.onmessage = (event) => {
                this.handleMessage(event.data.toString());
            };
            this.ws.onclose = () => {
                this.logger.debug('WebSocket closed');
                this.disconnect();
            };
            this.ws.on('pong', () => {
                this.lastPongTime = Date.now();
            });
        });
    }
    /**
     * Start ping interval to keep connection alive and detect disconnections
     */
    startPingInterval() {
        // Clear any existing interval
        if (this.pingInterval) {
            clearInterval(this.pingInterval);
        }
        // Set up new ping interval
        this.pingInterval = setInterval(() => {
            if (this.ws && this.ws.readyState === WebSocket.OPEN) {
                // Check if we've received a pong recently
                const timeSinceLastPong = Date.now() - this.lastPongTime;
                if (timeSinceLastPong > this.PING_INTERVAL + this.PONG_TIMEOUT) {
                    this.logger.warn(`No pong received for ${timeSinceLastPong}ms, considering connection dead`);
                    this.disconnect();
                    return;
                }
                // Send ping
                try {
                    this.ws.ping();
                }
                catch (err) {
                    this.logger.error(`Error sending ping: ${err instanceof Error ? err.message : String(err)}`);
                    this.disconnect();
                }
            }
            else {
                // WebSocket is not open, clear interval
                this.stopPingInterval();
            }
        }, this.PING_INTERVAL);
    }
    /**
     * Stop ping interval
     */
    stopPingInterval() {
        if (this.pingInterval) {
            clearInterval(this.pingInterval);
            this.pingInterval = null;
        }
    }
    /**
     * Handle messages received from Unity
     */
    handleMessage(data) {
        try {
            const response = JSON.parse(data);
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
            this.logger.error(`Error parsing WebSocket message: ${e instanceof Error ? e.message : String(e)}`);
        }
    }
    /**
     * Disconnect from Unity
     */
    disconnect() {
        // Stop ping interval
        this.stopPingInterval();
        if (this.ws) {
            this.logger.debug(`Disconnecting WebSocket in state: ${this.ws.readyState}`);
            // First remove all event handlers to prevent callbacks during close
            this.ws.onopen = null;
            this.ws.onmessage = null;
            this.ws.onerror = null;
            this.ws.onclose = null;
            // Different handling based on WebSocket state
            try {
                if (this.ws.readyState === WebSocket.CONNECTING) {
                    // For sockets still connecting, use terminate() to force immediate close
                    this.ws.terminate();
                }
                else if (this.ws.readyState === WebSocket.OPEN) {
                    // For open sockets, use close() for clean shutdown
                    this.ws.close();
                }
            }
            catch (err) {
                this.logger.error(`Error closing WebSocket: ${err instanceof Error ? err.message : String(err)}`);
            }
            // Clear the reference
            this.ws = null;
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
        this.logger.info('Unity WebSocket client stopped');
        return Promise.resolve();
    }
    /**
     * Send a request to the Unity server
     */
    async sendRequest(request) {
        // Ensure we're connected first
        if (!this.isConnected) {
            this.logger.info('Not connected to Unity, connecting first...');
            await this.connect();
        }
        // Use given id or generate a new one
        const requestId = request.id || uuidv4();
        const message = {
            ...request,
            id: requestId
        };
        return new Promise((resolve, reject) => {
            // Double check isConnected again after await
            if (!this.ws || !this.isConnected) {
                reject(new McpUnityError(ErrorType.CONNECTION, 'Not connected to Unity'));
                return;
            }
            // Create timeout for the request
            const timeout = setTimeout(() => {
                if (this.pendingRequests.has(requestId)) {
                    this.logger.error(`Request ${requestId} timed out after ${this.REQUEST_TIMEOUT}ms`);
                    this.pendingRequests.delete(requestId);
                    reject(new McpUnityError(ErrorType.TIMEOUT, 'Request timed out'));
                    this.disconnect();
                }
            }, this.REQUEST_TIMEOUT);
            // Store pending request
            this.pendingRequests.set(requestId, {
                resolve,
                reject,
                timeout
            });
            try {
                this.ws.send(JSON.stringify(message));
                this.logger.debug(`Request sent: ${requestId}`);
            }
            catch (err) {
                clearTimeout(timeout);
                this.pendingRequests.delete(requestId);
                reject(new McpUnityError(ErrorType.CONNECTION, `Send failed: ${err instanceof Error ? err.message : String(err)}`));
                this.disconnect();
            }
        });
    }
    /**
     * Check if connected to Unity
     * Only returns true if the connection is guaranteed to be active
     */
    get isConnected() {
        // Basic WebSocket connection check
        const isSocketConnected = this.ws !== null && this.ws.readyState === WebSocket.OPEN;
        if (!isSocketConnected) {
            return false;
        }
        // Check if we've received a pong recently
        const timeSinceLastPong = Date.now() - this.lastPongTime;
        const isPongRecent = timeSinceLastPong < this.PING_INTERVAL + this.PONG_TIMEOUT;
        if (!isPongRecent && this.lastPongTime > 0) {
            this.logger.debug(`Connection may be stale: ${timeSinceLastPong}ms since last pong`);
            return false;
        }
        return true;
    }
}
