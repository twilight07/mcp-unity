export enum ErrorType {
  CONNECTION = 'connection_error',
  TIMEOUT = 'timeout_error',
  TOOL_EXECUTION = 'tool_execution_error',
  VALIDATION = 'validation_error',
  INTERNAL = 'internal_error'
}

export class McpUnityError extends Error {
  type: ErrorType;
  details?: any;
  
  constructor(type: ErrorType, message: string, details?: any) {
    super(message);
    this.type = type;
    this.details = details;
    this.name = 'McpUnityError';
  }
  
  toJSON() {
    return {
      type: this.type,
      message: this.message,
      details: this.details
    };
  }
}

export function handleError(error: any, context: string): McpUnityError {
  if (error instanceof McpUnityError) {
    return error;
  }
  
  console.error(`${context}: ${error.message}`, error);
  
  return new McpUnityError(
    ErrorType.INTERNAL,
    `Error in ${context}: ${error.message}`,
    { originalError: error.toString() }
  );
}
