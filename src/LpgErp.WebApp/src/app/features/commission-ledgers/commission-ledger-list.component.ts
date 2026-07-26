import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { CommissionLedger } from '../../core/commission.models';
import { CommissionPolicy } from '../../core/commission.models';

@Component({
  selector: 'app-commission-ledger-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <h2>Commission Ledger</h2>
    </div>

    <div class="filters">
      <select [(ngModel)]="filterType" (change)="loadLedger()">
        <option [ngValue]="-1">All Types</option>
        <option [ngValue]="0">Salesman</option>
        <option [ngValue]="1">Customer</option>
        <option [ngValue]="2">Supplier</option>
      </select>
      <select [(ngModel)]="filterStatus" (change)="loadLedger()">
        <option [ngValue]="-1">All Status</option>
        <option [ngValue]="0">Pending</option>
        <option [ngValue]="1">Earned</option>
        <option [ngValue]="2">Applied</option>
        <option [ngValue]="3">Expired</option>
      </select>
    </div>

    <table class="table">
      <thead>
        <tr>
          <th>Period</th>
          <th>Type</th>
          <th>Policy</th>
          <th>Quantity</th>
          <th>Amount</th>
          <th>Commission</th>
          <th>Status</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        @for (item of items(); track item.id) {
          <tr>
            <td>
              <div>{{ item.periodKey }}</div>
              <div class="period-dates">{{ item.periodStart | date:'dd MMM' }} - {{ item.periodEnd | date:'dd MMM yyyy' }}</div>
            </td>
            <td><span class="badge" [class]="entityBadge(item.entityType)">{{ entityLabel(item.entityType) }}</span></td>
            <td>{{ item.policyName }}</td>
            <td>{{ item.actualQuantity }}</td>
            <td>৳{{ item.actualAmount | number:'1.0-0' }}</td>
            <td class="commission">৳{{ item.commissionEarned | number:'1.2-2' }}</td>
            <td><span class="badge" [class]="statusBadge(item.status)">{{ statusLabel(item.status) }}</span></td>
            <td>
              @if (item.status === 1) {
                <button class="btn btn-sm btn-primary" (click)="settle(item)">Settle</button>
              }
            </td>
          </tr>
        } @empty {
          <tr><td colspan="8" class="text-center">No commission entries found</td></tr>
        }
      </tbody>
    </table>

    <div class="summary" *ngIf="items().length > 0">
      <div class="summary-item">
        <span>Total Earned:</span>
        <strong>৳{{ totalEarned() | number:'1.2-2' }}</strong>
      </div>
      <div class="summary-item">
        <span>Pending Settlement:</span>
        <strong>৳{{ totalPending() | number:'1.2-2' }}</strong>
      </div>
    </div>
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .page-header h2 { margin: 0; }
    .filters { display: flex; gap: 1rem; margin-bottom: 1rem; }
    .filters select { padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; }
    .table { width: 100%; border-collapse: collapse; }
    .table th, .table td { padding: 0.75rem; text-align: left; border-bottom: 1px solid #eee; }
    .table th { font-weight: 600; font-size: 0.85rem; color: #666; }
    .period-dates { font-size: 0.8rem; color: #888; }
    .commission { font-weight: 700; color: #16a34a; }
    .badge { padding: 0.2rem 0.6rem; border-radius: 12px; font-size: 0.75rem; font-weight: 600; }
    .badge-blue { background: #eff6ff; color: #1d4ed8; }
    .badge-green { background: #f0fdf4; color: #15803d; }
    .badge-purple { background: #faf5ff; color: #7e22ce; }
    .badge-yellow { background: #fefce8; color: #a16207; }
    .badge-gray { background: #f4f5f7; color: #6b7280; }
    .text-center { text-align: center; color: #999; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; border: 1px solid #ddd; }
    .btn-sm { padding: 0.3rem 0.6rem; font-size: 0.8rem; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .summary { display: flex; gap: 2rem; margin-top: 1.5rem; padding: 1rem; background: #f9fafb; border-radius: 8px; }
    .summary-item { display: flex; flex-direction: column; gap: 0.25rem; }
    .summary-item span { font-size: 0.85rem; color: #666; }
    .summary-item strong { font-size: 1.1rem; }
  `]
})
export class CommissionLedgerListComponent implements OnInit {
  private api = inject(ApiService);

  items = signal<CommissionLedger[]>([]);
  filterType = -1;
  filterStatus = -1;

  ngOnInit() { this.loadLedger(); }

  loadLedger() {
    this.api.getAll<CommissionLedger>('commissionledgers').subscribe(data => {
      let items = data.items || [];
      if (this.filterType >= 0) items = items.filter(i => i.entityType === this.filterType);
      if (this.filterStatus >= 0) items = items.filter(i => i.status === this.filterStatus);
      this.items.set(items);
    });
  }

  settle(item: CommissionLedger) {
    if (!confirm(`Settle ৳${item.commissionEarned} commission?`)) return;
    this.api.create('commissionledgers/settle', { ledgerId: item.id, reference: `Settled ${item.periodKey}` })
      .subscribe(() => this.loadLedger());
  }

  totalEarned(): number { return this.items().reduce((sum, i) => sum + i.commissionEarned, 0); }
  totalPending(): number { return this.items().filter(i => i.status === 1).reduce((sum, i) => sum + i.commissionEarned, 0); }

  entityLabel(type: number): string { return ['Salesman', 'Customer', 'Supplier'][type] || 'Unknown'; }
  entityBadge(type: number): string { return ['badge-blue', 'badge-green', 'badge-purple'][type] || ''; }
  statusLabel(status: number): string { return ['Pending', 'Earned', 'Applied', 'Expired', 'Cancelled'][status] || ''; }
  statusBadge(status: number): string { return ['badge-gray', 'badge-yellow', 'badge-green', 'badge-gray', 'badge-gray'][status] || ''; }
}
