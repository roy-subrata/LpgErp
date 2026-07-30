import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { SupplierStatement, SupplierOrder, Supplier } from '../../core/models';
import { PaymentFormComponent } from '../payments/payment-form.component';

/**
 * Everything about one supplier in one place — what we owe them, what we've paid, and their
 * commission activity. Mirrors the customer account page.
 */
@Component({
  selector: 'app-supplier-account',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, PaymentFormComponent],
  template: `
    <div class="page-header no-print">
      <div>
        <a routerLink="/suppliers" class="back-link">← Suppliers</a>
        <h1 class="page-title">{{ supplier()?.name || 'Supplier' }}</h1>
        <span class="page-sub">{{ supplier()?.code }}{{ supplier()?.phone ? ' · ' + supplier()?.phone : '' }}</span>
      </div>
      <div class="header-actions">
        <button class="btn-secondary-sm" (click)="print()">🖨 Print</button>
        <button class="btn-primary-sm" (click)="paymentOpen.set(true)">+ Record Payment</button>
      </div>
    </div>

    <div class="print-only print-head">
      <h2>{{ supplier()?.name }} — Account Statement</h2>
      <span>{{ rangeLabel() }} · printed {{ today | date:'dd MMM yyyy' }}</span>
    </div>

    @if (loading()) {
      <div class="state-note">Loading…</div>
    } @else if (error()) {
      <div class="state-note error">{{ error() }}</div>
    } @else if (statement(); as s) {
      <div class="kpi-grid">
        <div class="kpi-card">
          <div class="kpi-top">
            <span class="kpi-label">Outstanding Due</span>
            <span class="kpi-icon" style="background:#fef2f2;color:#dc2626">৳</span>
          </div>
          <div class="kpi-value" [class.danger]="s.summary.outstandingDue > 0">৳ {{ s.summary.outstandingDue | number:'1.0-0' }}</div>
          <span class="kpi-foot">what we owe this supplier</span>
        </div>
        <div class="kpi-card">
          <div class="kpi-top">
            <span class="kpi-label">Commission Balance</span>
            <span class="kpi-icon" style="background:#faf5ff;color:#7e22ce">%</span>
          </div>
          <div class="kpi-value">৳ {{ s.summary.commissionBalance | number:'1.0-0' }}</div>
          <span class="kpi-foot">available to apply to a future order</span>
        </div>
        <div class="kpi-card">
          <div class="kpi-top">
            <span class="kpi-label">Commission Earned</span>
            <span class="kpi-icon" style="background:#fff7ed;color:#c2410c">Σ</span>
          </div>
          <div class="kpi-value">৳ {{ s.summary.commissionEarnedLifetime | number:'1.0-0' }}</div>
          <span class="kpi-foot">lifetime, applied or not</span>
        </div>
        <div class="kpi-card">
          <div class="kpi-top">
            <span class="kpi-label">Total Purchased</span>
            <span class="kpi-icon" style="background:#eff6ff;color:#1d4ed8">▤</span>
          </div>
          <div class="kpi-value">৳ {{ s.summary.totalPurchased | number:'1.0-0' }}</div>
          <span class="kpi-foot">paid ৳{{ s.summary.totalPaid | number:'1.0-0' }}</span>
        </div>
      </div>

      <div class="table-card">
        <div class="table-toolbar no-print">
          <div class="tab-group">
            @for (t of tabs; track t.key) {
              <button class="tab-btn" [class.active]="tab() === t.key" (click)="tab.set(t.key)">{{ t.label }}</button>
            }
          </div>
          @if (tab() === 'statement') {
            <div class="toolbar-right">
              <label class="range-label">From</label>
              <input type="date" class="range-input" [(ngModel)]="from" (change)="reload()" />
              <label class="range-label">To</label>
              <input type="date" class="range-input" [(ngModel)]="to" (change)="reload()" />
              @if (from || to) {
                <button class="btn-secondary-sm" (click)="clearRange()">Clear</button>
              }
            </div>
          }
        </div>

        @switch (tab()) {
          @case ('statement') {
            <div class="table-scroll">
              <table class="data-table">
                <thead>
                  <tr>
                    <th style="width:12%">Date</th>
                    <th style="width:44%">Description</th>
                    <th style="width:14%" class="right">Debit</th>
                    <th style="width:14%" class="right">Credit</th>
                    <th style="width:16%" class="right">Balance</th>
                  </tr>
                </thead>
                <tbody>
                  <tr class="opening-row">
                    <td colspan="4">Opening balance{{ from ? ' as at ' + (from | date:'dd MMM yyyy') : '' }}</td>
                    <td class="right"><span class="money-text">৳ {{ s.openingBalance | number:'1.0-0' }}</span></td>
                  </tr>
                  @for (line of s.lines; track $index) {
                    <tr class="data-row">
                      <td><span class="muted-text">{{ line.date | date:'dd MMM yyyy' }}</span></td>
                      <td>
                        <div class="main-cell">
                          <span class="main-text">{{ line.description }}</span>
                          @if (line.reference) {
                            <span class="sub-text">{{ line.reference }}</span>
                          }
                        </div>
                      </td>
                      <td class="right">
                        @if (line.debit) { <span class="money-text">৳ {{ line.debit | number:'1.0-0' }}</span> }
                      </td>
                      <td class="right">
                        @if (line.credit) { <span class="money-text credit">৳ {{ line.credit | number:'1.0-0' }}</span> }
                      </td>
                      <td class="right"><span class="money-text">৳ {{ line.runningBalance | number:'1.0-0' }}</span></td>
                    </tr>
                  } @empty {
                    <tr><td colspan="5" class="empty-row">No transactions in this period.</td></tr>
                  }
                  <tr class="closing-row">
                    <td colspan="4">Closing balance — amount due</td>
                    <td class="right"><span class="money-text" [class.danger]="s.closingBalance > 0">৳ {{ s.closingBalance | number:'1.0-0' }}</span></td>
                  </tr>
                </tbody>
              </table>
            </div>
          }

          @case ('orders') {
            <div class="table-scroll">
              <table class="data-table">
                <thead>
                  <tr>
                    <th style="width:16%">Order</th>
                    <th style="width:13%">Date</th>
                    <th style="width:13%">Due</th>
                    <th style="width:12%">Status</th>
                    <th style="width:15%" class="right">Amount</th>
                    <th style="width:15%" class="right">Paid</th>
                    <th style="width:16%" class="right">Outstanding</th>
                  </tr>
                </thead>
                <tbody>
                  @for (o of orders(); track o.id) {
                    <tr class="data-row">
                      <td><span class="mono-text">{{ o.orderNumber }}</span></td>
                      <td><span class="muted-text">{{ o.orderDate | date:'dd MMM yyyy' }}</span></td>
                      <td>
                        @if (o.dueDate) {
                          <span class="muted-text" [class.danger]="o.isOverdue">{{ o.dueDate | date:'dd MMM yyyy' }}</span>
                        } @else { <span class="muted-text">—</span> }
                      </td>
                      <td>
                        <span class="badge" [style.background]="statusBadge[o.status]?.[1]" [style.color]="statusBadge[o.status]?.[2]">
                          {{ statusBadge[o.status]?.[0] }}
                        </span>
                      </td>
                      <td class="right"><span class="money-text">৳ {{ o.netPayable | number:'1.0-0' }}</span></td>
                      <td class="right"><span class="money-text credit">৳ {{ o.paid | number:'1.0-0' }}</span></td>
                      <td class="right">
                        <span class="money-text" [class.danger]="o.outstanding > 0">৳ {{ o.outstanding | number:'1.0-0' }}</span>
                        @if (o.isOverdue) { <span class="badge overdue">overdue</span> }
                      </td>
                    </tr>
                  } @empty {
                    <tr><td colspan="7" class="empty-row">No purchase orders for this supplier.</td></tr>
                  }
                </tbody>
              </table>
            </div>
          }
        }
      </div>
    }

    <app-payment-form
      [open]="paymentOpen()"
      [entityId]="null"
      [supplierId]="supplierId"
      (close)="paymentOpen.set(false)"
      (saved)="onPaymentSaved()" />
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .page-title { font-size: 22px; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 4px 0 0; }
    .header-actions { display: flex; gap: 8px; align-items: center; }

    .btn-primary-sm { padding: 9px 18px; border-radius: 7px; border: none; background: var(--primary); color: #fff; font-size: 13px; font-weight: 600; cursor: pointer; box-shadow: var(--shadow-btn); }
    .btn-primary-sm:hover { background: var(--primary-hover); }
    .btn-secondary-sm { padding: 9px 18px; border-radius: 7px; border: 1px solid var(--border-input); background: var(--surface); color: var(--text-secondary); font-size: 13px; font-weight: 600; cursor: pointer; }
    .btn-secondary-sm:hover { background: var(--fill-subtle); }

    .kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 20px; }
    .kpi-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); padding: 16px 18px; }
    .kpi-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
    .kpi-label { font-size: 12px; font-weight: 600; color: var(--text-muted); }
    .kpi-icon { width: 26px; height: 26px; border-radius: 7px; display: flex; align-items: center; justify-content: center; font-size: 14px; }
    .kpi-value { font-size: 24px; font-weight: 800; letter-spacing: -0.02em; color: var(--text-primary); line-height: 1.1; }
    @media (max-width: 1200px) { .kpi-grid { grid-template-columns: repeat(2, 1fr); } }

    .table-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); overflow: hidden; }
    .table-toolbar { display: flex; align-items: center; justify-content: space-between; padding: 12px 16px; border-bottom: 1px solid var(--border-row); gap: 12px; flex-wrap: wrap; }
    .toolbar-right { display: flex; align-items: center; gap: 12px; }
    .tab-group { display: flex; gap: 4px; background: var(--fill-subtle); border-radius: 7px; padding: 3px; }
    .tab-btn { padding: 5px 12px; border: none; border-radius: 5px; background: transparent; font-size: 12px; font-weight: 600; color: var(--text-muted); cursor: pointer; transition: all 0.15s; }
    .tab-btn.active { background: var(--surface); color: var(--text-primary); box-shadow: 0 1px 2px rgba(0,0,0,0.06); }
    .tab-btn:hover:not(.active) { color: var(--text-secondary); }

    .table-scroll { overflow-x: auto; }
    .data-table { width: 100%; border-collapse: collapse; min-width: 600px; }
    .data-table th { padding: 10px 14px; text-align: left; font-size: 12px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.04em; border-bottom: 1px solid var(--border); background: var(--fill-subtle); white-space: nowrap; }
    .data-table td { padding: 12px 14px; font-size: 13px; border-bottom: 1px solid var(--border-row); color: var(--text-primary); }
    .data-row:hover { background: var(--fill-subtle); }
    .mono-text { font-family: var(--font-mono); font-size: 12px; color: var(--text-muted); }
    .main-cell { display: flex; flex-direction: column; gap: 2px; }
    .main-text { font-weight: 600; color: var(--text-primary); }
    .sub-text { font-size: 12px; color: var(--text-muted); }
    .badge { display: inline-block; padding: 3px 10px; border-radius: var(--radius-pill); font-size: 12px; font-weight: 600; white-space: nowrap; }
    .empty-row { text-align: center; padding: 40px 14px !important; color: var(--text-muted); }

    .back-link { font-size: 12px; color: var(--text-muted); text-decoration: none; }
    .back-link:hover { text-decoration: underline; }
    .page-sub { font-size: 12px; color: var(--text-muted); }
    .state-note { padding: 24px; color: #6b7280; }
    .state-note.error { color: #b91c1c; }

    .kpi-foot { display: block; margin-top: 4px; font-size: 11px; color: #6b7280; }
    .kpi-value.danger, .money-text.danger, .muted-text.danger, .kpi-foot.danger { color: #b91c1c; }
    .money-text { font-weight: 700; color: var(--text-primary); }
    .money-text.credit { color: #15803d; }
    .muted-text { font-size: 12px; color: var(--text-muted); }

    .data-table th.right, .data-table td.right { text-align: right; }
    .range-label { font-size: 11px; color: #6b7280; text-transform: uppercase; letter-spacing: 0.04em; }
    .range-input { padding: 5px 8px; border: 1px solid var(--border-input, #e0e3e9); border-radius: 6px; font-size: 12px; }

    .opening-row td, .closing-row td { background: var(--fill-subtle, #fafbfc); font-weight: 600; font-size: 12px; }
    .closing-row td { border-top: 2px solid var(--border, #e7e9ee); }

    .badge.overdue { background: #fef2f2; color: #b91c1c; margin-left: 6px; }

    .print-only { display: none; }
    @media print {
      .no-print { display: none !important; }
      .print-only { display: block; }
      .print-head h2 { margin: 0 0 4px; font-size: 18px; }
      .print-head span { font-size: 11px; color: #555; }
      .table-card { border: none; box-shadow: none; }
    }
  `],
})
export class SupplierAccountComponent implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);

  supplierId = '';
  from = '';
  to = '';
  readonly today = new Date();

  supplier = signal<Supplier | null>(null);
  statement = signal<SupplierStatement | null>(null);
  orders = signal<SupplierOrder[]>([]);
  loading = signal(true);
  error = signal('');
  tab = signal<'statement' | 'orders'>('statement');
  paymentOpen = signal(false);

  readonly tabs = [
    { key: 'statement' as const, label: 'Statement' },
    { key: 'orders' as const, label: 'Orders' },
  ];

  /** [label, background, text] — matches PurchaseOrderStatus. */
  readonly statusBadge: Record<number, string[]> = {
    0: ['Draft', '#f4f5f7', '#6b7280'],
    1: ['Confirmed', '#dbeafe', '#1d4ed8'],
    2: ['In Transit', '#e0e7ff', '#4338ca'],
    3: ['Partially Received', '#fef3c7', '#92400e'],
    4: ['Received', '#dcfce7', '#166534'],
    5: ['Cancelled', '#fee2e2', '#991b1b'],
  };

  rangeLabel = computed(() => {
    if (!this.from && !this.to) return 'All transactions';
    return `${this.from || 'start'} to ${this.to || 'today'}`;
  });

  ngOnInit() {
    this.supplierId = this.route.snapshot.paramMap.get('id') ?? '';
    if (!this.supplierId) {
      this.error.set('No supplier specified.');
      this.loading.set(false);
      return;
    }

    this.api.getById<Supplier>('suppliers', this.supplierId)
      .subscribe({ next: s => this.supplier.set(s), error: () => {} });

    this.api.get<SupplierOrder[]>(`supplieraccount/supplier/${this.supplierId}/orders`)
      .subscribe({ next: o => this.orders.set(o), error: () => {} });

    this.reload();
  }

  reload() {
    this.loading.set(true);
    const params: string[] = [];
    if (this.from) params.push(`from=${this.from}`);
    if (this.to) params.push(`to=${this.to}T23:59:59`);
    const qs = params.length ? `?${params.join('&')}` : '';

    this.api.get<SupplierStatement>(`supplieraccount/supplier/${this.supplierId}/statement${qs}`)
      .subscribe({
        next: s => {
          this.statement.set(s);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Could not load this account.');
          this.loading.set(false);
        },
      });
  }

  clearRange() {
    this.from = '';
    this.to = '';
    this.reload();
  }

  onPaymentSaved() {
    this.paymentOpen.set(false);
    this.api.get<SupplierOrder[]>(`supplieraccount/supplier/${this.supplierId}/orders`)
      .subscribe({ next: o => this.orders.set(o), error: () => {} });
    this.reload();
  }

  print() {
    window.print();
  }
}
