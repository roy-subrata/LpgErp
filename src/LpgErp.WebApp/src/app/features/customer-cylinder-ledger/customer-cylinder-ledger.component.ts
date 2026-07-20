import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { CustomerCylinderBalance, Customer } from '../../core/models';

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
            <th>Outstanding</th>
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
              <td [class.text-danger]="b.outstanding > 0">{{ b.outstanding }}</td>
            </tr>
          } @empty {
            <tr><td colspan="6">No cylinder balances found.</td></tr>
          }
        </tbody>
      </table>
    </div>
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
  `],
})
export class CustomerCylinderLedgerComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<CustomerCylinderBalance[]>([]);
  customers = signal<Customer[]>([]);
  selectedCustomerId = '';

  ngOnInit() {
    this.api.getAllList<Customer>('customers').subscribe(data => this.customers.set(data));
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

  private loadAll() {
    this.api.getAll<CustomerCylinderBalance>('customercylinderledger', 1, 10000)
      .subscribe(data => this.items.set(data.items));
  }
}
