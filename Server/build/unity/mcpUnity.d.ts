import { Logger } from '../utils/logger.js';
interface UnityRequest {
    id?: string;
    method: string;
    params: any;
}
export declare class McpUnity {
    private ws;
    private connections;
    private pendingRequests;
    private logger;
    private port;
    constructor(logger: Logger);
    start(): Promise<void>;
    stop(): Promise<void>;
    sendRequest(request: UnityRequest): Promise<any>;
    private handleMessage;
    get isConnected(): boolean;
    get connectionCount(): number;
}
export {};
