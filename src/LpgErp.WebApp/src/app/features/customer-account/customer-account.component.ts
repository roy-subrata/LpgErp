import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { CustomerStatement, CustomerOrder, Customer } from '../../core/models';
import { PaymentFormComponent } from '../payments/payment-form.component';

/**
 * Everything about one customer in one place — what they owe, what they have paid, the cylinders
 * they hold and the deposit held against them. Replaces jumping between the credit, gas ledger
 * and cylinder ledger screens and re-picking the customer in each.
 */
@Component({
  selector: 'app-customer-account',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, PaymentFormComponent],
  template: `
    <div class="page-header no-print">
      <div>
        <a routerLink="/customers" class="back-link">← Customers</a>
        <h1 class="page-title">{{ customer()?.name || 'Customer' }}</h1>
        <span class="page-sub">{{ customer()?.code }}{{ customer()?.phone ? ' · ' + customer()?.phone : '' }}</span>
      </div>
      <div class="header-actions">
        <button class="btn-secondary-sm" (click)="print()">🖨 Print</button>
        <button class="btn-primary-sm" (click)="paymentOpen.set(true)">+ Record Payment</button>
      </div>
    </div>

    <div class="print-only print-head">
      <h2>{{ customer()?.name }} — Account Statement</h2>
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
          @if (s.summary.creditLimit > 0) {
            <span class="kpi-foot" [class.danger]="s.summary.isOverCredit">
              {{ s.summary.creditUtilization | number:'1.0-0' }}% of ৳{{ s.summary.creditLimit | number:'1.0-0' }} limit
              {{ s.summary.isOverCredit ? '· over limit' : '' }}
            </span>
          }
        </div>
        <div class="kpi-card">
          <div class="kpi-top">
            <span class="kpi-label">Deposit Held</span>
            <span class="kpi-icon" style="background:#f0fdf4;color:#15803d">🔒</span>
          </div>
          <div class="kpi-value">৳ {{ s.summary.depositHeld | number:'1.0-0' }}</div>
          <span class="kpi-foot">refundable — not revenue</span>
        </div>
        <div class="kpi-card">
          <div class="kpi-top">
            <span class="kpi-label">Cylinders Held</span>
            <span class="kpi-icon" style="background:#eff6ff;color:#1d4ed8">🛢</span>
          </div>
          <div class="kpi-value">{{ s.summary.cylindersHeld }}</div>
          <span class="kpi-foot">not yet returned</span>
        </div>
        <div class="kpi-card">
          <div class="kpi-top">
            <span class="kpi-label">Total Billed</span>
            <span class="kpi-icon" style="background:#fff7ed;color:#ea580c">▤</span>
          </div>
          <div class="kpi-value">৳ {{ s.summary.totalBilled | number:'1.0-0' }}</div>
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
                        @else if (isDeposit(line.kind)) {
                          <span class="badge" style="background:#f0fdf4;color:#15803d">
                            {{ line.kind === 2 ? '+' : '−' }}৳{{ line.amount | number:'1.0-0' }} deposit
                          </span>
                        }
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
            <p class="foot-note">
              Cylinder deposits move money but are refundable, so they are shown separately and left out
              of the balance owed on goods.
            </p>
          }

          @case ('cylinders') {
            <div class="table-scroll">
              <table class="data-table">
                <thead>
                  <tr>
                    <th style="width:40%">Brand</th>
                    <th style="width:40%">Size</th>
                    <th style="width:20%" class="right">Held</th>
                  </tr>
                </thead>
                <tbody>
                  @for (c of s.cylinders; track $index) {
                    <tr class="data-row">
                      <td><span class="main-text">{{ c.brandName }}</span></td>
                      <td><span class="muted-text">{{ c.cylinderSizeName }}</span></td>
                      <td class="right"><span class="num-text">{{ c.held }}</span></td>
                    </tr>
                  } @empty {
                    <tr><td colspan="3" class="empty-row">This customer is not holding any cylinders.</td></tr>
                  }
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
                      <td>
                        <div class="main-cell">
                          <span class="mono-text">{{ o.orderNumber }}</span>
                          @if (o.isCreditSale) { <span class="sub-text">credit</span> }
                        </div>
                      </td>
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
                      <td class="right"><span class="money-text">৳ {{ o.netAmount | number:'1.0-0' }}</span></td>
                      <td class="right"><span class="money-text credit">৳ {{ o.paid | number:'1.0-0' }}</span></td>
                      <td class="right">
                        <span class="money-text" [class.danger]="o.outstanding > 0">৳ {{ o.outstanding | number:'1.0-0' }}</span>
                        @if (o.isOverdue) { <span class="badge overdue">overdue</span> }
                      </td>
                    </tr>
                  } @empty {
                    <tr><td colspan="7" class="empty-row">No orders for this customer.</td></tr>
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
      [customerId]="customerId"
      (close)="paymentOpen.set(false)"
      (saved)="onPaymentSaved()" />
  `,
  styles: [`
    .back-link { font-size: 12px; color: var(--text-muted, #6b7280); text-decoration: none; }
    .back-link:hover { text-decoration: underline; }
    .page-sub { font-size: 12px; color: var(--text-muted, #6b7280); }
    .state-note { padding: 24px; color: #6b7280; }
    .state-note.error { color: #b91c1c; }

    .kpi-foot { display: block; margin-top: 4px; font-size: 11px; color: #6b7280; }
    .kpi-value.danger, .money-text.danger, .muted-text.danger, .kpi-foot.danger { color: #b91c1c; }
    .money-text.credit { color: #15803d; }

    .right { text-align: right; }
    .range-label { font-size: 11px; color: #6b7280; text-transform: uppercase; letter-spacing: 0.04em; }
    .range-input { padding: 5px 8px; border: 1px solid var(--border-input, #e0e3e9); border-radius: 6px; font-size: 12px; }

    .opening-row td, .closing-row td { background: var(--fill-subtle, #fafbfc); font-weight: 600; font-size: 12px; }
    .closing-row td { border-top: 2px solid var(--border, #e7e9ee); }

    .badge.overdue { background: #fef2f2; color: #b91c1c; margin-left: 6px; }
    .foot-note { margin: 0; padding: 10px 16px; font-size: 11px; color: #6b7280; border-top: 1px solid var(--border-row, #f1f3f6); }

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
export class CustomerAccountComponent implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);

  customerId = '';
  from = '';
  to = '';
  readonly today = new Date();

  customer = signal<Customer | null>(null);
  statement = signal<CustomerStatement | null>(null);
  orders = signal<CustomerOrder[]>([]);
  loading = signal(true);
  error = signal('');
  tab = signal<'statement' | 'cylinders' | 'orders'>('statement');
  paymentOpen = signal(false);

  readonly tabs = [
    { key: 'statement' as const, label: 'Statement' },
    { key: 'cylinders' as const, label: 'Cylinders' },
    { key: 'orders' as const, label: 'Orders' },
  ];

  /** [label, background, text] — matches SalesOrderStatus. */
  readonly statusBadge: Record<number, string[]> = {
    0: ['Draft', '#f4f5f7', '#6b7280'],
    1: ['Confirmed', '#eff6ff', '#1d4ed8'],
    2: ['Delivered', '#f0fdf4', '#15803d'],
    3: ['Cancelled', '#fef2f2', '#b91c1c'],
  };

  rangeLabel = computed(() => {
    if (!this.from && !this.to) return 'All transactions';
    return `${this.from || 'start'} to ${this.to || 'today'}`;
  });

  ngOnInit() {
    this.customerId = this.route.snapshot.paramMap.get('id') ?? '';
    if (!this.customerId) {
      this.error.set('No customer specified.');
      this.loading.set(false);
      return;
    }

    this.api.getById<Customer>('customers', this.customerId)
      .subscribe({ next: c => this.customer.set(c), error: () => {} });

    this.api.get<CustomerOrder[]>(`customeraccount/customer/${this.customerId}/orders`)
      .subscribe({ next: o => this.orders.set(o), error: () => {} });

    this.reload();
  }

  reload() {
    this.loading.set(true);
    const params: string[] = [];
    if (this.from) params.push(`from=${this.from}`);
    if (this.to) params.push(`to=${this.to}T23:59:59`);
    const qs = params.length ? `?${params.join('&')}` : '';

    this.api.get<CustomerStatement>(`customeraccount/customer/${this.customerId}/statement${qs}`)
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

  /** Deposit lines carry cash but sit outside the goods balance. */
  isDeposit(kind: number): boolean {
    return kind === 2 || kind === 3;
  }

  onPaymentSaved() {
    this.paymentOpen.set(false);
    this.api.get<CustomerOrder[]>(`customeraccount/customer/${this.customerId}/orders`)
      .subscribe({ next: o => this.orders.set(o), error: () => {} });
    this.reload();
  }

  print() {
    window.print();
  }
}
