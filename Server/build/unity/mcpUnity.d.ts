import { Logger } from '../utils/logger.js';
interface UnityRequest {
    id?: string;
    method: string;
    params: any;
}
export declare class McpUnity {
    private logger;
    private port;
    private unityUrl;
    private isUnityAvailable;
    private pendingRequests;
    constructor(logger: Logger);
    start(): Promise<void>;
    checkUnityConnection(): Promise<void>;
    stop(): Promise<void>;
    sendRequest(request: UnityRequest): Promise<any>;
    get isConnected(): boolean;
    get connectionCount(): number;
}
export {};
