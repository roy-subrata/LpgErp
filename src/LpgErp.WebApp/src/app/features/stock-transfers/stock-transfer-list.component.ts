import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { StockMovement } from '../../core/models';
import { StockTransferFormComponent } from './stock-transfer-form.component';

@Component({
  selector: 'app-stock-transfer-list',
  standalone: true,
  imports: [CommonModule, StockTransferFormComponent],
  template: `
    <div class="page-header">
      <h1>Stock Transfers</h1>
      <button class="btn-primary" (click)="showForm.set(true)">+ New Transfer</button>
    </div>
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Product</th>
            <th>From</th>
            <th>To</th>
            <th>Quantity</th>
            <th>Reference</th>
          </tr>
        </thead>
        <tbody>
          @for (m of items(); track m.id) {
            <tr>
              <td>{{ m.movementDate | date:'short' }}</td>
              <td>{{ m.productName }}</td>
              <td>{{ m.fromWarehouseName }}</td>
              <td>{{ m.toWarehouseName }}</td>
              <td>{{ m.quantity }}</td>
              <td>{{ m.reference }}</td>
            </tr>
          } @empty {
            <tr><td colspan="6">No stock movements found.</td></tr>
          }
        </tbody>
      </table>
    </div>
    <app-stock-transfer-form [open]="showForm()" (close)="showForm.set(false)" (saved)="loadData()" />
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .btn-primary { background: #1a1a2e; color: white; border: none; padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; }
    .table-container { background: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow: hidden; }
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 0.75rem 1rem; text-align: left; border-bottom: 1px solid #eee; }
    th { background: #f8f9fa; font-weight: 600; }
  `],
})
export class StockTransferListComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<StockMovement[]>([]);
  showForm = signal(false);

  ngOnInit() { this.loadData(); }

  loadData() {
    this.api.getAll<StockMovement>('stocktransfer/history').subscribe(data => this.items.set(data.items));
  }
}
