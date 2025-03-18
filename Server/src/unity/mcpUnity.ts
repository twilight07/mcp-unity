import { WebSocketServer, WebSocket } from 'ws';
import { v4 as uuidv4 } from 'uuid';
import { Logger } from '../utils/logger.js';
import { McpUnityError, ErrorType } from '../utils/errors.js';

interface PendingRequest {
  resolve: (value: any) => void;
  reject: (reason: any) => void;
  timeout: NodeJS.Timeout;
}

interface UnityRequest {
  id?: string;
  method: string;
  params: any;
}

export class McpUnity {
  private ws: WebSocketServer | null = null;
  private connections: Map<string, WebSocket> = new Map<string, WebSocket>();
  private pendingRequests: Map<string, PendingRequest> = new Map<string, PendingRequest>();
  private logger: Logger;
  private port: number;
  
  constructor(logger: Logger) {
    this.logger = logger;
    
    // Initialize port from environment variable or use default
    const envPort = process.env.UNITY_PORT;
    this.port = envPort ? parseInt(envPort, 10) : 8090;
    
    // Log the port being used
    this.logger.info(`Using port: ${this.port}`);
  }
  
  public async start(): Promise<void> {
    if (this.ws) {
      this.logger.warn('MCP Node WebSocket Connection already started');
      return;
    }
    
    return new Promise((resolve) => {
      this.ws = new WebSocketServer({ port: this.port });
      
      this.ws.on('listening', () => {
        this.logger.info(`MCP Node WebSocket listening on port ${this.port}`);
        resolve();
      });
      
      this.ws.on('connection', (socket: WebSocket) => {
        const id = uuidv4();
        this.connections.set(id, socket);
        this.logger.info(`MCP Node WebSocket Connected: ${id}`);
        
        socket.on('message', (data: WebSocket.Data) => {
          const message = JSON.parse(data.toString());
          this.handleMessage(message);
        });
        
        socket.on('close', () => {
          this.connections.delete(id);
          this.logger.info(`MCP Node WebSocket disconnected: ${id}`);
        });
        
        socket.on('error', (error: Error) => {
          this.logger.error(`MCP Node WebSocket error for connection ${id}`, error);
        });
      });
      
      this.ws.on('error', (error: Error) => {
        this.logger.error('MCP Node WebSocket server error', error);
      });
    });
  }
  
  public async stop(): Promise<void> {
    if (!this.ws) {
      return;
    }
    
    return new Promise((resolve) => {
      this.ws!.close(() => {
        this.logger.info('Unity bridge stopped');
        this.ws = null;
        this.connections.clear();
        resolve();
      });
    });
  }
  
  public async sendRequest(request: UnityRequest): Promise<any> {
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
      socket.send(JSON.stringify(message), (error: Error | undefined) => {
        if (error) {
          clearTimeout(timeout);
          this.pendingRequests.delete(requestId);
          reject(new McpUnityError(ErrorType.CONNECTION, `Failed to send request: ${error.message}`));
        } else {
          this.logger.debug(`Sent request to Unity: ${JSON.stringify(message)}`);
        }
      });
    });
  }
  
  private handleMessage(message: any): void {
    const { id, type, result, error } = message;
    
    if (!id || !type) {
      this.logger.warn('Received message with missing id or type', message);
      return;
    }
    
    this.logger.debug(`Received message: ${JSON.stringify(message)}`);
    
    if ((type === 'response' || type === 'error') && this.pendingRequests.has(id)) {
      const pendingRequest = this.pendingRequests.get(id)!;
      this.pendingRequests.delete(id);
      clearTimeout(pendingRequest.timeout);
      
      if (type === 'error') {
        pendingRequest.reject(new McpUnityError(
          ErrorType.TOOL_EXECUTION,
          error?.message || 'Unknown error',
          error?.details
        ));
      } else {
        pendingRequest.resolve(result);
      }
    }
  }
  
  public get isConnected(): boolean {
    return this.connections.size > 0;
  }
  
  public get connectionCount(): number {
    return this.connections.size;
  }
}
