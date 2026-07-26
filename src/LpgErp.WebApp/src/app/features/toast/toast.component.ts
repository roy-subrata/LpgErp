import { Component, inject, effect, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationHubService, RealTimeNotification } from '../../core/notification-hub.service';

interface ToastItem {
  notification: RealTimeNotification;
  id: number;
}

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container">
      @for (toast of visibleToasts(); track toast.id) {
        <div class="toast" [class]="'toast-' + (toast.notification.severity || 'info')">
          <div class="toast-icon">{{ getIcon(toast.notification.severity) }}</div>
          <div class="toast-body">
            <div class="toast-title">{{ toast.notification.title }}</div>
            <div class="toast-message">{{ toast.notification.message }}</div>
          </div>
          <button class="toast-close" (click)="dismiss(toast.id)">&#x2715;</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 72px;
      right: 24px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 8px;
      max-width: 380px;
    }

    .toast {
      display: flex;
      align-items: flex-start;
      gap: 10px;
      padding: 12px 14px;
      border-radius: 10px;
      background: #1e293b;
      border: 1px solid #334155;
      box-shadow: 0 8px 24px rgba(0,0,0,0.4);
      animation: slideIn 0.25s ease-out;
    }

    .toast-info { border-left: 3px solid #3b82f6; }
    .toast-success { border-left: 3px solid #22c55e; }
    .toast-warning { border-left: 3px solid #f59e0b; }
    .toast-danger { border-left: 3px solid #ef4444; }

    .toast-icon { font-size: 16px; margin-top: 1px; }
    .toast-body { flex: 1; min-width: 0; }
    .toast-title { font-size: 13px; font-weight: 600; color: #f8fafc; }
    .toast-message { font-size: 12px; color: #94a3b8; margin-top: 2px; word-wrap: break-word; }

    .toast-close {
      background: none;
      border: none;
      color: #64748b;
      cursor: pointer;
      font-size: 12px;
      padding: 2px;
      line-height: 1;
    }
    .toast-close:hover { color: #f8fafc; }

    @keyframes slideIn {
      from { opacity: 0; transform: translateX(40px); }
      to { opacity: 1; transform: translateX(0); }
    }
  `]
})
export class ToastComponent {
  private notificationService = inject(NotificationHubService);
  private nextId = 0;
  private toastMap = new Map<number, { notification: RealTimeNotification; addedAt: number }>();
  private dismissedIds = new Set<number>();

  visibleToasts = signal<ToastItem[]>([]);

  constructor() {
    effect(() => {
      const all = this.notificationService.notifications();
      const cutoff = Date.now() - 8000;

      for (const n of all) {
        if (!this.toastMap.has(n.timestamp as any)) {
          const id = this.nextId++;
          this.toastMap.set(id, { notification: n, addedAt: Date.now() });
        }
      }

      const entries: ToastItem[] = [];
      for (const [id, entry] of this.toastMap) {
        if (entry.addedAt > cutoff && !this.dismissedIds.has(id)) {
          entries.push({ notification: entry.notification, id });
        }
        if (entry.addedAt < cutoff) {
          this.toastMap.delete(id);
          this.dismissedIds.delete(id);
        }
      }
      this.visibleToasts.set(entries.slice(0, 4));
    });
  }

  getIcon(severity?: string): string {
    switch (severity) {
      case 'success': return '\u2713';
      case 'warning': return '\u26A0';
      case 'danger': return '\u2715';
      default: return '\u2139';
    }
  }

  dismiss(id: number) {
    this.dismissedIds.add(id);
    this.visibleToasts.update(toasts => toasts.filter(t => t.id !== id));
  }
}
