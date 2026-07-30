import { Injectable, inject, signal, computed } from '@angular/core';
import { ApiService } from './api.service';
import { CompanySettings } from './models';

/**
 * The distributor's own name/address/contact info, cached app-wide so the sidebar, login page and
 * browser tab title all show the same configured name without each re-fetching it. Read is
 * anonymous on the backend — the login page needs it before signing in.
 */
@Injectable({ providedIn: 'root' })
export class CompanySettingsService {
  private api = inject(ApiService);

  settings = signal<CompanySettings | null>(null);
  displayName = computed(() => this.settings()?.name || 'LPG ERP');

  load(): void {
    this.api.get<CompanySettings>('settings').subscribe({
      next: s => this.settings.set(s),
      error: () => {},
    });
  }
}
