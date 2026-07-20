import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { CustomerCreditSummary, CreditAgingEntry } from '../../core/models';

@Component({
  selector: 'app-customer-credit',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-header">
      <h1>Customer Credit Management</h1>
    </div>

    <div class="tabs">
      <button [class.active]="tab() === 'summary'" (click)="tab.set('summary')">Credit Summary</button>
      <button [class.active]="tab() === 'aging'" (click)="tab.set('aging'); loadAging()">Credit Aging</button>
    </div>

    @if (tab() === 'summary') {
      <div class="table-container">
        <table>
          <thead>
            <tr>
              <th>Customer</th>
              <th>Credit Limit</th>
              <th>Total Purchases</th>
              <th>Total Payments</th>
              <th>Outstanding</th>
              <th>Utilization</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            @for (s of summaries(); track s.customerId) {
              <tr [class.over-credit]="s.isOverCredit">
                <td>{{ s.customerName }}</td>
                <td>{{ s.creditLimit | number:'1.2-2' }}</td>
                <td>{{ s.totalPurchases | number:'1.2-2' }}</td>
                <td>{{ s.totalPayments | number:'1.2-2' }}</td>
                <td>{{ s.outstandingBalance | number:'1.2-2' }}</td>
                <td>{{ s.creditUtilization }}%</td>
                <td>
                  @if (s.isOverCredit) {
                    <span class="badge-danger">Over Limit</span>
                  } @else if (s.creditUtilization > 80) {
                    <span class="badge-warning">Near Limit</span>
                  } @else {
                    <span class="badge-ok">OK</span>
                  }
                </td>
              </tr>
            } @empty {
              <tr><td colspan="7">No credit data found.</td></tr>
            }
          </tbody>
        </table>
      </div>
    }

    @if (tab() === 'aging' && agingReport()) {
      <div class="summary-grid">
        <div class="summary-card"><label>Current</label><span>{{ agingReport()!.totalCurrent | number:'1.2-2' }}</span></div>
        <div class="summary-card"><label>1-30 Days</label><span>{{ agingReport()!.totalDays30 | number:'1.2-2' }}</span></div>
        <div class="summary-card"><label>31-60 Days</label><span>{{ agingReport()!.totalDays60 | number:'1.2-2' }}</span></div>
        <div class="summary-card"><label>61-90 Days</label><span>{{ agingReport()!.totalDays90 | number:'1.2-2' }}</span></div>
        <div class="summary-card"><label>Over 90 Days</label><span class="text-danger">{{ agingReport()!.totalDaysOver90 | number:'1.2-2' }}</span></div>
      </div>
      <div class="table-container">
        <table>
          <thead>
            <tr>
              <th>Customer</th>
              <th>Current</th>
              <th>1-30 Days</th>
              <th>31-60 Days</th>
              <th>61-90 Days</th>
              <th>Over 90 Days</th>
            </tr>
          </thead>
          <tbody>
            @for (e of agingReport()!.entries; track e.customerName) {
              <tr>
                <td>{{ e.customerName }}</td>
                <td>{{ e.current | number:'1.2-2' }}</td>
                <td>{{ e.days30 | number:'1.2-2' }}</td>
                <td>{{ e.days60 | number:'1.2-2' }}</td>
                <td>{{ e.days90 | number:'1.2-2' }}</td>
                <td [class.text-danger]="e.daysOver90 > 0">{{ e.daysOver90 | number:'1.2-2' }}</td>
              </tr>
            } @empty {
              <tr><td colspan="6">No aging data found.</td></tr>
            }
          </tbody>
        </table>
      </div>
    }
  `,
  styles: [`
    .page-header { margin-bottom: 1.5rem; }
    .tabs { display: flex; gap: 0.5rem; margin-bottom: 1.5rem; }
    .tabs button { padding: 0.5rem 1rem; border: 1px solid #ddd; border-radius: 4px; cursor: pointer; background: white; }
    .tabs button.active { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .summary-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
    .summary-card { background: white; border-radius: 8px; padding: 1rem; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
    .summary-card label { display: block; font-size: 0.85rem; color: #666; margin-bottom: 0.25rem; }
    .summary-card span { font-size: 1.1rem; font-weight: 600; }
    .table-container { background: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow: hidden; }
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 0.75rem 1rem; text-align: left; border-bottom: 1px solid #eee; }
    th { background: #f8f9fa; font-weight: 600; }
    .over-credit { background: #ffebee; }
    .text-danger { color: #dc3545; font-weight: 600; }
    .badge-danger { background: #dc3545; color: white; padding: 0.2rem 0.5rem; border-radius: 4px; font-size: 0.8rem; }
    .badge-warning { background: #ffc107; color: #333; padding: 0.2rem 0.5rem; border-radius: 4px; font-size: 0.8rem; }
    .badge-ok { background: #28a745; color: white; padding: 0.2rem 0.5rem; border-radius: 4px; font-size: 0.8rem; }
  `],
})
export class CustomerCreditComponent implements OnInit {
  private api = inject(ApiService);
  summaries = signal<CustomerCreditSummary[]>([]);
  agingReport = signal<any>(null);
  tab = signal<'summary' | 'aging'>('summary');

  ngOnInit() {
    this.loadSummaries();
  }

  loadSummaries() {
    this.api.getAll<CustomerCreditSummary>('customercredit').subscribe(data => this.summaries.set(data.items));
  }

  loadAging() {
    if (!this.agingReport()) {
      this.api.get<any>('customercredit/aging').subscribe(data => this.agingReport.set(data));
    }
  }
}
