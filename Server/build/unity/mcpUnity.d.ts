import { Logger } from '../utils/logger.js';
interface UnityRequest {
    id?: string;
    method: string;
    params: any;
}
export declare class McpUnity {
    private logger;
    private port;
    private socket;
    private pendingRequests;
    private receivedData;
    private readonly PING_TIMEOUT;
    private readonly CONNECTION_TIMEOUT;
    private connecting;
    constructor(logger: Logger);
    /**
     * Start the Unity connection
     */
    start(): Promise<void>;
    /**
     * Connect to the Unity server
     */
    private connect;
    /**
     * Handle data received from Unity
     */
    private handleData;
    /**
     * Handle socket errors
     */
    private handleError;
    /**
     * Handle socket close
     */
    private handleClose;
    /**
     * Disconnect from Unity and clean up
     */
    private disconnect;
    /**
     * Stop the Unity connection
     */
    stop(): Promise<void>;
    /**
     * Sends a ping to Unity server to check connectivity
     */
    ping(): Promise<boolean>;
    /**
     * Send a request to the Unity server
     */
    sendRequest(request: UnityRequest): Promise<any>;
    /**
     * Check if connected to Unity
     */
    get isConnected(): boolean;
}
export {};
