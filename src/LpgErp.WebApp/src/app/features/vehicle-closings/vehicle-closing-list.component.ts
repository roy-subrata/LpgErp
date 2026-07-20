import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { VehicleClosing } from '../../core/models';

@Component({
  selector: 'app-vehicle-closing-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-header">
      <h1>Vehicle Closings</h1>
    </div>
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Loading ID</th>
            <th>Cash Collected</th>
            <th>Credit Sales</th>
            <th>Outstanding</th>
            <th>Exchanges</th>
            <th>Returned Empties</th>
            <th>Damaged</th>
            <th>Variance</th>
            <th>Notes</th>
          </tr>
        </thead>
        <tbody>
          @for (item of items(); track item.id) {
            <tr>
              <td>{{ item.closingDate | date:'short' }}</td>
              <td>{{ item.vehicleLoadingId }}</td>
              <td>{{ item.cashCollected | number:'1.2-2' }}</td>
              <td>{{ item.creditSales | number:'1.2-2' }}</td>
              <td>{{ item.outstandingAmount | number:'1.2-2' }}</td>
              <td>{{ item.cylinderExchanges }}</td>
              <td>{{ item.returnedEmptyCylinders }}</td>
              <td>{{ item.damagedCount }}</td>
              <td>{{ item.variance | number:'1.2-2' }}</td>
              <td>{{ item.notes }}</td>
            </tr>
          } @empty {
            <tr>
              <td colspan="10">No vehicle closings found.</td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `,
  styles: [`
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
    }
    .table-container {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      overflow: hidden;
    }
    table {
      width: 100%;
      border-collapse: collapse;
    }
    th, td {
      padding: 0.75rem 1rem;
      text-align: left;
      border-bottom: 1px solid #eee;
    }
    th {
      background: #f8f9fa;
      font-weight: 600;
    }
  `],
})
export class VehicleClosingListComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<VehicleClosing[]>([]);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getAll<VehicleClosing>('vehicleclosings').subscribe(data => this.items.set(data.items));
  }
}
