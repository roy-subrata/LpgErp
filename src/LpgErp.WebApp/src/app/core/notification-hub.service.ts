import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { environment } from 'src/environments/environment';

export interface RealTimeNotification {
  type: string;
  title: string;
  message: string;
  entityId?: string;
  entityType?: string;
  timestamp: string;
  severity?: string;
  targetRoles?: string[];
}

export interface SystemNotificationDto {
  id: string;
  type: string;
  title: string;
  message: string;
  entityId?: string;
  entityType?: string;
  severity?: string;
  targetRoles: string;
  isRead: boolean;
  readAt?: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationHubService {
  private hubConnection: signalR.HubConnection | null = null;
  private baseUrl = environment.apiUrl;
  notifications = signal<RealTimeNotification[]>([]);
  unreadCount = signal(0);
  history = signal<SystemNotificationDto[]>([]);

  constructor(private authService: AuthService, private http: HttpClient) {}

  start(): void {
    const token = this.authService.getToken();
    if (!token || this.hubConnection?.state === signalR.HubConnectionState.Connected) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl.replace('/api/v1', '')}/hubs/notifications`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: RealTimeNotification) => {
      if (!this.matchesUserRoles(notification)) return;
      this.notifications.update(n => [notification, ...n].slice(0, 50));
      this.unreadCount.update(c => c + 1);
    });

    this.hubConnection.onclose(() => {
      setTimeout(() => this.start(), 5000);
    });

    this.hubConnection.start().catch(err => {
      console.error('SignalR connection error:', err);
      setTimeout(() => this.start(), 5000);
    });

    this.loadHistory();
  }

  stop(): void {
    this.hubConnection?.stop();
    this.hubConnection = null;
  }

  loadHistory(): void {
    this.http.get<{ success: boolean; data: SystemNotificationDto[] }>(
      `${this.baseUrl}/SystemNotifications?pageSize=50`
    ).subscribe({
      next: res => {
        if (res.success) {
          const filtered = res.data.filter(n => this.matchesHistoryNotification(n));
          this.history.set(filtered);
          // The badge reflects what the server actually has marked unread, not just live pushes
          // received since connecting — otherwise a refresh (or a second device) loses the count.
          this.unreadCount.set(filtered.filter(n => !n.isRead).length);
        }
      },
      error: () => {}
    });
  }

  markAsRead(id: string): void {
    this.http.post(`${this.baseUrl}/SystemNotifications/${id}/read`, {}).subscribe({
      next: () => {
        this.history.update(items =>
          items.map(n => n.id === id ? { ...n, isRead: true, readAt: new Date().toISOString() } : n));
        this.unreadCount.update(c => Math.max(0, c - 1));
      },
      error: () => {}
    });
  }

  markAllAsRead(): void {
    this.http.post(`${this.baseUrl}/SystemNotifications/read-all`, {}).subscribe({
      next: () => {
        this.history.update(items => items.map(n => ({ ...n, isRead: true, readAt: new Date().toISOString() })));
        this.unreadCount.set(0);
      },
      error: () => {}
    });
  }

  private matchesUserRoles(notification: RealTimeNotification): boolean {
    if (!notification.targetRoles || notification.targetRoles.length === 0) return true;
    const user = this.authService.currentUser();
    if (!user) return false;
    return notification.targetRoles.some(role => user.roles.includes(role));
  }

  private matchesHistoryNotification(notification: SystemNotificationDto): boolean {
    if (!notification.targetRoles) return true;
    try {
      const roles: string[] = JSON.parse(notification.targetRoles);
      if (roles.length === 0) return true;
      const user = this.authService.currentUser();
      if (!user) return false;
      return roles.some(role => user.roles.includes(role));
    } catch {
      return true;
    }
  }
}
