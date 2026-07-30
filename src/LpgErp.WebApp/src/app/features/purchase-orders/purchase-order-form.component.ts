import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Supplier, Warehouse, Product, TransportCompany, PaymentAccount } from '../../core/models';
import { PAYMENT_METHOD_OPTIONS, requiresAccount } from '../../core/payment-methods';

interface OrderItem {
  productId: string;
  orderedQuantity: number;
  unitPrice: number;
  /** Empties promised to the company. Blank = one per refill, the normal swap. */
  emptyReturnedQuantity: number | null;
}

interface LeakageItem {
  brandId: string;
  cylinderSizeId: string;
  quantity: number;
  resolution: number;
  creditAmount: number;
}

@Component({
  selector: 'app-purchase-order-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Purchase Order' : 'New Purchase Order'" (close)="onClose()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label for="supplierId">Supplier</label>
          <select id="supplierId" [ngModel]="supplierId" (ngModelChange)="onSupplierChange($event)" name="supplierId" required>
            <option value="">-- Select --</option>
            @for (s of suppliers(); track s.id) {
              <option [value]="s.id">{{ s.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="warehouseId">Warehouse</label>
          <select id="warehouseId" [(ngModel)]="warehouseId" name="warehouseId" required>
            <option value="">-- Select --</option>
            @for (w of warehouses(); track w.id) {
              <option [value]="w.id">{{ w.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="expectedDeliveryDate">Expected Delivery Date</label>
          <input id="expectedDeliveryDate" type="date" [(ngModel)]="expectedDeliveryDate" name="expectedDeliveryDate" required />
        </div>
        <div class="form-group">
          <label for="notes">Notes</label>
          <input id="notes" type="text" [(ngModel)]="notes" name="notes" />
        </div>
        <div class="form-group">
          <label for="transportCompanyId">Transport Company</label>
          <select id="transportCompanyId" [(ngModel)]="transportCompanyId" name="transportCompanyId">
            <option value="">-- Select --</option>
            @for (tc of transportCompanies(); track tc.id) {
              <option [value]="tc.id">{{ tc.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="transportationCost">Transportation Cost</label>
          <input id="transportationCost" type="number" [(ngModel)]="transportationCost" name="transportationCost" />
        </div>
        <div class="form-group">
          <label for="dueDate">Due Date</label>
          <input id="dueDate" type="date" [(ngModel)]="dueDate" name="dueDate" />
        </div>
        <h4>Order Items</h4>
        @for (item of items; track $index) {
          <div class="item-row">
            <select [(ngModel)]="item.productId" [name]="'productId_' + $index" required>
              <option value="">-- Product --</option>
              @for (p of products(); track p.id) {
                <option [value]="p.id">{{ p.name }}</option>
              }
            </select>
            <input type="number" placeholder="Qty" [(ngModel)]="item.orderedQuantity" [name]="'qty_' + $index" required />
            <input type="number" placeholder="Price" [(ngModel)]="item.unitPrice" [name]="'price_' + $index" required step="0.01" />
            @if (isRefill(item)) {
              <input type="number" min="0" placeholder="Empties (=qty)"
                     title="Empty cylinders you send back. Blank = one per refill; fewer means cylinders owed."
                     [(ngModel)]="item.emptyReturnedQuantity" [name]="'empty_' + $index" />
            }
            <button type="button" class="btn-remove" (click)="removeItem($index)">&times;</button>
          </div>
        }
        <button type="button" class="btn-add" (click)="addItem()">+ Add Item</button>

        <h4>Leaking Cylinders Returned <span class="optional">— sent back to the company</span></h4>
        @for (leak of leakages; track $index) {
          <div class="item-row">
            <select [(ngModel)]="leak.brandId" [name]="'lbrand_' + $index">
              <option value="">-- Brand --</option>
              @for (b of brands(); track b.id) {
                <option [value]="b.id">{{ b.name }}</option>
              }
            </select>
            <select [(ngModel)]="leak.cylinderSizeId" [name]="'lsize_' + $index">
              <option value="">-- Size --</option>
              @for (sz of cylinderSizes(); track sz.id) {
                <option [value]="sz.id">{{ sz.name }}</option>
              }
            </select>
            <input type="number" min="1" placeholder="Qty" [(ngModel)]="leak.quantity" [name]="'lqty_' + $index" />
            <select [(ngModel)]="leak.resolution" [name]="'lres_' + $index">
              <option [ngValue]="0">Free refill</option>
              <option [ngValue]="1">Credit on bill</option>
              <option [ngValue]="2">Replacement</option>
            </select>
            @if (leak.resolution == 1) {
              <input type="number" min="0" step="0.01" placeholder="Credit ৳" [(ngModel)]="leak.creditAmount" [name]="'lcredit_' + $index" />
            }
            <button type="button" class="btn-remove" (click)="removeLeakage($index)">&times;</button>
          </div>
        }
        <button type="button" class="btn-add" (click)="addLeakage()">+ Add Leakage</button>

        @if (availableCommissionBalance() > 0) {
          <div class="form-group">
            <label for="commissionCreditInput">
              Commission Credit to Apply
              <span class="optional">— ৳{{ availableCommissionBalance() | number:'1.0-2' }} available, optional and varies per order</span>
            </label>
            <div class="commission-row">
              <input id="commissionCreditInput" type="number" min="0" [max]="maxCommissionCredit()" step="0.01"
                     [ngModel]="commissionCreditInput" (ngModelChange)="onCommissionInputChange($event)" name="commissionCreditInput" />
              <button type="button" class="btn-max" (click)="onCommissionInputChange(maxCommissionCredit())">Use max</button>
            </div>
            @if (commissionCreditInput > maxCommissionCredit()) {
              <small class="field-hint warn">This is more than what's available for this order (৳{{ maxCommissionCredit() | number:'1.0-2' }}) and won't be accepted.</small>
            }
          </div>
        }

        @if (!entityId) {
          <h4>Payment <span class="optional">— leave the amount at 0 if nothing was paid yet</span></h4>
          <div class="form-group">
            <label for="paidAmount">Amount Paid</label>
            <input id="paidAmount" type="number" min="0" [max]="netPayable()" step="0.01" [(ngModel)]="paidAmount" name="paidAmount" />
            @if (commissionApplied() > 0) {
              <small class="field-hint">
                Order ৳{{ orderTotal() | number:'1.0-2' }} + transport ৳{{ transportationCost | number:'1.0-2' }}
                − ৳{{ commissionApplied() | number:'1.0-2' }} supplier commission credit (applied)
                = <strong>payable ৳{{ netPayable() | number:'1.0-2' }}</strong>
              </small>
            } @else {
              <small class="field-hint">Payable is ৳{{ netPayable() | number:'1.0-2' }} (order + transport).</small>
            }
            @if (paidAmount > netPayable()) {
              <small class="field-hint warn">This is more than the payable amount and won't be accepted.</small>
            }
          </div>
          @if (paidAmount > 0) {
            <div class="form-group">
              <label for="payMethod">Paid By</label>
              <select id="payMethod" [(ngModel)]="payMethod" name="payMethod" (ngModelChange)="onMethodChange()">
                @for (m of methodOptions; track m.value) {
                  <option [ngValue]="m.value">{{ m.label }}</option>
                }
              </select>
            </div>
            <div class="form-group">
              <label for="payAccountId">Account</label>
              <select id="payAccountId" [(ngModel)]="payAccountId" name="payAccountId">
                <option value="">{{ accountRequired() ? '-- Select --' : '-- None (cash in hand) --' }}</option>
                @for (a of accountsForMethod(); track a.id) {
                  <option [value]="a.id">{{ a.name }}{{ a.accountNumber ? ' · ' + a.accountNumber : '' }}</option>
                }
              </select>
              @if (accountRequired() && accountsForMethod().length === 0) {
                <small class="field-hint">No active account for this method — add one under Transactions → Payment Accounts.</small>
              }
            </div>
            <div class="form-group">
              <label for="payReference">Reference</label>
              <input id="payReference" type="text" [(ngModel)]="payReference" name="payReference" placeholder="bKash TrxID / cheque no." />
            </div>
          }
        }

        @if (error()) { <p class="error">{{ error() }}</p> }
        <div class="form-actions">
          <button type="button" class="btn btn-secondary" (click)="onClose()">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="saving()">{{ saving() ? 'Saving...' : entityId ? 'Update' : 'Create' }}</button>
        </div>
      </form>
    </app-dialog>
  `,
  styles: [`
    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; margin-bottom: 0.25rem; font-weight: 600; font-size: 0.9rem; }
    .form-group input, .form-group select { width: 100%; padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .item-row { display: flex; gap: 0.5rem; margin-bottom: 0.5rem; align-items: center; }
    .item-row select, .item-row input { flex: 1; padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; font: inherit; }
    .btn-remove { background: #dc3545; color: white; border: none; border-radius: 4px; padding: 0.5rem; cursor: pointer; flex-shrink: 0; }
    .btn-add { background: #28a745; color: white; border: none; padding: 0.4rem 0.8rem; border-radius: 4px; cursor: pointer; margin-bottom: 1rem; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.5rem; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; border: 1px solid #ddd; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .btn-secondary { background: white; color: #333; }
    h4 { margin: 1rem 0 0.5rem; font-size: 0.95rem; color: #555; }
    h4 .optional { font-weight: 400; color: #6b7280; font-size: 0.8rem; }
    .field-hint { display: block; margin-top: 0.25rem; font-size: 0.75rem; color: #6b7280; }
    .field-hint.warn { color: #b45309; font-weight: 600; }
    .error { color: #dc3545; font-size: 0.85rem; margin-top: 0.75rem; }
    .commission-row { display: flex; gap: 0.5rem; }
    .btn-max { flex-shrink: 0; background: #f4f5f7; border: 1px solid #ddd; border-radius: 4px; padding: 0 0.75rem; cursor: pointer; font-size: 0.8rem; }
  `],
})
export class PurchaseOrderFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  supplierId = '';
  warehouseId = '';
  expectedDeliveryDate = '';
  notes = '';
  transportCompanyId = '';
  transportationCost = 0;
  dueDate = '';
  items: OrderItem[] = [{ productId: '', orderedQuantity: 0, unitPrice: 0, emptyReturnedQuantity: null }];
  leakages: LeakageItem[] = [];
  /** How much of the supplier's commission balance to draw down against this order — defaults from
   *  the supplier's balance but is freely overridable, since it's optional and varies per order. */
  commissionCreditInput = 0;
  /** The order's own commission draw at load time (edit mode) — refunded server-side before re-applying,
   *  so it still counts as "available" as long as the supplier on the order hasn't changed. */
  private originalCommissionApplied = 0;
  private originalSupplierId = '';
  paidAmount = 0;
  payMethod = 0;
  payAccountId = '';
  payReference = '';
  suppliers = signal<Supplier[]>([]);
  brands = signal<any[]>([]);
  cylinderSizes = signal<any[]>([]);
  warehouses = signal<Warehouse[]>([]);
  products = signal<Product[]>([]);
  transportCompanies = signal<TransportCompany[]>([]);
  paymentAccounts = signal<PaymentAccount[]>([]);
  saving = signal(false);
  error = signal('');

  readonly methodOptions = PAYMENT_METHOD_OPTIONS;

  orderTotal(): number {
    return this.items.reduce((sum, i) => sum + (Number(i.orderedQuantity) || 0) * (Number(i.unitPrice) || 0), 0);
  }

  /** Total leakage credit across "Credit on bill" lines — reduces what's payable, same as commission. */
  private leakageCreditTotal(): number {
    return this.leakages
      .filter(l => Number(l.resolution) === 1)
      .reduce((sum, l) => sum + (Number(l.creditAmount) || 0), 0);
  }

  /**
   * The supplier's raw commission balance, plus whatever this order itself is currently drawing
   * (edit mode only, same supplier) — the backend refunds that amount before re-applying, so it's
   * still "available" to this order even though it's no longer sitting on the supplier's balance.
   */
  availableCommissionBalance(): number {
    const supplier = this.suppliers().find(s => s.id === this.supplierId);
    if (!supplier) return 0;
    const refundable = this.entityId && this.originalSupplierId === this.supplierId ? this.originalCommissionApplied : 0;
    return (supplier.commissionBalance ?? 0) + refundable;
  }

  /** The most that can actually be applied to this specific order right now. */
  maxCommissionCredit(): number {
    const payable = this.orderTotal() + (Number(this.transportationCost) || 0);
    return Math.max(0, Math.min(this.availableCommissionBalance(), payable));
  }

  /**
   * What the user has entered, defaulting from the supplier's balance but freely overridable —
   * commission credit is optional and varies per order, not an automatic fixed deduction.
   * Clamped here only for the payable preview; the raw entered value is validated on submit.
   */
  commissionApplied(): number {
    return Math.min(Math.max(Number(this.commissionCreditInput) || 0, 0), this.maxCommissionCredit());
  }

  onSupplierChange(supplierId: string) {
    this.supplierId = supplierId;
    this.commissionCreditInput = this.maxCommissionCredit();
  }

  onCommissionInputChange(value: number) {
    this.commissionCreditInput = Number(value) || 0;
  }

  netPayable(): number {
    const payable = this.orderTotal() + (Number(this.transportationCost) || 0)
      - this.commissionApplied() - this.leakageCreditTotal();
    return Math.max(0, payable);
  }

  /** Only accounts of the selected method — a bKash payment can't come out of a bank account. */
  accountsForMethod(): PaymentAccount[] {
    return this.paymentAccounts().filter(a => a.method === Number(this.payMethod));
  }

  accountRequired(): boolean {
    return requiresAccount(Number(this.payMethod));
  }

  onMethodChange() {
    if (!this.accountsForMethod().some(a => a.id === this.payAccountId)) {
      this.payAccountId = '';
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      this.resetForm();
      this.api.getAllList<PaymentAccount>('paymentaccounts').subscribe(data => this.paymentAccounts.set(data));
      this.api.getAllList<Supplier>('suppliers').subscribe(data => this.suppliers.set(data));
      this.api.getAllList<any>('brands').subscribe(data => this.brands.set(data));
      this.api.getAllList<any>('cylindersizes').subscribe(data => this.cylinderSizes.set(data));
      this.api.getAllList<Warehouse>('warehouses').subscribe(data => this.warehouses.set(data));
      this.api.getAllList<Product>('products').subscribe(data => this.products.set(data));
      this.api.getAllList<TransportCompany>('transportcompanies').subscribe(data => this.transportCompanies.set(data));
      if (this.entityId) {
        this.api.getById<any>('purchaseorders', this.entityId).subscribe(po => {
          this.supplierId = po.supplierId ?? '';
          this.warehouseId = po.warehouseId ?? '';
          this.expectedDeliveryDate = po.expectedDeliveryDate?.split('T')[0] ?? '';
          this.notes = po.notes ?? '';
          this.transportCompanyId = po.transportCompanyId ?? '';
          this.transportationCost = po.transportationCost ?? 0;
          this.dueDate = po.dueDate?.split('T')[0] ?? '';
          this.commissionCreditInput = po.commissionApplied ?? 0;
          this.originalCommissionApplied = po.commissionApplied ?? 0;
          this.originalSupplierId = po.supplierId ?? '';
          if (po.items?.length) {
            this.items = po.items.map((i: any) => ({
              productId: i.productId,
              orderedQuantity: i.orderedQuantity,
              unitPrice: i.unitPrice,
              emptyReturnedQuantity: i.emptyReturnedQuantity ?? null,
            }));
          }
        });
      }
    }
  }

  addItem() {
    this.items.push({ productId: '', orderedQuantity: 0, unitPrice: 0, emptyReturnedQuantity: null });
  }

  addLeakage() {
    this.leakages.push({ brandId: '', cylinderSizeId: '', quantity: 0, resolution: 0, creditAmount: 0 });
  }

  removeLeakage(index: number) {
    this.leakages.splice(index, 1);
  }

  /** Empties only apply to gas refills — a package or accessory has nothing to swap. */
  isRefill(item: OrderItem): boolean {
    return this.products().find(p => p.id === item.productId)?.type === 1;
  }

  removeItem(index: number) {
    if (this.items.length > 1) {
      this.items.splice(index, 1);
    }
  }

  submit() {
    this.error.set('');

    // Caught here rather than left to the backend round-trip: the max is the real ceiling for
    // what this order can draw down right now, same rule the server enforces.
    if (Number(this.commissionCreditInput) > this.maxCommissionCredit()) {
      this.error.set(`Commission credit (৳${Number(this.commissionCreditInput).toFixed(2)}) exceeds what's available for this order (৳${this.maxCommissionCredit().toFixed(2)}).`);
      return;
    }

    // Caught here rather than left to the backend round-trip: the payable amount already
    // accounts for the commission credit entered above, so this is the real ceiling, not just
    // a copy of the server-side check.
    if (!this.entityId && Number(this.paidAmount) > this.netPayable()) {
      this.error.set(`Payment (৳${this.paidAmount.toFixed(2)}) exceeds the payable amount (৳${this.netPayable().toFixed(2)}).`);
      return;
    }

    this.saving.set(true);
    const body = {
      supplierId: this.supplierId,
      warehouseId: this.warehouseId,
      expectedDeliveryDate: this.expectedDeliveryDate,
      notes: this.notes,
      transportCompanyId: this.transportCompanyId,
      transportationCost: this.transportationCost,
      dueDate: this.dueDate,
      commissionCreditApplied: Number(this.commissionCreditInput) || 0,
      items: this.items.map(i => ({
        productId: i.productId,
        orderedQuantity: i.orderedQuantity,
        unitPrice: i.unitPrice,
        emptyReturnedQuantity: i.emptyReturnedQuantity === null || (i.emptyReturnedQuantity as any) === ''
          ? null
          : Number(i.emptyReturnedQuantity),
      })),
      leakages: this.leakages
        .filter(l => l.brandId && l.cylinderSizeId && l.quantity > 0)
        .map(l => ({
          brandId: l.brandId,
          cylinderSizeId: l.cylinderSizeId,
          quantity: Number(l.quantity),
          resolution: Number(l.resolution),
          creditAmount: Number(l.resolution) === 1 ? Number(l.creditAmount) : 0,
        })),
      // Only sent on create — an existing order's payments are edited from the Payments screen.
      payment: !this.entityId && Number(this.paidAmount) > 0
        ? {
            amount: Number(this.paidAmount),
            method: Number(this.payMethod),
            paymentAccountId: this.payAccountId || null,
            reference: this.payReference || null,
          }
        : null,
    };

    const req$ = this.entityId
      ? this.api.update('purchaseorders', this.entityId, body)
      : this.api.create('purchaseorders', body);

    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.saved.emit();
        this.resetForm();
      },
      error: (e) => {
        this.saving.set(false);
        this.error.set(e?.error?.errors?.[0] ?? e?.error?.message ?? 'Could not save this order.');
      },
    });
  }

  onClose() {
    this.resetForm();
    this.close.emit();
  }

  private resetForm() {
    this.supplierId = '';
    this.warehouseId = '';
    this.expectedDeliveryDate = '';
    this.notes = '';
    this.transportCompanyId = '';
    this.transportationCost = 0;
    this.dueDate = '';
    this.items = [{ productId: '', orderedQuantity: 0, unitPrice: 0, emptyReturnedQuantity: null }];
    this.leakages = [];
    this.commissionCreditInput = 0;
    this.originalCommissionApplied = 0;
    this.originalSupplierId = '';
    this.paidAmount = 0;
    this.payMethod = 0;
    this.payAccountId = '';
    this.payReference = '';
    this.error.set('');
  }
}
