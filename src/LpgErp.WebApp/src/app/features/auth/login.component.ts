import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="login-page">
      <div class="login-card">
        <div class="login-header">
          <div class="brand-logo">L</div>
          <h1>LPG ERP</h1>
          <p>Distributor Management Suite</p>
        </div>

        @if (error()) {
          <div class="error-banner">{{ error() }}</div>
        }

        <form (ngSubmit)="onLogin()">
          <div class="field">
            <label>Username</label>
            <input type="text" [(ngModel)]="username" name="username" placeholder="Enter username" autofocus />
          </div>
          <div class="field">
            <label>Password</label>
            <input type="password" [(ngModel)]="password" name="password" placeholder="Enter password" />
          </div>
          <button type="submit" class="login-btn" [disabled]="loading()">
            {{ loading() ? 'Signing in...' : 'Sign In' }}
          </button>
        </form>

        <div class="login-footer">
          <span>Demo: admin / admin123</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .login-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
    }

    .login-card {
      width: 380px;
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 16px;
      padding: 40px 32px 32px;
    }

    .login-header {
      text-align: center;
      margin-bottom: 32px;
    }

    .brand-logo {
      width: 56px;
      height: 56px;
      border-radius: 14px;
      background: linear-gradient(135deg, #f97316, #ea580c);
      display: flex;
      align-items: center;
      justify-content: center;
      color: #fff;
      font-weight: 800;
      font-size: 24px;
      margin: 0 auto 16px;
    }

    .login-header h1 {
      font-size: 22px;
      font-weight: 700;
      color: #f8fafc;
      margin: 0 0 4px;
    }

    .login-header p {
      font-size: 13px;
      color: #94a3b8;
      margin: 0;
    }

    .error-banner {
      background: rgba(239, 68, 68, 0.12);
      border: 1px solid rgba(239, 68, 68, 0.3);
      color: #fca5a5;
      border-radius: 8px;
      padding: 10px 14px;
      font-size: 13px;
      margin-bottom: 20px;
    }

    .field {
      margin-bottom: 18px;
    }

    .field label {
      display: block;
      font-size: 12px;
      font-weight: 600;
      color: #94a3b8;
      margin-bottom: 6px;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .field input {
      width: 100%;
      padding: 10px 14px;
      border-radius: 8px;
      border: 1px solid #334155;
      background: #0f172a;
      color: #f8fafc;
      font-size: 14px;
      outline: none;
      box-sizing: border-box;
      transition: border-color 0.15s;
    }

    .field input:focus {
      border-color: #f97316;
    }

    .field input::placeholder {
      color: #475569;
    }

    .login-btn {
      width: 100%;
      padding: 11px;
      border-radius: 8px;
      border: none;
      background: linear-gradient(135deg, #f97316, #ea580c);
      color: #fff;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      transition: opacity 0.15s;
    }

    .login-btn:hover {
      opacity: 0.9;
    }

    .login-btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .login-footer {
      text-align: center;
      margin-top: 24px;
      font-size: 12px;
      color: #64748b;
    }
  `]
})
export class LoginComponent {
  username = '';
  password = '';
  loading = signal(false);
  error = signal('');

  constructor(private authService: AuthService, private router: Router) {}

  onLogin(): void {
    if (!this.username || !this.password) {
      this.error.set('Please enter username and password');
      return;
    }

    this.loading.set(true);
    this.error.set('');

    this.authService.login({ username: this.username, password: this.password }).subscribe({
      next: (result) => {
        this.loading.set(false);
        if (result.isSuccess) {
          this.router.navigate(['/dashboard']);
        } else {
          this.error.set(result.error || 'Login failed');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.error.set(this.messageFor(err));
      }
    });
  }

  private messageFor(err: HttpErrorResponse): string {
    const fromApi = err.error?.errors?.[0];
    if (fromApi) return fromApi;

    switch (err.status) {
      case 429: return 'Too many login attempts. Please wait a minute and try again.';
      case 0: return 'Cannot reach the server. Check that the API is running.';
      default: return 'Invalid credentials';
    }
  }
}
