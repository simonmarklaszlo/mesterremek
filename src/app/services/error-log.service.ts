import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ErrorLogService {

  constructor() { }

  logError(message: string, stack?: string, level: string = 'error') {
    console.error(`[${level.toUpperCase()}] ${message}`, stack);
    // You can add more logging logic here, e.g., saving to file or sending to a server
  }
}
