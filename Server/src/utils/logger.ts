export enum LogLevel {
  DEBUG = 0,
  INFO = 1,
  WARN = 2,
  ERROR = 3
}

export class Logger {
  private level: LogLevel;
  private prefix: string;
  
  constructor(prefix: string, level: LogLevel = LogLevel.INFO) {
    this.prefix = prefix;
    this.level = level;
  }
  
  debug(message: string, data?: any) {
    this.log(LogLevel.DEBUG, message, data);
  }
  
  info(message: string, data?: any) {
    this.log(LogLevel.INFO, message, data);
  }
  
  warn(message: string, data?: any) {
    this.log(LogLevel.WARN, message, data);
  }
  
  error(message: string, error?: any) {
    this.log(LogLevel.ERROR, message, error);
  }
  
  private log(level: LogLevel, message: string, data?: any) {
    if (level < this.level) return;
    
    const timestamp = new Date().toISOString();
    const levelStr = LogLevel[level];
    const logMessage = `[${timestamp}] [${levelStr}] [${this.prefix}] ${message}`;
    
    if (data) {
      console.log(logMessage, data);
    } else {
      console.log(logMessage);
    }
  }
}
