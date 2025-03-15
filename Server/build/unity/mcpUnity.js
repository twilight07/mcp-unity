import { WebSocketServer } from 'ws';
import { v4 as uuidv4 } from 'uuid';
import { McpUnityError, ErrorType } from '../utils/errors.js';
export class McpUnity {
    ws = null;
    connections = new Map();
    pendingRequests = new Map();
    logger;
    port;
    constructor(port = 8080, logger) {
        this.port = port;
        this.logger = logger;
    }
    async start() {
        if (this.ws) {
            this.logger.warn('MCP Node WebSocket Connection already started');
            return;
        }
        return new Promise((resolve, reject) => {
            try {
                this.logger.info(`Starting WebSocket server on port ${this.port}`);
                this.ws = new WebSocketServer({ port: this.port });
                this.ws.on('listening', () => {
                    this.logger.info(`MCP Node WebSocket listening on port ${this.port}`);
                    resolve();
                });
                this.ws.on('error', (error) => {
                    this.logger.error('WebSocket server error', error);
                    reject(error);
                });
                this.ws.on('connection', (socket) => {
                    const id = uuidv4();
                    this.connections.set(id, socket);
                    this.logger.info(`MCP Node WebSocket Connected: ${id}`);
                    socket.on('message', (data) => {
                        try {
                            const message = JSON.parse(data.toString());
                            this.handleMessage(message);
                        }
                        catch (error) {
                            this.logger.error(`Error parsing message from ${id}`, error);
                        }
                    });
                    socket.on('close', () => {
                        this.connections.delete(id);
                        this.logger.info(`MCP Node WebSocket disconnected: ${id}`);
                    });
                    socket.on('error', (error) => {
                        this.logger.error(`MCP Node WebSocket error for connection ${id}`, error);
                    });
                });
            }
            catch (error) {
                this.logger.error('Failed to create WebSocket server', error);
                reject(error);
            }
        });
    }
    async stop() {
        if (!this.ws) {
            return;
        }
        return new Promise((resolve) => {
            this.ws.close(() => {
                this.logger.info('Unity bridge stopped');
                this.ws = null;
                this.connections.clear();
                resolve();
            });
        });
    }
    async sendRequest(request) {
        if (this.connections.size === 0) {
            throw new McpUnityError(ErrorType.CONNECTION, 'No Unity connections available');
        }
        // Use the first available connection
        const firstEntry = this.connections.entries().next();
        if (firstEntry.done) {
            throw new McpUnityError(ErrorType.CONNECTION, 'No Unity connections available');
        }
        const [id, socket] = firstEntry.value;
        const requestId = request.id || uuidv4();
        const message = {
            id: requestId,
            type: 'request',
            method: request.method,
            params: request.params
        };
        return new Promise((resolve, reject) => {
            // Set timeout for request
            const timeout = setTimeout(() => {
                if (this.pendingRequests.has(requestId)) {
                    this.pendingRequests.delete(requestId);
                    reject(new McpUnityError(ErrorType.TIMEOUT, `Request timeout: ${request.method}`));
                }
            }, 5000);
            // Store pending request
            this.pendingRequests.set(requestId, {
                resolve,
                reject,
                timeout
            });
            // Send the request
            socket.send(JSON.stringify(message), (error) => {
                if (error) {
                    clearTimeout(timeout);
                    this.pendingRequests.delete(requestId);
                    reject(new McpUnityError(ErrorType.CONNECTION, `Failed to send request: ${error.message}`));
                }
                else {
                    this.logger.debug(`Sent request to Unity: ${JSON.stringify(message)}`);
                }
            });
        });
    }
    handleMessage(message) {
        const { id, type, result, error } = message;
        if (!id || !type) {
            this.logger.warn('Received message with missing id or type', message);
            return;
        }
        this.logger.debug(`Received message: ${JSON.stringify(message)}`);
        if ((type === 'response' || type === 'error') && this.pendingRequests.has(id)) {
            const pendingRequest = this.pendingRequests.get(id);
            this.pendingRequests.delete(id);
            clearTimeout(pendingRequest.timeout);
            if (type === 'error') {
                pendingRequest.reject(new McpUnityError(ErrorType.TOOL_EXECUTION, error?.message || 'Unknown error', error?.details));
            }
            else {
                pendingRequest.resolve(result);
            }
        }
    }
    get isConnected() {
        return this.connections.size > 0;
    }
    get connectionCount() {
        return this.connections.size;
    }
}
