import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { VehicleLoading } from '../../core/models';

@Component({
  selector: 'app-vehicle-loading-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="page-header">
      <div>
        <a routerLink="/vehicle-loadings" class="back-link">← Vehicle Loadings</a>
        <h1 class="page-title">{{ loading()?.truckName || 'Loading' }}</h1>
        <p class="page-subtitle">
          {{ loading()?.loadingDate | date:'fullDate' }} ·
          <span class="status-chip" [ngClass]="'st-' + loading()?.status">{{ statusLabel(loading()?.status) }}</span>
        </p>
      </div>
    </div>

    @if (loading(); as vl) {
      <div class="detail-grid">
        <div class="panel">
          <div class="panel-title">Assignment</div>
          <div class="kv"><span>Salesman</span><b>{{ vl.salesmanName }}</b></div>
          <div class="kv"><span>Driver</span><b>{{ vl.driverName }}</b></div>
          <div class="kv"><span>Warehouse</span><b>{{ vl.warehouseName }}</b></div>
          <div class="kv"><span>Route</span><b>{{ $any(vl).routeName || '—' }}</b></div>
          <div class="kv"><span>Notes</span><b>{{ vl.notes || '—' }}</b></div>
        </div>

        <div class="panel">
          <div class="panel-title">Load Reconciliation</div>
          <table class="mini-table">
            <thead><tr><th>Product</th><th class="num">Loaded</th><th class="num">Sold</th><th class="num">Returned</th><th class="num">Damaged</th></tr></thead>
            <tbody>
              @for (it of vl.items; track it.id) {
                <tr>
                  <td>{{ it.productName }}</td>
                  <td class="num">{{ it.loadedQuantity }}</td>
                  <td class="num">{{ it.soldQuantity }}</td>
                  <td class="num">{{ it.returnedQuantity }}</td>
                  <td class="num">{{ it.damagedQuantity }}</td>
                </tr>
              }
            </tbody>
            <tfoot>
              <tr>
                <td>Total</td>
                <td class="num">{{ total('loadedQuantity') }}</td>
                <td class="num">{{ total('soldQuantity') }}</td>
                <td class="num">{{ total('returnedQuantity') }}</td>
                <td class="num">{{ total('damagedQuantity') }}</td>
              </tr>
            </tfoot>
          </table>
        </div>

        @if (closing(); as c) {
          <div class="panel">
            <div class="panel-title">Closing — {{ c.closingDate | date:'medium' }}</div>
            <div class="stat-row">
              <div class="stat"><span>Cash</span><b class="green">৳ {{ c.cashCollected | number }}</b></div>
              <div class="stat"><span>Credit</span><b class="amber">৳ {{ c.creditSales | number }}</b></div>
              <div class="stat"><span>Outstanding</span><b>৳ {{ c.outstandingAmount | number }}</b></div>
              <div class="stat"><span>Empties</span><b>{{ c.returnedEmptyCylinders }}</b></div>
              <div class="stat"><span>Exchanges</span><b>{{ c.cylinderExchanges }}</b></div>
              <div class="stat"><span>Damaged</span><b>{{ c.damagedCount }}</b></div>
              <div class="stat"><span>Leakage</span><b>{{ c.leakageCount }}</b></div>
              <div class="stat"><span>Variance</span><b>{{ c.variance }}</b></div>
            </div>
            @if (c.notes) { <p class="closing-notes">{{ c.notes }}</p> }
          </div>
        } @else if (vl.status !== 2) {
          <div class="panel muted-panel">Vehicle not yet closed — close it from the Vehicle Loadings page.</div>
        }
      </div>
    } @else {
      <p class="loading-msg">Loading…</p>
    }
  `,
  styles: [`
    :host { display: block; }
    .back-link { font-size: 12px; color: var(--text-muted); text-decoration: none; }
    .back-link:hover { color: var(--primary); }
    .page-header { margin-bottom: 18px; }
    .page-title { font-size: 22px; font-weight: 800; color: var(--text-primary); margin: 4px 0 0; }
    .page-subtitle { font-size: 13px; color: var(--text-muted); margin: 4px 0 0; }
    .status-chip { padding: 2px 10px; border-radius: 10px; font-size: 11px; font-weight: 600; }
    .st-0 { background: #fefce8; color: #a16207; }
    .st-1 { background: #f0fdf4; color: #15803d; }
    .st-2 { background: #eff6ff; color: #1d4ed8; }
    .detail-grid { display: grid; grid-template-columns: 320px 1fr; gap: 16px; align-items: start; }
    .panel { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); padding: 16px; }
    .panel-title { font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.05em; color: var(--text-muted); margin-bottom: 12px; }
    .kv { display: flex; justify-content: space-between; padding: 6px 0; font-size: 13px; border-bottom: 1px solid var(--border-row); }
    .kv span { color: var(--text-muted); }
    .kv b { color: var(--text-primary); font-weight: 600; }
    .mini-table { width: 100%; border-collapse: collapse; font-size: 13px; }
    .mini-table th { text-align: left; font-size: 11px; text-transform: uppercase; color: var(--text-muted); padding: 6px 8px; border-bottom: 1px solid var(--border); }
    .mini-table td { padding: 7px 8px; border-bottom: 1px solid var(--border-row); color: var(--text-secondary); }
    .mini-table tfoot td { font-weight: 700; color: var(--text-primary); border-top: 2px solid var(--border); }
    .num { text-align: right; font-family: var(--font-mono); }
    .stat-row { display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px; }
    .stat { display: flex; flex-direction: column; gap: 2px; background: var(--fill-subtle); border-radius: 8px; padding: 10px 12px; }
    .stat span { font-size: 10px; text-transform: uppercase; color: var(--text-faint); letter-spacing: 0.04em; }
    .stat b { font-size: 15px; color: var(--text-primary); }
    .green { color: var(--green-fg) !important; }
    .amber { color: var(--amber-fg2) !important; }
    .closing-notes { margin: 12px 0 0; font-size: 12px; color: var(--text-muted); font-style: italic; }
    .muted-panel { color: var(--text-muted); font-size: 13px; }
    .loading-msg { color: var(--text-muted); }
    @media (max-width: 900px) { .detail-grid { grid-template-columns: 1fr; } .stat-row { grid-template-columns: repeat(2, 1fr); } }
  `],
})
export class VehicleLoadingDetailComponent implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);

  loading = signal<VehicleLoading | null>(null);
  closing = signal<any | null>(null);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.api.getById<VehicleLoading>('vehicleloadings', id).subscribe(vl => this.loading.set(vl));
    this.api.getAll<any>('vehicleclosings', 1, 100).subscribe(page =>
      this.closing.set(page.items.find((c: any) => c.vehicleLoadingId === id) ?? null));
  }

  total(key: 'loadedQuantity' | 'soldQuantity' | 'returnedQuantity' | 'damagedQuantity'): number {
    return (this.loading()?.items || []).reduce((s, i) => s + (i[key] || 0), 0);
  }

  statusLabel(status?: number): string {
    const map: Record<number, string> = { 0: 'Dispatched', 1: 'In Transit', 2: 'Returned' };
    return status !== undefined ? (map[status] ?? '—') : '—';
  }
}
