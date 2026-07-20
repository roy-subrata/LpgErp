import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { DriverSettlement } from '../../core/models';
import { DriverSettlementFormComponent } from './driver-settlement-form.component';

@Component({
  selector: 'app-driver-settlement-list',
  standalone: true,
  imports: [CommonModule, DriverSettlementFormComponent],
  template: `
    <div class="page-header">
      <h1>Driver Settlements</h1>
      <button class="btn-primary" (click)="onNew()">+ New Settlement</button>
    </div>
    @if (showForm()) {
      <app-driver-settlement-form [entityId]="editingId()" (saved)="onSaved()" (close)="showForm.set(false)" />
    }
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Driver</th>
            <th>Trips</th>
            <th>Fuel Cost</th>
            <th>Trip Income</th>
            <th>Net Settlement</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (s of items(); track s.id) {
            <tr>
              <td>{{ s.settlementDate | date:'short' }}</td>
              <td>{{ s.driverName }}</td>
              <td>{{ s.tripCount }}</td>
              <td>{{ s.fuelCost | number:'1.2-2' }}</td>
              <td>{{ s.tripIncome | number:'1.2-2' }}</td>
              <td>{{ s.netSettlement | number:'1.2-2' }}</td>
              <td>
                <button class="btn-sm" (click)="onEdit(s.id)">Edit</button>
                <button class="btn-sm btn-danger" (click)="onDelete(s.id)">Delete</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="7">No settlements found.</td></tr>
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
    th { background: #f8f9fa; font-weight: 600; }
    .btn-sm { padding: 0.25rem 0.5rem; border: 1px solid #ddd; border-radius: 4px; cursor: pointer; margin-right: 0.25rem; }
    .btn-danger { color: #dc3545; border-color: #dc3545; }
  `],
})
export class DriverSettlementListComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<DriverSettlement[]>([]);
  showForm = signal(false);
  editingId = signal<string | null>(null);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getAll<DriverSettlement>('driversettlements').subscribe(data => this.items.set(data.items));
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
      this.api.delete('driversettlements', id).subscribe(() => this.loadData());
    }
  }

  onSaved() {
    this.showForm.set(false);
    this.editingId.set(null);
    this.loadData();
  }
}
