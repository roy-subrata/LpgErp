import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { PriceHistory, Product } from '../../core/models';
import { DialogComponent } from '../../shared/dialog.component';

@Component({
  selector: 'app-price-history-list',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <div class="page-header">
      <h2>Price History</h2>
      <button class="btn btn-primary" (click)="openCreate()">+ Record Price Change</button>
    </div>

    <div class="filters">
      <select [(ngModel)]="selectedProductId" (change)="loadHistory()">
        <option value="">All Products</option>
        @for (p of products(); track p.id) {
          <option [value]="p.id">{{ p.name }}</option>
        }
      </select>
    </div>

    <table class="table">
      <thead>
        <tr>
          <th>Date</th>
          <th>Product</th>
          <th>Type</th>
          <th>Previous</th>
          <th>New</th>
          <th>Change</th>
          <th>Reason</th>
          <th>Reference</th>
        </tr>
      </thead>
      <tbody>
        @for (item of items(); track item.id) {
          <tr>
            <td>{{ item.effectiveDate | date:'dd MMM yyyy' }}</td>
            <td>{{ item.productName }}</td>
            <td><span class="badge" [class]="item.priceType === 0 ? 'badge-blue' : 'badge-green'">{{ item.priceType === 0 ? 'Purchase' : 'Sale' }}</span></td>
            <td>{{ item.previousPrice | number:'1.2-2' }}</td>
            <td>{{ item.newPrice | number:'1.2-2' }}</td>
            <td [class]="item.changeAmount > 0 ? 'text-red' : 'text-green'">
              {{ item.changeAmount > 0 ? '+' : '' }}{{ item.changeAmount | number:'1.2-2' }}
              <small>({{ item.changePercent | number:'1.1-1' }}%)</small>
            </td>
            <td>{{ reasonLabel(item.reason) }}</td>
            <td>{{ item.reference || '-' }}</td>
          </tr>
        } @empty {
          <tr><td colspan="8" class="text-center">No price history found</td></tr>
        }
      </tbody>
    </table>

    <div class="pagination">
      <button [disabled]="pageNumber() <= 1" (click)="goPage(pageNumber() - 1)">Previous</button>
      <span>Page {{ pageNumber() }} of {{ totalPages() }}</span>
      <button [disabled]="pageNumber() >= totalPages()" (click)="goPage(pageNumber() + 1)">Next</button>
    </div>

    <app-dialog [open]="dialogOpen()" title="Record Price Change" (close)="dialogOpen.set(false)">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label>Product</label>
          <select [(ngModel)]="form.productId" name="productId" required>
            <option value="">-- Select Product --</option>
            @for (p of products(); track p.id) {
              <option [value]="p.id">{{ p.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label>Price Type</label>
          <select [(ngModel)]="form.priceType" name="priceType">
            <option [ngValue]="0">Purchase Price</option>
            <option [ngValue]="1">Sale Price</option>
          </select>
        </div>
        <div class="form-group">
          <label>New Price</label>
          <input type="number" [(ngModel)]="form.newPrice" name="newPrice" step="0.01" required />
        </div>
        <div class="form-group">
          <label>Reason</label>
          <select [(ngModel)]="form.reason" name="reason">
            <option [ngValue]="0">Supplier Price Change</option>
            <option [ngValue]="1">Regulatory Board Order</option>
            <option [ngValue]="2">Market Adjustment</option>
            <option [ngValue]="3">Manual Correction</option>
            <option [ngValue]="4">Other</option>
          </select>
        </div>
        <div class="form-group">
          <label>Effective Date</label>
          <input type="date" [(ngModel)]="form.effectiveDate" name="effectiveDate" required />
        </div>
        <div class="form-group">
          <label>Reference</label>
          <input type="text" [(ngModel)]="form.reference" name="reference" placeholder="e.g. Reg Board Order No." />
        </div>
        <div class="form-group">
          <label>Notes</label>
          <textarea [(ngModel)]="form.notes" name="notes" rows="2"></textarea>
        </div>
        <div class="form-actions">
          <button type="button" class="btn btn-secondary" (click)="dialogOpen.set(false)">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="saving()">Save</button>
        </div>
      </form>
    </app-dialog>
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .page-header h2 { margin: 0; }
    .filters { margin-bottom: 1rem; }
    .filters select { padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; min-width: 200px; }
    .table { width: 100%; border-collapse: collapse; }
    .table th, .table td { padding: 0.75rem; text-align: left; border-bottom: 1px solid #eee; }
    .table th { font-weight: 600; font-size: 0.85rem; color: #666; }
    .badge { padding: 0.2rem 0.6rem; border-radius: 12px; font-size: 0.75rem; font-weight: 600; }
    .badge-blue { background: #eff6ff; color: #1d4ed8; }
    .badge-green { background: #f0fdf4; color: #15803d; }
    .text-red { color: #dc2626; }
    .text-green { color: #16a34a; }
    .text-center { text-align: center; color: #999; }
    .pagination { display: flex; justify-content: center; align-items: center; gap: 1rem; margin-top: 1.5rem; }
    .pagination button { padding: 0.4rem 1rem; border: 1px solid #ddd; border-radius: 4px; cursor: pointer; }
    .pagination button:disabled { opacity: 0.5; cursor: not-allowed; }
    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; margin-bottom: 0.25rem; font-weight: 600; font-size: 0.9rem; }
    .form-group input, .form-group select, .form-group textarea { width: 100%; padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.5rem; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; border: 1px solid #ddd; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .btn-secondary { background: white; color: #333; }
  `]
})
export class PriceHistoryListComponent implements OnInit {
  private api = inject(ApiService);

  items = signal<PriceHistory[]>([]);
  products = signal<Product[]>([]);
  dialogOpen = signal(false);
  saving = signal(false);
  pageNumber = signal(1);
  totalPages = signal(1);
  selectedProductId = '';

  form = {
    productId: '',
    priceType: 0,
    newPrice: 0,
    reason: 0,
    effectiveDate: new Date().toISOString().split('T')[0],
    reference: '',
    notes: ''
  };

  ngOnInit() {
    this.loadProducts();
    this.loadHistory();
  }

  loadProducts() {
    this.api.getAllList<Product>('products').subscribe(data => this.products.set(data));
  }

  loadHistory() {
    const params: any = { pageNumber: this.pageNumber(), pageSize: 20 };
    if (this.selectedProductId) {
      this.api.getAll<PriceHistory>(`pricehistories/product/${this.selectedProductId}`).subscribe(data => {
        this.items.set(Array.isArray(data) ? data : (data as any).items || []);
        this.totalPages.set(1);
      });
    } else {
      this.api.getAll<PriceHistory>('pricehistories').subscribe(data => {
        this.items.set(data.items);
        this.totalPages.set(data.pagination?.totalPages || 1);
      });
    }
  }

  goPage(page: number) {
    this.pageNumber.set(page);
    this.loadHistory();
  }

  openCreate() {
    this.form = {
      productId: '',
      priceType: 0,
      newPrice: 0,
      reason: 0,
      effectiveDate: new Date().toISOString().split('T')[0],
      reference: '',
      notes: ''
    };
    this.dialogOpen.set(true);
  }

  submit() {
    this.saving.set(true);
    this.api.create('pricehistories', {
      ...this.form,
      productId: this.form.productId,
      effectiveDate: new Date(this.form.effectiveDate).toISOString()
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.dialogOpen.set(false);
        this.loadHistory();
      },
      error: () => this.saving.set(false)
    });
  }

  reasonLabel(reason: number): string {
    const labels: Record<number, string> = {
      0: 'Supplier Change',
      1: 'Regulatory Board',
      2: 'Market Adjustment',
      3: 'Manual Correction',
      4: 'Other'
    };
    return labels[reason] || 'Unknown';
  }
}
