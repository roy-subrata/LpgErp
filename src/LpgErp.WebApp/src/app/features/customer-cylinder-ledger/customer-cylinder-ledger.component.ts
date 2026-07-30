import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { CustomerCylinderBalance, Customer, Warehouse } from '../../core/models';

@Component({
  selector: 'app-customer-cylinder-ledger',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <h1>Customer Cylinder Ledger</h1>
      <div class="filter-group">
        <select [(ngModel)]="selectedCustomerId" (ngModelChange)="onCustomerChange()">
          <option value="">All Customers</option>
          @for (c of customers(); track c.id) {
            <option [value]="c.id">{{ c.name }}</option>
          }
        </select>
      </div>
    </div>
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Customer</th>
            <th>Brand</th>
            <th>Cylinder Size</th>
            <th>Received</th>
            <th>Returned</th>
            <th>Forfeited</th>
            <th>Outstanding</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          @for (b of items(); track b.customerId + b.brandId + b.cylinderSizeId) {
            <tr [class.highlight]="b.outstanding > 0">
              <td>{{ b.customerName }}</td>
              <td>{{ b.brandName }}</td>
              <td>{{ b.cylinderSizeName }}</td>
              <td>{{ b.received }}</td>
              <td>{{ b.returned }}</td>
              <td>{{ b.forfeited }}</td>
              <td [class.text-danger]="b.outstanding > 0">{{ b.outstanding }}</td>
              <td>
                <button class="btn-sm" (click)="openAdjust(b)">↩ Return / Adjust</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="8">No cylinder balances found.</td></tr>
          }
        </tbody>
      </table>
    </div>

    @if (adjustOpen()) {
      <div class="overlay" (click)="adjustOpen.set(false)"></div>
      <div class="modal">
        <h3>{{ adjustTarget()?.customerName }} — {{ adjustTarget()?.brandName }} {{ adjustTarget()?.cylinderSizeName }}</h3>
        <p class="modal-sub">Outstanding: <b>{{ adjustTarget()?.outstanding }}</b> cylinder(s)</p>

        <div class="form-row">
          <label>Direction</label>
          <div class="radio-row">
            <label><input type="radio" name="dir" [value]="'return'" [(ngModel)]="mode" /> Customer returns empties</label>
            <label><input type="radio" name="dir" [value]="'receive'" [(ngModel)]="mode" /> Customer receives cylinders</label>
            <label><input type="radio" name="dir" [value]="'forfeit'" [(ngModel)]="mode" /> Customer keeps it — charge for cylinder</label>
          </div>
        </div>
        <div class="form-row">
          <label>Quantity</label>
          <input type="number" min="1" [(ngModel)]="quantity" />
        </div>

        @if (mode === 'return') {
          <div class="settle-box">
            <div class="settle-title">Optional: adjust credit due for this return</div>
            <div class="form-row">
              <label>Credit sales order</label>
              <select [(ngModel)]="settlementOrderId">
                <option value="">-- No credit adjustment --</option>
                @for (o of creditOrders(); track o.id) {
                  <option [value]="o.id">{{ o.orderNumber }} · due ৳{{ o.due | number:'1.0-0' }}</option>
                }
              </select>
            </div>
            @if (settlementOrderId) {
              <div class="form-row">
                <label>Amount to credit (৳)</label>
                <input type="number" min="0" [(ngModel)]="settlementAmount" />
              </div>
            }
          </div>
        }

        @if (mode === 'forfeit') {
          <div class="settle-box">
            <div class="settle-title">Bill the customer for the cylinder(s) they're keeping</div>
            <div class="form-row">
              <label>Warehouse</label>
              <select [(ngModel)]="forfeitWarehouseId">
                <option value="">-- Select --</option>
                @for (w of warehouses(); track w.id) {
                  <option [value]="w.id">{{ w.name }}</option>
                }
              </select>
            </div>
            <div class="form-row">
              <label>Price per cylinder (৳)</label>
              <input type="number" min="0.01" step="0.01" [(ngModel)]="forfeitUnitPrice" />
            </div>
            <div class="form-row">
              <label>Payment</label>
              <div class="radio-row">
                <label><input type="radio" name="pay" [value]="false" [(ngModel)]="forfeitOnCredit" /> Paid now (cash)</label>
                <label><input type="radio" name="pay" [value]="true" [(ngModel)]="forfeitOnCredit" /> Add to their account (pay later)</label>
              </div>
            </div>
          </div>
        }

        @if (error()) { <p class="error">{{ error() }}</p> }

        <div class="modal-actions">
          <button class="btn-sm" (click)="adjustOpen.set(false)">Cancel</button>
          <button class="btn-primary" [disabled]="saving()" (click)="submitAdjust()">{{ saving() ? 'Saving…' : 'Save' }}</button>
        </div>
      </div>
    }
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .filter-group select { padding: 0.5rem 1rem; border: 1px solid #ddd; border-radius: 4px; font-size: 0.9rem; min-width: 250px; }
    .table-container { background: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow: hidden; }
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 0.75rem 1rem; text-align: left; border-bottom: 1px solid #eee; }
    th { background: #f8f9fa; font-weight: 600; }
    .highlight { background: #fff8e1; }
    .text-danger { color: #dc3545; font-weight: 600; }
    .btn-sm { padding: 0.3rem 0.6rem; border: 1px solid #ddd; border-radius: 4px; cursor: pointer; background: white; font-size: 0.8rem; }
    .btn-primary { background: #1a1a2e; color: white; border: none; padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; }
    .btn-primary:disabled { opacity: 0.6; }
    .overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.35); z-index: 40; }
    .modal { position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%); width: 440px; max-width: 92vw; background: white; border-radius: 10px; padding: 1.25rem 1.5rem; z-index: 50; box-shadow: 0 10px 40px rgba(0,0,0,0.2); }
    .modal h3 { margin: 0 0 0.25rem; font-size: 1rem; }
    .modal-sub { margin: 0 0 1rem; font-size: 0.85rem; color: #6b7280; }
    .form-row { margin-bottom: 0.9rem; }
    .form-row label { display: block; font-size: 0.8rem; font-weight: 600; margin-bottom: 0.25rem; }
    .form-row input, .form-row select { width: 100%; padding: 0.45rem 0.6rem; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .radio-row { display: flex; flex-direction: column; gap: 0.3rem; }
    .radio-row label { font-weight: 400; display: flex; align-items: center; gap: 0.4rem; font-size: 0.85rem; }
    .radio-row input { width: auto; }
    .settle-box { border: 1px dashed #ddd; border-radius: 6px; padding: 0.75rem; margin-bottom: 0.9rem; }
    .settle-title { font-size: 0.75rem; font-weight: 700; text-transform: uppercase; color: #6b7280; margin-bottom: 0.6rem; }
    .error { color: #dc3545; font-size: 0.85rem; }
    .modal-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem; }
  `],
})
export class CustomerCylinderLedgerComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<CustomerCylinderBalance[]>([]);
  customers = signal<Customer[]>([]);
  warehouses = signal<Warehouse[]>([]);
  selectedCustomerId = '';

  // Adjust dialog state
  adjustOpen = signal(false);
  adjustTarget = signal<CustomerCylinderBalance | null>(null);
  creditOrders = signal<any[]>([]);
  mode: 'return' | 'receive' | 'forfeit' = 'return';
  quantity = 1;
  settlementOrderId = '';
  settlementAmount = 0;
  forfeitWarehouseId = '';
  forfeitUnitPrice = 0;
  forfeitOnCredit = false;
  saving = signal(false);
  error = signal('');

  ngOnInit() {
    this.api.getAllList<Customer>('customers').subscribe(data => this.customers.set(data));
    this.api.getAllList<Warehouse>('warehouses').subscribe(data => this.warehouses.set(data));
    this.loadAll();
  }

  onCustomerChange() {
    if (this.selectedCustomerId) {
      this.api.get<CustomerCylinderBalance[]>('customercylinderledger/customer/' + this.selectedCustomerId)
        .subscribe(data => this.items.set(data));
    } else {
      this.loadAll();
    }
  }

  openAdjust(balance: CustomerCylinderBalance) {
    this.adjustTarget.set(balance);
    this.mode = 'return';
    this.quantity = Math.max(1, balance.outstanding);
    this.settlementOrderId = '';
    this.settlementAmount = 0;
    this.forfeitWarehouseId = '';
    this.forfeitUnitPrice = 0;
    this.forfeitOnCredit = false;
    this.error.set('');
    this.adjustOpen.set(true);

    // Delivered credit orders for this customer, with remaining due, for the settlement dropdown.
    this.api.getAll<any>('salesorders', 1, 200).subscribe(page => {
      const orders = page.items.filter((o: any) => o.customerId === balance.customerId && o.isCreditSale && o.status === 2);
      this.creditOrders.set(orders.map((o: any) => ({
        id: o.id,
        orderNumber: o.orderNumber,
        due: (o.totalAmount ?? 0) - (o.discount ?? 0),
      })));
    });
  }

  submitAdjust() {
    const target = this.adjustTarget();
    if (!target) return;
    if (!this.quantity || this.quantity <= 0) {
      this.error.set('Quantity must be positive.');
      return;
    }

    this.saving.set(true);
    this.error.set('');

    const req$ = this.mode === 'forfeit'
      ? (() => {
          if (!this.forfeitWarehouseId) {
            this.error.set('Select a warehouse.');
            this.saving.set(false);
            return null;
          }
          if (!this.forfeitUnitPrice || this.forfeitUnitPrice <= 0) {
            this.error.set('Enter a price per cylinder.');
            this.saving.set(false);
            return null;
          }
          return this.api.post('customercylinderledger/forfeit', {
            customerId: target.customerId,
            brandId: target.brandId,
            cylinderSizeId: target.cylinderSizeId,
            warehouseId: this.forfeitWarehouseId,
            quantity: this.quantity,
            unitPrice: this.forfeitUnitPrice,
            isCreditSale: this.forfeitOnCredit,
          });
        })()
      : this.api.post('customercylinderledger/adjust', {
          customerId: target.customerId,
          brandId: target.brandId,
          cylinderSizeId: target.cylinderSizeId,
          quantity: this.quantity,
          isReturn: this.mode === 'return',
          settlementSalesOrderId: this.mode === 'return' && this.settlementOrderId ? this.settlementOrderId : null,
          settlementAmount: this.mode === 'return' && this.settlementOrderId ? (this.settlementAmount || 0) : 0,
        });

    if (!req$) return;

    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.adjustOpen.set(false);
        this.onCustomerChange();
      },
      error: (e) => {
        this.saving.set(false);
        this.error.set(e?.error?.message ?? e?.error?.errors?.[0] ?? 'Adjustment failed.');
      },
    });
  }

  private loadAll() {
    this.api.getAll<CustomerCylinderBalance>('customercylinderledger', 1, 10000)
      .subscribe(data => this.items.set(data.items));
  }
}
