import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { VehicleLoading, Truck, Driver, Salesman, Route, Product } from '../../core/models';

interface LoadItemRow {
  productId: string;
  quantity: number;
}

@Component({
  selector: 'app-vehicle-loading-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Vehicle Loadings</h1>
        <p class="page-subtitle">{{ todayDate }}</p>
      </div>
      <div class="header-actions">
        <button class="btn-secondary" routerLink="/loading-history">◷ History</button>
        <button class="btn-primary" (click)="drawerOpen.set(true)">+ New Loading</button>
      </div>
    </div>

    <div class="card-grid">
      @for (card of displayCards(); track card.id) {
        <div class="loading-card">
          <div class="card-header">
            <div class="card-header-top">
              <span class="truck-icon" [style.background]="card.iconBg">🚛</span>
              <span class="plate-number">{{ card.plate }}</span>
              <span class="status-badge" [style.background]="card.statusBg" [style.color]="card.statusColor">{{ card.status }}</span>
              <button class="menu-btn" (click)="onMenu(card.id)">⋯</button>
            </div>
            <div class="card-meta">{{ card.route }} · {{ card.departed }}</div>
          </div>

          <div class="card-crew">
            <div class="crew-chip">
              <span class="crew-avatar salesman-avatar">{{ card.salesmanInitials }}</span>
              <div>
                <div class="crew-name">{{ card.salesmanName }}</div>
                <div class="crew-role">Salesman</div>
              </div>
            </div>
            <div class="crew-chip">
              <span class="crew-avatar driver-avatar">{{ card.driverInitials }}</span>
              <div>
                <div class="crew-name">{{ card.driverName }}</div>
                <div class="crew-role">Driver</div>
              </div>
            </div>
          </div>

          <div class="card-items">
            @for (item of card.items; track item.name) {
              <div class="item-row">
                <span class="item-name">{{ item.name }}</span>
                <span class="item-count">{{ item.sold }}/{{ item.loaded }}</span>
                <div class="progress-bar">
                  <div class="progress-fill" [style.width.%]="item.pct" [style.background]="item.isHigh ? 'var(--green-fg)' : '#ea580c'"></div>
                </div>
              </div>
            }
          </div>

          <div class="card-footer">
            <div class="footer-stat">
              <span class="footer-label">Cash</span>
              <span class="footer-val green">{{ card.cash | number }}</span>
            </div>
            <div class="footer-stat">
              <span class="footer-label">Credit</span>
              <span class="footer-val amber">{{ card.credit | number }}</span>
            </div>
            <div class="footer-stat">
              <span class="footer-label">Empties</span>
              <span class="footer-val">{{ card.empties }}</span>
            </div>
            <button class="footer-action" [style.background]="card.actionBg" [style.color]="card.actionColor" [style.border-color]="card.actionBorder" (click)="onCardAction(card.id)">
              {{ card.actionLabel }}
            </button>
          </div>
        </div>
      } @empty {
        <div class="empty-state">
          <span class="empty-icon">🚛</span>
          <p class="empty-title">No vehicle loadings today</p>
          <p class="empty-sub">Create a new loading to dispatch vehicles</p>
        </div>
      }
    </div>

    @if (drawerOpen()) {
      <div class="drawer-overlay" (click)="drawerOpen.set(false)"></div>
      <div class="drawer-panel">
        <div class="drawer-header">
          <div>
            <h2 class="drawer-title">New Vehicle Loading</h2>
            <span class="drawer-ref">{{ draftRef }}</span>
          </div>
          <button class="drawer-close" (click)="drawerOpen.set(false)">✕</button>
        </div>

        <div class="drawer-body">
          <div class="section-label">Assignment</div>
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Vehicle</label>
              <select class="form-select" [(ngModel)]="selectedVehicleId">
                <option value="">Select vehicle</option>
                @for (v of vehicles(); track v.id) {
                  <option [value]="v.id">{{ v.plateNumber }} — {{ v.name }}</option>
                }
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Route</label>
              <select class="form-select" [(ngModel)]="selectedRouteId">
                <option value="">Select route</option>
                @for (r of routes(); track r.id) {
                  <option [value]="r.id">{{ r.name }}</option>
                }
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Salesman</label>
              <select class="form-select" [(ngModel)]="selectedSalesmanId">
                <option value="">Select salesman</option>
                @for (s of salesmen(); track s.id) {
                  <option [value]="s.id">{{ s.name }}</option>
                }
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Driver</label>
              <select class="form-select" [(ngModel)]="selectedDriverId">
                <option value="">Select driver</option>
                @for (d of drivers(); track d.id) {
                  <option [value]="d.id">{{ d.name }}</option>
                }
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Warehouse</label>
              <select class="form-select" [(ngModel)]="selectedWarehouseId">
                <option value="">Select warehouse</option>
                @for (w of warehouses(); track w.id) {
                  <option [value]="w.id">{{ w.name }}</option>
                }
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Loading Date</label>
              <input type="date" class="form-input" [(ngModel)]="loadingDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Departure Time</label>
              <input type="time" class="form-input" [(ngModel)]="departureTime" />
            </div>
          </div>

          <div class="section-label">Load Items</div>
          <div class="items-header">
            <span class="items-col-label">Product</span>
            <span class="items-col-label qty">Qty</span>
            <span class="items-col-label"></span>
          </div>
          <div class="load-items">
            @for (row of loadRows(); track $index; let i = $index) {
              <div class="load-row">
                <select class="form-select" [(ngModel)]="row.productId">
                  <option value="">Select product</option>
                  @for (p of products(); track p.id) {
                    <option [value]="p.id">{{ p.name }}</option>
                  }
                </select>
                <input type="number" class="form-input qty-input" [(ngModel)]="row.quantity" min="1" placeholder="0" />
                <button class="remove-btn" (click)="removeRow(i)" title="Remove">✕</button>
              </div>
            }
          </div>
          <button class="add-item-btn" (click)="addRow()">+ Add</button>

          <div class="summary-card">
            <div class="summary-row">
              <span class="summary-label">Total line items</span>
              <span class="summary-value">{{ lineCount() }}</span>
            </div>
            <div class="summary-row">
              <span class="summary-label">Total cylinders</span>
              <span class="summary-value">{{ totalCylinders() }}</span>
            </div>
          </div>

          <div class="notice-amber">
            <span class="notice-icon">⚠</span>
            <span>Stock is deducted from the warehouse upon confirmation. Ensure inventory is available before proceeding.</span>
          </div>
          @if (formError()) {
            <div class="notice-error">{{ formError() }}</div>
          }
        </div>

        <div class="drawer-footer">
          <button class="btn-cancel" (click)="drawerOpen.set(false)">Cancel</button>
          <button class="btn-confirm" [disabled]="saving()" (click)="confirmLoading()">{{ saving() ? 'Saving…' : 'Confirm loading' }}</button>
        </div>
      </div>
    }

    @if (closingOpen()) {
      <div class="drawer-overlay" (click)="closingOpen.set(false)"></div>
      <div class="drawer-panel">
        <div class="drawer-header">
          <div>
            <h2 class="drawer-title">Vehicle Closing — {{ closingLoading()?.truckName }}</h2>
            <span class="drawer-ref">{{ closingLoading()?.loadingDate | date:'mediumDate' }}</span>
          </div>
          <button class="drawer-close" (click)="closingOpen.set(false)">✕</button>
        </div>

        <div class="drawer-body">
          <div class="section-label">Reconciliation</div>
          <div class="items-header close-grid">
            <span class="items-col-label">Product</span>
            <span class="items-col-label qty">Loaded</span>
            <span class="items-col-label qty">Sold</span>
            <span class="items-col-label qty">Returned</span>
            <span class="items-col-label qty">Damaged</span>
          </div>
          <div class="load-items">
            @for (row of closingRows(); track row.productId) {
              <div class="load-row close-grid">
                <span class="close-product">{{ row.productName }}</span>
                <span class="close-loaded">{{ row.loadedQuantity }}</span>
                <input type="number" class="form-input qty-input" min="0" [(ngModel)]="row.soldQuantity" />
                <input type="number" class="form-input qty-input" min="0" [(ngModel)]="row.returnedQuantity" />
                <input type="number" class="form-input qty-input" min="0" [(ngModel)]="row.damagedQuantity" />
              </div>
            }
          </div>
          <p class="close-hint">Sold is the day's total including recorded vehicle sales orders; only unrecorded cash sales are deducted again at closing.</p>

          <div class="section-label">Collections</div>
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Cash Collected (৳)</label>
              <input type="number" class="form-input" min="0" [(ngModel)]="closeCash" />
            </div>
            <div class="form-group">
              <label class="form-label">Credit Sales (৳)</label>
              <input type="number" class="form-input" min="0" [(ngModel)]="closeCredit" />
            </div>
            <div class="form-group">
              <label class="form-label">Outstanding (৳)</label>
              <input type="number" class="form-input" min="0" [(ngModel)]="closeOutstanding" />
            </div>
            <div class="form-group">
              <label class="form-label">Empty Cylinders Returned</label>
              <input type="number" class="form-input" min="0" [(ngModel)]="closeEmpties" />
            </div>
            <div class="form-group">
              <label class="form-label">Cylinder Exchanges</label>
              <input type="number" class="form-input" min="0" [(ngModel)]="closeExchanges" />
            </div>
            <div class="form-group">
              <label class="form-label">Leakage Count</label>
              <input type="number" class="form-input" min="0" [(ngModel)]="closeLeakage" />
            </div>
            <div class="form-group">
              <label class="form-label">Variance</label>
              <input type="number" class="form-input" [(ngModel)]="closeVariance" />
            </div>
            <div class="form-group">
              <label class="form-label">Notes</label>
              <input type="text" class="form-input" [(ngModel)]="closeNotes" />
            </div>
          </div>

          @if (closeError()) {
            <div class="notice-error">{{ closeError() }}</div>
          }
        </div>

        <div class="drawer-footer">
          <button class="btn-cancel" (click)="closingOpen.set(false)">Cancel</button>
          <button class="btn-confirm" [disabled]="saving()" (click)="submitClosing()">{{ saving() ? 'Closing…' : 'Confirm closing' }}</button>
        </div>
      </div>
    }
  `,
  styles: [`
    :host { display: block; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; }
    .page-title { font-size: 22px; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 0; }
    .page-subtitle { font-size: 13px; color: var(--text-muted); margin-top: 4px; }
    .header-actions { display: flex; gap: 8px; }
    .btn-secondary { display: inline-flex; align-items: center; gap: 6px; padding: 8px 16px; border: 1px solid var(--border); border-radius: var(--radius-btn); background: var(--surface); color: var(--text-secondary); font-size: 13px; font-weight: 600; cursor: pointer; transition: all 0.15s; }
    .btn-secondary:hover { background: var(--fill-subtle); border-color: var(--border-input); }
    .btn-primary { display: inline-flex; align-items: center; gap: 6px; padding: 8px 16px; border: none; border-radius: var(--radius-btn); background: var(--sidebar-bg); color: #fff; font-size: 13px; font-weight: 600; cursor: pointer; box-shadow: 0 1px 3px rgba(0,0,0,0.15); transition: all 0.15s; }
    .btn-primary:hover { opacity: 0.9; }
    .card-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(340px, 1fr)); gap: 16px; }
    .loading-card { background: var(--surface); border: 1px solid var(--border); border-radius: 14px; overflow: hidden; }
    .card-header { padding: 14px 16px 10px; }
    .card-header-top { display: flex; align-items: center; gap: 8px; }
    .truck-icon { width: 42px; height: 42px; border-radius: var(--radius-tile); display: flex; align-items: center; justify-content: center; font-size: 20px; flex-shrink: 0; }
    .plate-number { font-family: var(--font-mono); font-size: 14px; font-weight: 700; color: var(--text-primary); }
    .status-badge { display: inline-flex; align-items: center; padding: 2px 10px; border-radius: var(--radius-pill); font-size: 11px; font-weight: 600; margin-left: auto; }
    .menu-btn { width: 28px; height: 28px; border: none; background: transparent; border-radius: 6px; font-size: 16px; color: var(--text-muted); cursor: pointer; display: flex; align-items: center; justify-content: center; }
    .menu-btn:hover { background: var(--fill-subtle); }
    .card-meta { font-size: 12px; color: var(--text-muted); margin-top: 4px; margin-left: 50px; }
    .card-crew { display: flex; gap: 10px; padding: 0 16px 12px; }
    .crew-chip { display: flex; align-items: center; gap: 8px; padding: 6px 12px 6px 6px; background: var(--fill-subtle); border-radius: var(--radius-tile); border: 1px solid var(--border-row); flex: 1; min-width: 0; }
    .crew-avatar { width: 28px; height: 28px; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: #fff; font-size: 10px; font-weight: 700; flex-shrink: 0; }
    .salesman-avatar { background: #ea580c; }
    .driver-avatar { background: #1d4ed8; }
    .crew-name { font-size: 12px; font-weight: 600; color: var(--text-primary); line-height: 1.2; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .crew-role { font-size: 10px; color: var(--text-muted); }
    .card-items { padding: 0 16px 12px; }
    .item-row { display: grid; grid-template-columns: 1fr auto 60px; gap: 8px; align-items: center; padding: 4px 0; }
    .item-name { font-size: 12px; color: var(--text-secondary); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .item-count { font-size: 12px; font-weight: 600; color: var(--text-primary); font-family: var(--font-mono); }
    .progress-bar { height: 5px; background: var(--border-row); border-radius: 3px; overflow: hidden; }
    .progress-fill { height: 100%; border-radius: 3px; transition: width 0.3s; }
    .card-footer { display: flex; align-items: center; gap: 14px; padding: 10px 16px; background: var(--fill-subtle); border-top: 1px solid var(--border-row); }
    .footer-stat { display: flex; flex-direction: column; gap: 1px; }
    .footer-label { font-size: 10px; color: var(--text-faint); text-transform: uppercase; letter-spacing: 0.03em; }
    .footer-val { font-size: 13px; font-weight: 700; color: var(--text-primary); }
    .footer-val.green { color: var(--green-fg); }
    .footer-val.amber { color: var(--amber-fg2); }
    .footer-action { margin-left: auto; padding: 6px 14px; border-radius: 6px; border: 1px solid; font-size: 12px; font-weight: 600; cursor: pointer; transition: all 0.15s; }
    .empty-state { grid-column: 1 / -1; display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 60px 20px; background: var(--surface); border: 1px dashed var(--border); border-radius: var(--radius-card); }
    .empty-icon { font-size: 36px; margin-bottom: 12px; }
    .empty-title { font-size: 15px; font-weight: 700; color: var(--text-primary); margin: 0 0 4px; }
    .empty-sub { font-size: 13px; color: var(--text-muted); margin: 0; }
    .drawer-overlay { position: fixed; inset: 0; background: rgba(15, 17, 23, 0.4); z-index: 40; }
    .drawer-panel { position: fixed; top: 0; right: 0; bottom: 0; width: 500px; background: var(--surface); box-shadow: var(--shadow-drawer); z-index: 50; display: flex; flex-direction: column; animation: slideIn 0.2s ease-out; }
    @keyframes slideIn { from { transform: translateX(100%); } to { transform: translateX(0); } }
    .drawer-header { display: flex; justify-content: space-between; align-items: flex-start; padding: 20px 24px 16px; border-bottom: 1px solid var(--border); }
    .drawer-title { font-size: 16px; font-weight: 700; color: var(--text-primary); margin: 0; }
    .drawer-ref { font-family: var(--font-mono); font-size: 12px; font-weight: 600; color: var(--text-muted); background: var(--fill-subtle); padding: 2px 8px; border-radius: 4px; margin-top: 4px; display: inline-block; }
    .drawer-close { width: 32px; height: 32px; border: none; background: transparent; border-radius: 8px; font-size: 16px; color: var(--text-muted); cursor: pointer; display: flex; align-items: center; justify-content: center; }
    .drawer-close:hover { background: var(--fill-subtle); }
    .drawer-body { flex: 1; overflow-y: auto; padding: 20px 24px; }
    .section-label { font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.05em; color: var(--text-muted); margin-bottom: 12px; margin-top: 8px; }
    .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-bottom: 24px; }
    .form-group { display: flex; flex-direction: column; gap: 4px; }
    .form-label { font-size: 12px; font-weight: 600; color: var(--text-secondary); }
    .form-select, .form-input { padding: 8px 10px; border: 1px solid var(--border-input); border-radius: 6px; font-size: 13px; color: var(--text-primary); background: var(--surface); outline: none; transition: border-color 0.15s; }
    .form-select:focus, .form-input:focus { border-color: var(--primary); }
    .items-header { display: grid; grid-template-columns: 1fr 74px 34px; gap: 8px; padding: 0 0 6px; }
    .items-col-label { font-size: 11px; font-weight: 600; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.04em; }
    .items-col-label.qty { text-align: center; }
    .load-items { display: flex; flex-direction: column; gap: 8px; margin-bottom: 10px; }
    .load-row { display: grid; grid-template-columns: 1fr 74px 34px; gap: 8px; align-items: center; }
    .qty-input { width: 74px; text-align: center; }
    .remove-btn { width: 34px; height: 34px; border: 1px solid var(--border); border-radius: 6px; background: var(--surface); color: var(--text-muted); font-size: 14px; cursor: pointer; display: flex; align-items: center; justify-content: center; }
    .remove-btn:hover { border-color: var(--red-fg); color: var(--red-fg); background: var(--red-bg); }
    .add-item-btn { display: inline-flex; align-items: center; gap: 4px; padding: 6px 12px; border: 1px dashed var(--border-input); border-radius: 6px; background: transparent; color: var(--primary); font-size: 12px; font-weight: 600; cursor: pointer; margin-bottom: 20px; }
    .add-item-btn:hover { background: var(--primary-tint1); border-color: var(--primary); }
    .summary-card { background: var(--fill-subtle2); border: 1px solid var(--border); border-radius: 8px; padding: 12px 14px; margin-bottom: 16px; }
    .summary-row { display: flex; justify-content: space-between; align-items: center; padding: 4px 0; }
    .summary-label { font-size: 12px; color: var(--text-muted); }
    .summary-value { font-size: 13px; font-weight: 700; color: var(--text-primary); font-family: var(--font-mono); }
    .notice-amber { display: flex; align-items: flex-start; gap: 8px; padding: 10px 12px; background: var(--amber-bg); border: 1px solid #fde68a; border-radius: 8px; font-size: 12px; color: var(--amber-fg); line-height: 1.4; }
    .notice-error { margin-top: 12px; padding: 10px 12px; background: #fef2f2; border: 1px solid #fecaca; border-radius: 8px; font-size: 12px; color: #b91c1c; line-height: 1.4; }
    .close-grid { grid-template-columns: 1fr 52px 64px 64px 64px !important; }
    .close-product { font-size: 12px; color: var(--text-secondary); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .close-loaded { font-size: 13px; font-weight: 700; font-family: var(--font-mono); color: var(--text-primary); text-align: center; }
    .close-hint { font-size: 11px; color: var(--text-muted); margin: 6px 0 18px; line-height: 1.4; }
    .btn-confirm:disabled { opacity: 0.6; cursor: default; }
    .notice-icon { font-size: 14px; flex-shrink: 0; margin-top: 1px; }
    .drawer-footer { display: flex; justify-content: flex-end; gap: 8px; padding: 14px 24px; border-top: 1px solid var(--border); }
    .btn-cancel { padding: 8px 16px; border: 1px solid var(--border); border-radius: var(--radius-btn); background: var(--surface); color: var(--text-secondary); font-size: 13px; font-weight: 600; cursor: pointer; }
    .btn-cancel:hover { background: var(--fill-subtle); }
    .btn-confirm { padding: 8px 20px; border: none; border-radius: var(--radius-btn); background: var(--amber-fg2); color: #fff; font-size: 13px; font-weight: 600; cursor: pointer; box-shadow: 0 1px 3px rgba(217,119,6,0.3); }
    .btn-confirm:hover { background: var(--amber-fg); }
    @media (max-width: 800px) { .drawer-panel { width: 100%; } .card-grid { grid-template-columns: 1fr; } }
  `],
})
export class VehicleLoadingListComponent implements OnInit {
  private api = inject(ApiService);
  private router = inject(Router);

  items = signal<VehicleLoading[]>([]);
  closingsByLoading = signal<Record<string, any>>({});
  drawerOpen = signal(false);
  draftRef = 'LD-' + Math.floor(1000 + Math.random() * 9000);

  vehicles = signal<Truck[]>([]);
  drivers = signal<Driver[]>([]);
  salesmen = signal<Salesman[]>([]);
  routes = signal<Route[]>([]);
  products = signal<Product[]>([]);
  warehouses = signal<any[]>([]);

  selectedVehicleId = '';
  selectedRouteId = '';
  selectedSalesmanId = '';
  selectedDriverId = '';
  selectedWarehouseId = '';
  loadingDate = new Date().toISOString().slice(0, 10);
  departureTime = '08:00';
  saving = signal(false);
  formError = signal('');

  loadRows = signal<LoadItemRow[]>([{ productId: '', quantity: 1 }]);

  // Closing drawer state
  closingOpen = signal(false);
  closingLoading = signal<VehicleLoading | null>(null);
  closingRows = signal<any[]>([]);
  closeCash = 0;
  closeCredit = 0;
  closeOutstanding = 0;
  closeEmpties = 0;
  closeExchanges = 0;
  closeLeakage = 0;
  closeVariance = 0;
  closeNotes = '';
  closeError = signal('');

  lineCount = computed(() => this.loadRows().filter(r => r.productId).length);
  totalCylinders = computed(() => this.loadRows().reduce((sum, r) => sum + (r.quantity || 0), 0));

  todayDate = new Date().toLocaleDateString('en-US', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });

  displayCards = computed(() => this.items().map(v => this.mapToCard(v)));

  ngOnInit() {
    this.loadData();
    this.loadReferenceData();
  }

  loadData() {
    this.api.getAll<VehicleLoading>('vehicleloadings').subscribe(data => {
      this.items.set(data.items);
    });
    this.api.getAll<any>('vehicleclosings', 1, 100).subscribe(data => {
      const map: Record<string, any> = {};
      for (const c of data.items) map[c.vehicleLoadingId] = c;
      this.closingsByLoading.set(map);
    });
  }

  loadReferenceData() {
    this.api.getAllList<Truck>('trucks').subscribe(d => this.vehicles.set(d));
    this.api.getAllList<Driver>('drivers').subscribe(d => this.drivers.set(d));
    this.api.getAllList<Salesman>('salesmen').subscribe(d => this.salesmen.set(d));
    this.api.getAllList<Route>('routes').subscribe(d => this.routes.set(d));
    this.api.getAllList<Product>('products').subscribe(d => this.products.set(d));
    this.api.getAllList<any>('warehouses').subscribe(d => this.warehouses.set(d));
  }

  mapToCard(v: VehicleLoading): any {
    const statusMap: Record<number, { label: string; bg: string; color: string }> = {
      0: { label: 'Loading', bg: '#fefce8', color: '#a16207' },
      1: { label: 'Selling', bg: '#f0fdf4', color: '#15803d' },
      2: { label: 'Returned', bg: '#eff6ff', color: '#1d4ed8' },
    };
    const s = statusMap[v.status] ?? statusMap[0];
    const items = (v.items || []).map(it => {
      const pct = it.loadedQuantity > 0 ? Math.round((it.soldQuantity / it.loadedQuantity) * 100) : 0;
      return { name: it.productName, loaded: it.loadedQuantity, sold: it.soldQuantity, pct, isHigh: pct > 70 };
    });
    const nameParts = (v.salesmanName || '').split(' ');
    const driverParts = (v.driverName || '').split(' ');
    const closing = this.closingsByLoading()[v.id];
    return {
      id: v.id,
      plate: v.truckName,
      status: s.label,
      statusBg: s.bg,
      statusColor: s.color,
      route: (v as any).routeName || v.warehouseName || '—',
      departed: v.loadingDate ? new Date(v.loadingDate).toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' }) : '—',
      iconBg: '#fff7ed',
      salesmanName: v.salesmanName,
      salesmanInitials: (nameParts[0]?.[0] ?? '') + (nameParts[1]?.[0] ?? ''),
      driverName: v.driverName,
      driverInitials: (driverParts[0]?.[0] ?? '') + (driverParts[1]?.[0] ?? ''),
      items,
      cash: closing?.cashCollected ?? 0,
      credit: closing?.creditSales ?? 0,
      empties: closing?.returnedEmptyCylinders ?? 0,
      actionLabel: s.label === 'Returned' ? 'View' : 'Closing',
      actionBg: 'var(--fill-subtle)',
      actionColor: 'var(--text-secondary)',
      actionBorder: 'var(--border)',
    };
  }

  addRow() {
    this.loadRows.update(rows => [...rows, { productId: '', quantity: 1 }]);
  }

  removeRow(index: number) {
    this.loadRows.update(rows => rows.filter((_, i) => i !== index));
  }

  onMenu(id: string) {
    this.router.navigate(['/vehicle-loadings', id]);
  }

  onCardAction(id: string) {
    const loading = this.items().find(v => v.id === id);
    if (!loading) return;
    if (loading.status === 2) {
      this.router.navigate(['/vehicle-loadings', id]);
      return;
    }
    this.openClosing(loading);
  }

  openClosing(loading: VehicleLoading) {
    this.closingLoading.set(loading);
    this.closingRows.set((loading.items || []).map(it => ({
      productId: it.productId,
      productName: it.productName,
      loadedQuantity: it.loadedQuantity,
      // Prefill: sold from recorded vehicle sales orders, the rest assumed returned.
      soldQuantity: it.soldQuantity ?? 0,
      returnedQuantity: Math.max(0, it.loadedQuantity - (it.soldQuantity ?? 0)),
      damagedQuantity: 0,
    })));
    this.closeCash = 0;
    this.closeCredit = 0;
    this.closeOutstanding = 0;
    this.closeEmpties = 0;
    this.closeExchanges = 0;
    this.closeLeakage = 0;
    this.closeVariance = 0;
    this.closeNotes = '';
    this.closeError.set('');
    this.closingOpen.set(true);
  }

  submitClosing() {
    const loading = this.closingLoading();
    if (!loading) return;

    const bad = this.closingRows().find(r => r.soldQuantity + r.returnedQuantity + r.damagedQuantity > r.loadedQuantity);
    if (bad) {
      this.closeError.set(`Sold + returned + damaged exceeds the loaded quantity for ${bad.productName}.`);
      return;
    }

    const payload = {
      vehicleLoadingId: loading.id,
      cashCollected: this.closeCash || 0,
      creditSales: this.closeCredit || 0,
      outstandingAmount: this.closeOutstanding || 0,
      cylinderExchanges: this.closeExchanges || 0,
      returnedEmptyCylinders: this.closeEmpties || 0,
      damagedCount: this.closingRows().reduce((s, r) => s + (r.damagedQuantity || 0), 0),
      leakageCount: this.closeLeakage || 0,
      variance: this.closeVariance || 0,
      notes: this.closeNotes,
      items: this.closingRows().map(r => ({
        productId: r.productId,
        loadedQuantity: r.loadedQuantity,
        soldQuantity: r.soldQuantity || 0,
        returnedQuantity: r.returnedQuantity || 0,
        damagedQuantity: r.damagedQuantity || 0,
      })),
    };

    this.saving.set(true);
    this.closeError.set('');
    this.api.post(`vehicleloadings/${loading.id}/close`, payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.closingOpen.set(false);
        this.loadData();
      },
      error: (e) => {
        this.saving.set(false);
        this.closeError.set(e?.error?.message ?? 'Failed to close the vehicle.');
      },
    });
  }

  confirmLoading() {
    if (!this.selectedVehicleId || !this.selectedDriverId || !this.selectedSalesmanId || !this.selectedWarehouseId) {
      this.formError.set('Vehicle, driver, salesman, and warehouse are required.');
      return;
    }
    const items = this.loadRows().filter(r => r.productId).map(r => ({
      productId: r.productId,
      loadedQuantity: r.quantity,
    }));
    if (items.length === 0) {
      this.formError.set('Add at least one load item.');
      return;
    }

    const payload = {
      truckId: this.selectedVehicleId,
      driverId: this.selectedDriverId,
      salesmanId: this.selectedSalesmanId,
      warehouseId: this.selectedWarehouseId,
      routeId: this.selectedRouteId || null,
      loadingDate: `${this.loadingDate}T${this.departureTime || '08:00'}:00`,
      items,
    };

    this.saving.set(true);
    this.formError.set('');
    this.api.post('vehicleloadings', payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.drawerOpen.set(false);
        this.loadData();
        this.resetForm();
      },
      error: (e) => {
        this.saving.set(false);
        this.formError.set(e?.error?.message ?? 'Failed to create the loading.');
      },
    });
  }

  private resetForm() {
    this.selectedVehicleId = '';
    this.selectedRouteId = '';
    this.selectedSalesmanId = '';
    this.selectedDriverId = '';
    this.selectedWarehouseId = '';
    this.loadingDate = new Date().toISOString().slice(0, 10);
    this.departureTime = '08:00';
    this.loadRows.set([{ productId: '', quantity: 1 }]);
    this.formError.set('');
  }
}
