import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface AppLogEntry {
  time: string;
  level: 'error' | 'rejection' | 'warn' | 'info';
  message: string;
  stack?: string;
}

@Injectable({ providedIn: 'root' })
export class ErrorLogService {
  private logs: AppLogEntry[] = [];
  private subject = new BehaviorSubject<AppLogEntry[]>([]);
  readonly logs$ = this.subject.asObservable();

  logError(message: string, stack?: string, level: AppLogEntry['level'] = 'error'): void {
    const time = new Date().toLocaleTimeString();
    this.logs = [{ time, level, message, stack }, ...this.logs].slice(0, 200);
    this.subject.next(this.logs);
  }

  clear(): void {
    this.logs = [];
    this.subject.next(this.logs);
  }
}
