import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { DailySalesSummary } from '../../core/models';
import { DailySalesSummaryFormComponent } from './daily-sales-summary-form.component';

@Component({
  selector: 'app-daily-sales-summary',
  standalone: true,
  imports: [CommonModule, DailySalesSummaryFormComponent],
  template: `
    <div class="page-header">
      <h1>Daily Sales Summaries</h1>
      <button class="btn-primary" (click)="onNew()">+ New Summary</button>
    </div>
    @if (showForm()) {
      <app-daily-sales-summary-form [open]="showForm()" [entityId]="editingId()" (close)="showForm.set(false)" (saved)="onSaved()" />
    }
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Truck</th>
            <th>Driver</th>
            <th>Salesman</th>
            <th>Total Sales</th>
            <th>Cash</th>
            <th>Credit</th>
            <th>Packages</th>
            <th>Refills</th>
            <th>Outstanding</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (s of items(); track s.id) {
            <tr>
              <td>{{ s.summaryDate | date:'short' }}</td>
              <td>{{ s.truckName }}</td>
              <td>{{ s.driverName }}</td>
              <td>{{ s.salesmanName }}</td>
              <td>{{ s.totalSales | number:'1.2-2' }}</td>
              <td>{{ s.cashSales | number:'1.2-2' }}</td>
              <td>{{ s.creditSales | number:'1.2-2' }}</td>
              <td>{{ s.packagesSold }}</td>
              <td>{{ s.refillsSold }}</td>
              <td>{{ s.outstandingCylinders }}</td>
              <td>
                <button class="btn-sm" (click)="onEdit(s.id)">Edit</button>
                <button class="btn-sm btn-danger" (click)="onDelete(s.id)">Delete</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="11">No daily summaries found.</td></tr>
          }
        </tbody>
      </table>
    </div>
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .btn-primary { background: #1a1a2e; color: white; border: none; padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; }
    .table-container { background: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow: hidden; }
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 0.75rem 1rem; text-align: left; border-bottom: 1px solid #eee; }
    th { background: #f8f9fa; font-weight: 600; font-size: 0.85rem; }
    .btn-sm { padding: 0.25rem 0.5rem; border: 1px solid #ddd; border-radius: 4px; cursor: pointer; margin-right: 0.25rem; }
    .btn-danger { color: #dc3545; border-color: #dc3545; }
  `],
})
export class DailySalesSummaryComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<DailySalesSummary[]>([]);
  showForm = signal(false);
  editingId = signal<string | null>(null);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getAll<DailySalesSummary>('dailysalessummaries').subscribe(data => this.items.set(data.items));
  }

  onNew() {
    this.editingId.set(null);
    this.showForm.set(true);
  }

  onEdit(id: string) {
    this.editingId.set(id);
    this.showForm.set(true);
  }

  onDelete(id: string) {
    if (confirm('Are you sure?')) {
      this.api.delete('dailysalessummaries', id).subscribe(() => this.loadData());
    }
  }

  onSaved() {
    this.showForm.set(false);
    this.editingId.set(null);
    this.loadData();
  }
}
