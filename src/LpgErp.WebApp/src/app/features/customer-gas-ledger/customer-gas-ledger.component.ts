import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { CustomerGasLedger } from '../../core/models';
import { Customer } from '../../core/models';

@Component({
  selector: 'app-customer-gas-ledger',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <h1>Customer Gas Ledger</h1>
    </div>
    <div class="filter-bar">
      <label>Customer:</label>
      <select [(ngModel)]="selectedCustomerId" (change)="loadLedger()">
        <option value="">-- Select Customer --</option>
        @for (c of customers(); track c.id) {
          <option [value]="c.id">{{ c.name }}</option>
        }
      </select>
    </div>

    @if (ledger()) {
      <div class="summary-grid">
        <div class="summary-card">
          <label>Gas Purchases</label>
          <span>{{ ledger()!.totalGasPurchases | number:'1.2-2' }}</span>
        </div>
        <div class="summary-card">
          <label>Cylinder Purchases</label>
          <span>{{ ledger()!.totalCylinderPurchases | number:'1.2-2' }}</span>
        </div>
        <div class="summary-card">
          <label>Total Payments</label>
          <span>{{ ledger()!.totalPayments | number:'1.2-2' }}</span>
        </div>
        <div class="summary-card">
          <label>Outstanding</label>
          <span [class.text-danger]="ledger()!.outstandingBalance > 0">{{ ledger()!.outstandingBalance | number:'1.2-2' }}</span>
        </div>
        <div class="summary-card">
          <label>Deposits</label>
          <span>{{ ledger()!.totalDeposits | number:'1.2-2' }}</span>
        </div>
      </div>

      <div class="table-container">
        <h3>Recent Transactions</h3>
        <table>
          <thead>
            <tr>
              <th>Date</th>
              <th>Description</th>
              <th>Debit</th>
              <th>Credit</th>
              <th>Balance</th>
            </tr>
          </thead>
          <tbody>
            @for (t of ledger()!.recentTransactions; track $index) {
              <tr>
                <td>{{ t.date | date:'short' }}</td>
                <td>{{ t.description }}</td>
                <td>{{ t.debit | number:'1.2-2' }}</td>
                <td>{{ t.credit | number:'1.2-2' }}</td>
                <td [class.text-danger]="t.runningBalance > 0">{{ t.runningBalance | number:'1.2-2' }}</td>
              </tr>
            } @empty {
              <tr><td colspan="5">No transactions found.</td></tr>
            }
          </tbody>
        </table>
      </div>
    }
  `,
  styles: [`
    .page-header { margin-bottom: 1.5rem; }
    .filter-bar { background: white; padding: 1rem; border-radius: 8px; margin-bottom: 1.5rem; display: flex; align-items: center; gap: 1rem; }
    .filter-bar select { padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; min-width: 250px; }
    .summary-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
    .summary-card { background: white; border-radius: 8px; padding: 1rem; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
    .summary-card label { display: block; font-size: 0.85rem; color: #666; margin-bottom: 0.25rem; }
    .summary-card span { font-size: 1.2rem; font-weight: 600; }
    .table-container { background: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow: hidden; padding: 1rem; }
    .table-container h3 { margin: 0 0 1rem; }
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 0.75rem 1rem; text-align: left; border-bottom: 1px solid #eee; }
    th { background: #f8f9fa; font-weight: 600; }
    .text-danger { color: #dc3545; font-weight: 600; }
  `],
})
export class CustomerGasLedgerComponent implements OnInit {
  private api = inject(ApiService);
  customers = signal<Customer[]>([]);
  ledger = signal<CustomerGasLedger | null>(null);
  selectedCustomerId = '';

  ngOnInit() {
    this.api.getAllList<Customer>('customers').subscribe(data => this.customers.set(data));
  }

  loadLedger() {
    if (!this.selectedCustomerId) { this.ledger.set(null); return; }
    this.api.get<CustomerGasLedger>(`customergasledger/customer/${this.selectedCustomerId}`).subscribe(data => this.ledger.set(data));
  }
}
