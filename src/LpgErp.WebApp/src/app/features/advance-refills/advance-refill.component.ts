import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { OutstandingCylinder } from '../../core/models';
import { AdvanceRefillFormComponent } from './advance-refill-form.component';

@Component({
  selector: 'app-advance-refill',
  standalone: true,
  imports: [CommonModule, AdvanceRefillFormComponent],
  template: `
    <div class="page-header">
      <h1>Advance Refill - Outstanding Cylinders</h1>
      <button class="btn-primary" (click)="showForm.set(true)">+ New Advance Refill</button>
    </div>
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Customer</th>
            <th>Brand</th>
            <th>Cylinder Size</th>
            <th>Outstanding Empty Cylinders</th>
          </tr>
        </thead>
        <tbody>
          @for (o of items(); track $index) {
            <tr>
              <td>{{ o.customerName }}</td>
              <td>{{ o.brandName }}</td>
              <td>{{ o.cylinderSizeName }}</td>
              <td class="text-danger">{{ o.outstanding }}</td>
            </tr>
          } @empty {
            <tr><td colspan="4">No outstanding cylinders found.</td></tr>
          }
        </tbody>
      </table>
    </div>
    <app-advance-refill-form [open]="showForm()" (close)="showForm.set(false)" (saved)="showForm.set(false); loadData()" />
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .btn-primary { background: #1a1a2e; color: white; border: none; padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; }
    .table-container { background: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow: hidden; }
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 0.75rem 1rem; text-align: left; border-bottom: 1px solid #eee; }
    th { background: #f8f9fa; font-weight: 600; }
    .text-danger { color: #dc3545; font-weight: 600; }
  `],
})
export class AdvanceRefillComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<OutstandingCylinder[]>([]);
  showForm = signal(false);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getAll<OutstandingCylinder>('advancerefills/outstanding').subscribe(data => this.items.set(data.items));
  }
}
