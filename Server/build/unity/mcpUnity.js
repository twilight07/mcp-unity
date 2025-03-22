import WebSocket from 'ws';
import { v4 as uuidv4 } from 'uuid';
import { McpUnityError, ErrorType } from '../utils/errors.js';
export class McpUnity {
    logger;
    port;
    ws = null;
    pendingRequests = new Map();
    REQUEST_TIMEOUT = 5000;
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
            if (await this.ping()) {
                this.logger.info('Successfully connected to Unity WebSocket');
            }
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
            return Promise.resolve();
        }
        // Close any existing connection
        this.disconnect();
        return new Promise((resolve, reject) => {
            const wsUrl = `ws://localhost:${this.port}`;
            this.logger.debug(`Connecting to ${wsUrl}...`);
            try {
                this.ws = new WebSocket(wsUrl);
                const connectionTimeout = setTimeout(() => {
                    if (this.ws && (this.ws.readyState === WebSocket.CONNECTING)) {
                        this.ws.terminate();
                        this.ws = null;
                        reject(new McpUnityError(ErrorType.CONNECTION, 'Connection timeout'));
                    }
                }, this.REQUEST_TIMEOUT);
                this.ws.onopen = () => {
                    clearTimeout(connectionTimeout);
                    this.logger.debug('WebSocket connected');
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
            }
            catch (err) {
                reject(new McpUnityError(ErrorType.CONNECTION, `WebSocket creation failed: ${err instanceof Error ? err.message : String(err)}`));
            }
        });
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
        if (this.ws) {
            this.ws.onopen = null;
            this.ws.onmessage = null;
            this.ws.onerror = null;
            this.ws.onclose = null;
            if (this.ws.readyState === WebSocket.OPEN || this.ws.readyState === WebSocket.CONNECTING) {
                this.ws.close();
            }
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
     * Send a ping to validate the connection
     */
    async ping() {
        try {
            const pingRequest = {
                id: uuidv4(),
                method: "ping",
                params: {}
            };
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
        // Ensure we're connected first
        if (!this.isConnected) {
            this.logger.debug('Not connected, connecting first...');
            await this.connect();
        }
        // Use given id or generate a new one
        const requestId = request.id || uuidv4();
        const message = {
            ...request,
            id: requestId
        };
        return new Promise((resolve, reject) => {
            if (!this.ws || this.ws.readyState !== WebSocket.OPEN) {
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
     */
    get isConnected() {
        return this.ws !== null && this.ws.readyState === WebSocket.OPEN;
    }
}
