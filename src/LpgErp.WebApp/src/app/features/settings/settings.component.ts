import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { CompanySettingsService } from '../../core/company-settings.service';
import { CompanySettings } from '../../core/models';

/**
 * The distributor's own business profile — shown across the app (sidebar, login page, browser tab)
 * via CompanySettingsService, edited here. Admin-only, gated by roleGuard('Admin') on the route.
 */
@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Company Settings</h1>
        <span class="page-sub">Shown across the app — sidebar, login page, and browser tab</span>
      </div>
    </div>

    @if (loading()) {
      <div class="state-note">Loading…</div>
    } @else {
      <form class="form-card" (ngSubmit)="save()">
        <div class="form-group">
          <label for="name">Company / Shop Name</label>
          <input id="name" type="text" [(ngModel)]="name" name="name" required />
        </div>
        <div class="form-group">
          <label for="address">Address</label>
          <input id="address" type="text" [(ngModel)]="address" name="address" placeholder="Street, area, city" />
        </div>
        <div class="form-group">
          <label for="phone">Phone</label>
          <input id="phone" type="text" [(ngModel)]="phone" name="phone" placeholder="01XXXXXXXXX" />
        </div>
        <div class="form-group">
          <label for="email">Email</label>
          <input id="email" type="text" [(ngModel)]="email" name="email" placeholder="contact@company.com" />
        </div>
        <div class="form-group">
          <label for="website">Website</label>
          <input id="website" type="text" [(ngModel)]="website" name="website" placeholder="https://" />
        </div>

        @if (error()) { <p class="error">{{ error() }}</p> }
        @if (saved()) { <p class="success">Saved.</p> }

        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="saving()">{{ saving() ? 'Saving...' : 'Save' }}</button>
        </div>
      </form>
    }
  `,
  styles: [`
    .page-header { margin-bottom: 20px; }
    .page-title { font-size: 22px; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 0; }
    .page-sub { font-size: 12px; color: var(--text-muted); }
    .state-note { padding: 24px; color: #6b7280; }

    .form-card { max-width: 480px; background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); padding: 24px; }
    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; margin-bottom: 0.25rem; font-weight: 600; font-size: 0.9rem; color: var(--text-primary); }
    .form-group input { width: 100%; padding: 0.55rem 0.7rem; border: 1px solid var(--border-input, #ddd); border-radius: 6px; box-sizing: border-box; font-size: 0.9rem; }
    .form-actions { display: flex; justify-content: flex-end; margin-top: 1.5rem; }
    .btn { padding: 0.55rem 1.2rem; border-radius: 6px; cursor: pointer; border: none; font-size: 0.9rem; font-weight: 600; }
    .btn-primary { background: var(--primary, #ea580c); color: #fff; }
    .btn-primary:disabled { opacity: 0.6; cursor: default; }
    .error { color: #dc3545; font-size: 0.85rem; }
    .success { color: #15803d; font-size: 0.85rem; }
  `],
})
export class SettingsComponent implements OnInit {
  private api = inject(ApiService);
  private companySettingsService = inject(CompanySettingsService);

  loading = signal(true);
  saving = signal(false);
  error = signal('');
  saved = signal(false);

  id = '';
  name = '';
  address = '';
  phone = '';
  email = '';
  website = '';

  ngOnInit() {
    this.api.get<CompanySettings>('settings').subscribe({
      next: s => {
        this.id = s.id;
        this.name = s.name ?? '';
        this.address = s.address ?? '';
        this.phone = s.phone ?? '';
        this.email = s.email ?? '';
        this.website = s.website ?? '';
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load company settings.');
        this.loading.set(false);
      },
    });
  }

  save() {
    this.saving.set(true);
    this.error.set('');
    this.saved.set(false);

    const body = {
      name: this.name,
      address: this.address || null,
      phone: this.phone || null,
      email: this.email || null,
      website: this.website || null,
    };

    this.api.put<CompanySettings>('settings', body).subscribe({
      next: s => {
        this.saving.set(false);
        this.saved.set(true);
        this.companySettingsService.settings.set(s);
      },
      error: (e) => {
        this.saving.set(false);
        this.error.set(e?.error?.errors?.[0] ?? e?.error?.message ?? 'Could not save settings.');
      },
    });
  }
}
