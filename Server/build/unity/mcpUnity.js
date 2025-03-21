import axios from 'axios';
import { v4 as uuidv4 } from 'uuid';
import { McpUnityError, ErrorType } from '../utils/errors.js';
export class McpUnity {
    logger;
    port;
    unityUrl;
    isUnityAvailable = false;
    pendingRequests = new Map();
    constructor(logger) {
        this.logger = logger;
        // Initialize port from environment variable or use default
        const envPort = process.env.UNITY_PORT;
        this.port = envPort ? parseInt(envPort, 10) : 8090;
        // Set Unity server URL
        this.unityUrl = `http://localhost:${this.port}`;
        // Log the port being used
        this.logger.info(`Using port: ${this.port} for Unity HTTP server at ${this.unityUrl}`);
    }
    async start() {
        // Check if Unity HTTP server is available
        try {
            await this.checkUnityConnection();
            this.isUnityAvailable = true;
            this.logger.info('Successfully connected to Unity HTTP server');
        }
        catch (error) {
            this.isUnityAvailable = false;
            this.logger.warn('Could not connect to Unity HTTP server. Will retry on next request.');
        }
        return Promise.resolve();
    }
    async checkUnityConnection() {
        try {
            await axios.get(`${this.unityUrl}/health`, { timeout: 1000 });
            return Promise.resolve();
        }
        catch (error) {
            return Promise.reject(new McpUnityError(ErrorType.CONNECTION, 'Unity HTTP server not available'));
        }
    }
    async stop() {
        // Nothing to clean up for HTTP client
        this.logger.info('Unity HTTP client stopped');
        return Promise.resolve();
    }
    async sendRequest(request) {
        // If we haven't successfully connected yet, try to check connection first
        if (!this.isUnityAvailable) {
            try {
                await this.checkUnityConnection();
                this.isUnityAvailable = true;
            }
            catch (error) {
                throw new McpUnityError(ErrorType.CONNECTION, 'No Unity connections available');
            }
        }
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
            // Send the request to Unity via HTTP
            this.logger.debug(`Sending request to Unity: ${JSON.stringify(message)}`);
            axios.post(this.unityUrl, message, {
                headers: { 'Content-Type': 'application/json' },
                timeout: 5000
            })
                .then((response) => {
                clearTimeout(timeout);
                this.pendingRequests.delete(requestId);
                const responseData = response.data;
                if (responseData.type === 'error') {
                    reject(new McpUnityError(ErrorType.TOOL_EXECUTION, responseData.error?.message || 'Unknown error', responseData.error?.details));
                }
                else {
                    resolve(responseData.result);
                }
            })
                .catch((error) => {
                clearTimeout(timeout);
                this.pendingRequests.delete(requestId);
                if (error.code === 'ECONNREFUSED' || error.code === 'ECONNABORTED') {
                    this.isUnityAvailable = false;
                    reject(new McpUnityError(ErrorType.CONNECTION, 'Unity HTTP server not available'));
                }
                else {
                    reject(new McpUnityError(ErrorType.CONNECTION, `Failed to send request: ${error.message}`));
                }
            });
        });
    }
    get isConnected() {
        return this.isUnityAvailable;
    }
    get connectionCount() {
        return this.isUnityAvailable ? 1 : 0;
    }
}
