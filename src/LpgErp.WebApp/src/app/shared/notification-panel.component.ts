import { Component, EventEmitter, Input, Output, HostListener, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationHubService, SystemNotificationDto } from '../core/notification-hub.service';

/**
 * The panel behind the bell icon. Previously the bell only zeroed a client-side counter — nothing
 * was ever persisted, so `isRead`/`readAt` on the server stayed false forever and a page refresh
 * (or a second device) would show every notification as unread again. This reads the real history
 * and calls the mark-read endpoints that already existed on the API but had no caller.
 */
@Component({
  selector: 'app-notification-panel',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (open) {
      <div class="panel" (click)="$event.stopPropagation()">
        <div class="panel-header">
          <span class="panel-title">Notifications</span>
          @if (hasUnread()) {
            <button class="mark-all-btn" (click)="onMarkAllRead()">Mark all read</button>
          }
        </div>
        <div class="panel-body">
          @for (n of hub.history(); track n.id) {
            <div class="notif-row" [class.unread]="!n.isRead" (click)="onOpen(n)">
              <span class="notif-dot" [class.show]="!n.isRead"></span>
              <div class="notif-text">
                <div class="notif-title">{{ n.title }}</div>
                <div class="notif-message">{{ n.message }}</div>
                <div class="notif-time">{{ n.createdAt | date:'dd MMM, h:mm a' }}</div>
              </div>
            </div>
          } @empty {
            <div class="notif-empty">No notifications yet.</div>
          }
        </div>
      </div>
    }
  `,
  styles: [`
    .panel {
      position: absolute;
      top: 40px;
      right: 0;
      width: 360px;
      max-height: 420px;
      display: flex;
      flex-direction: column;
      background: var(--surface, #fff);
      border: 1px solid var(--border, #e7e9ee);
      border-radius: 10px;
      box-shadow: 0 12px 32px rgba(0,0,0,0.14);
      z-index: 1000;
    }
    .panel-header {
      display: flex; justify-content: space-between; align-items: center;
      padding: 12px 16px; border-bottom: 1px solid var(--border, #e7e9ee);
    }
    .panel-title { font-weight: 700; font-size: 13px; }
    .mark-all-btn {
      border: none; background: none; color: #ea580c; font-size: 12px;
      font-weight: 600; cursor: pointer; padding: 0;
    }
    .mark-all-btn:hover { text-decoration: underline; }
    .panel-body { overflow-y: auto; }
    .notif-row {
      display: flex; gap: 10px; padding: 12px 16px; cursor: pointer;
      border-bottom: 1px solid var(--border-row, #f1f3f6);
    }
    .notif-row:hover { background: var(--fill-subtle, #fafbfc); }
    .notif-row.unread { background: #fff7ed; }
    .notif-dot {
      width: 8px; height: 8px; border-radius: 50%; margin-top: 5px; flex-shrink: 0;
      background: transparent;
    }
    .notif-dot.show { background: #ea580c; }
    .notif-text { min-width: 0; }
    .notif-title { font-weight: 600; font-size: 13px; }
    .notif-message { font-size: 12px; color: var(--text-secondary, #374151); margin-top: 2px; }
    .notif-time { font-size: 11px; color: var(--text-muted, #6b7280); margin-top: 4px; }
    .notif-empty { padding: 32px 16px; text-align: center; color: var(--text-muted, #6b7280); font-size: 13px; }
  `],
})
export class NotificationPanelComponent {
  hub = inject(NotificationHubService);
  private elementRef = inject(ElementRef);

  @Input() open = false;
  @Output() openChange = new EventEmitter<boolean>();

  hasUnread(): boolean {
    return this.hub.history().some(n => !n.isRead);
  }

  onOpen(n: SystemNotificationDto) {
    if (!n.isRead) this.hub.markAsRead(n.id);
  }

  onMarkAllRead() {
    this.hub.markAllAsRead();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (this.open && !this.elementRef.nativeElement.contains(event.target)) {
      this.openChange.emit(false);
    }
  }
}
