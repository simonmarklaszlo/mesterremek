import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonHeader, IonToolbar, IonTitle, IonContent, IonButtons, IonMenu, IonMenuButton } from '@ionic/angular/standalone';
import { ErrorLogService, AppLogEntry } from '../services/error-log.service';

@Component({
  selector: 'app-tab2',
  standalone: true,
  templateUrl: 'tab2.page.html',
  styleUrls: ['tab2.page.scss'],
  imports: [CommonModule, IonHeader, IonToolbar, IonTitle, IonContent, IonButtons, IonMenu, IonMenuButton]
})
export class Tab2Page implements OnInit, OnDestroy {
  logs: AppLogEntry[] = [];

  constructor(private errorLog: ErrorLogService) {}

  private onWindowError = (event: ErrorEvent) => {
    const msg = typeof event.message === 'string' ? event.message : String(event.message);
    this.errorLog.logError(msg, event.error?.stack, 'error');
  };

  private onUnhandledRejection = (event: PromiseRejectionEvent) => {
    const reason: any = event.reason;
    const msg = typeof reason === 'string' ? reason : (reason?.message || JSON.stringify(reason));
    const stack = reason?.stack;
    this.errorLog.logError(msg, stack, 'rejection');
  };

  ngOnInit(): void {
    window.addEventListener('error', this.onWindowError);
    window.addEventListener('unhandledrejection', this.onUnhandledRejection);
    this.errorLog.logs$.subscribe(entries => this.logs = entries);
  }

  ngOnDestroy(): void {
    window.removeEventListener('error', this.onWindowError);
    window.removeEventListener('unhandledrejection', this.onUnhandledRejection);
  }

  throwTestError(): void {
    // Intentionally throw to test window.onerror
    throw new Error('Test error from Tab2Page');
  }

  rejectTestPromise(): void {
    // Intentionally unhandled rejection
    Promise.reject(new Error('Test unhandled rejection from Tab2Page'));
  }

  clearLogs(): void {
    this.errorLog.clear();
  }
}
