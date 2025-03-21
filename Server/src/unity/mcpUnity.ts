import { Socket } from 'net';
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

// Define response interface
interface UnityResponse {
  jsonrpc: string;
  id: string;
  result?: any;
  error?: {
    message: string;
    type: string;
    details?: any;
  };
}

export class McpUnity {
  private logger: Logger;
  private port: number;
  private socket: Socket | null = null;
  private pendingRequests: Map<string, PendingRequest> = new Map<string, PendingRequest>();
  private receivedData: string = '';
  private readonly PING_TIMEOUT = 1000; // 1 second timeout for ping
  private readonly CONNECTION_TIMEOUT = 5000; // 5 second timeout for connection
  private connecting: boolean = false;
  
  constructor(logger: Logger) {
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
  public async start(): Promise<void> {
    // Try to establish the initial connection
    try {
      await this.connect();
      this.logger.info('Successfully connected to Unity TCP server');
    } catch (error) {
      this.logger.warn('Could not connect to Unity TCP server. Will retry on next request.');
    }
    
    return Promise.resolve();
  }
  
  /**
   * Connect to the Unity server
   */
  private async connect(): Promise<void> {
    // If already connected or connecting, return or wait
    if (this.isConnected) {
      return Promise.resolve();
    }
    
    if (this.connecting) {
      return new Promise<void>((resolve, reject) => {
        const checkInterval = setInterval(() => {
          if (this.isConnected) {
            clearInterval(checkInterval);
            resolve();
          } else if (!this.connecting) {
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
      await new Promise<void>((resolve, reject) => {
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
      
    } catch (error) {
      this.disconnect();
      throw error;
    } finally {
      this.connecting = false;
    }
  }
  
  /**
   * Handle data received from Unity
   */
  private handleData(data: Buffer): void {
    // Accumulate received data
    this.receivedData += data.toString('utf8');
    
    try {
      // Parse the JSON response
      const response = JSON.parse(this.receivedData) as UnityResponse;
      
      // Reset received data after successful parse
      this.receivedData = '';
      
      // Process the response if we have a matching request
      if (response.id && this.pendingRequests.has(response.id)) {
        const request = this.pendingRequests.get(response.id)!;
        clearTimeout(request.timeout);
        this.pendingRequests.delete(response.id);
        
        if (response.error) {
          request.reject(new McpUnityError(
            ErrorType.TOOL_EXECUTION,
            response.error.message || 'Unknown error',
            response.error.details
          ));
        } else {
          request.resolve(response.result);
        }
      }
    } catch (e) {
      // Partial data, wait for more
    }
  }
  
  /**
   * Handle socket errors
   */
  private handleError(err: Error): void {
    this.logger.error(`Socket error: ${err.message}`);
    this.disconnect();
  }
  
  /**
   * Handle socket close
   */
  private handleClose(): void {
    this.logger.debug('Socket closed');
    this.disconnect();
  }
  
  /**
   * Disconnect from Unity and clean up
   */
  private disconnect(): void {
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
  public async stop(): Promise<void> {
    this.disconnect();
    this.logger.info('Unity TCP client stopped');
    return Promise.resolve();
  }
  
  /**
   * Sends a ping to Unity server to check connectivity
   */
  public async ping(): Promise<boolean> {
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
    } catch (error) {
      this.logger.debug(`Ping failed: ${error instanceof Error ? error.message : String(error)}`);
      return false;
    }
  }
  
  /**
   * Send a request to the Unity server
   */
  public async sendRequest(request: UnityRequest): Promise<any> {
    // Make sure we're connected before sending
    if (!this.isConnected) {
      try {
        await this.connect();
      } catch (error) {
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
  public get isConnected(): boolean {
    return this.socket !== null && 
           !this.socket.destroyed && 
           this.socket.readyState === 'open';
  }
}
