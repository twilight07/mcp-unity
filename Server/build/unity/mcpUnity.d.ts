import { Logger } from '../utils/logger.js';
interface UnityRequest {
    id?: string;
    method: string;
    params: any;
}
export declare class McpUnity {
    private logger;
    private port;
    private ws;
    private pendingRequests;
    private readonly REQUEST_TIMEOUT;
    private lastPongTime;
    private pingInterval;
    private readonly PING_INTERVAL;
    private readonly PONG_TIMEOUT;
    constructor(logger: Logger);
    /**
     * Start the Unity connection
     */
    start(): Promise<void>;
    /**
     * Connect to the Unity WebSocket
     */
    private connect;
    /**
     * Start ping interval to keep connection alive and detect disconnections
     */
    private startPingInterval;
    /**
     * Stop ping interval
     */
    private stopPingInterval;
    /**
     * Handle messages received from Unity
     */
    private handleMessage;
    /**
     * Disconnect from Unity
     */
    private disconnect;
    /**
     * Stop the Unity connection
     */
    stop(): Promise<void>;
    /**
     * Send a request to the Unity server
     */
    sendRequest(request: UnityRequest): Promise<any>;
    /**
     * Check if connected to Unity
     * Only returns true if the connection is guaranteed to be active
     */
    get isConnected(): boolean;
}
export {};
