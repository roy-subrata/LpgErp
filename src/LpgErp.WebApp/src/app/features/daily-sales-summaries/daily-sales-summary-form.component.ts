import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { DailySalesSummary, Truck, Driver, Salesman, VehicleLoading } from '../../core/models';

@Component({
  selector: 'app-daily-sales-summary-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Daily Sales Summary' : 'New Daily Sales Summary'" (close)="onClose()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label for="vehicleLoadingId">Vehicle Loading</label>
          <select id="vehicleLoadingId" [(ngModel)]="vehicleLoadingId" name="vehicleLoadingId" required>
            <option value="">-- Select --</option>
            @for (vl of vehicleLoadings(); track vl.id) {
              <option [value]="vl.id">{{ vl.truckName }} - {{ vl.driverName }} ({{ vl.loadingDate | date:'shortDate' }})</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="truckId">Truck</label>
          <select id="truckId" [(ngModel)]="truckId" name="truckId" required>
            <option value="">-- Select --</option>
            @for (t of trucks(); track t.id) {
              <option [value]="t.id">{{ t.name }} ({{ t.plateNumber }})</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="driverId">Driver</label>
          <select id="driverId" [(ngModel)]="driverId" name="driverId" required>
            <option value="">-- Select --</option>
            @for (d of drivers(); track d.id) {
              <option [value]="d.id">{{ d.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="salesmanId">Salesman</label>
          <select id="salesmanId" [(ngModel)]="salesmanId" name="salesmanId" required>
            <option value="">-- Select --</option>
            @for (s of salesmen(); track s.id) {
              <option [value]="s.id">{{ s.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="totalSales">Total Sales</label>
          <input id="totalSales" type="number" [(ngModel)]="totalSales" name="totalSales" required />
        </div>
        <div class="form-group">
          <label for="cashSales">Cash Sales</label>
          <input id="cashSales" type="number" [(ngModel)]="cashSales" name="cashSales" required />
        </div>
        <div class="form-group">
          <label for="creditSales">Credit Sales</label>
          <input id="creditSales" type="number" [(ngModel)]="creditSales" name="creditSales" required />
        </div>
        <div class="form-group">
          <label for="packagesSold">Packages Sold</label>
          <input id="packagesSold" type="number" [(ngModel)]="packagesSold" name="packagesSold" required />
        </div>
        <div class="form-group">
          <label for="refillsSold">Refills Sold</label>
          <input id="refillsSold" type="number" [(ngModel)]="refillsSold" name="refillsSold" required />
        </div>
        <div class="form-group">
          <label for="emptyCylindersSold">Empty Cylinders Sold</label>
          <input id="emptyCylindersSold" type="number" [(ngModel)]="emptyCylindersSold" name="emptyCylindersSold" required />
        </div>
        <div class="form-group">
          <label for="accessoriesSold">Accessories Sold</label>
          <input id="accessoriesSold" type="number" [(ngModel)]="accessoriesSold" name="accessoriesSold" required />
        </div>
        <div class="form-group">
          <label for="paymentsCollected">Payments Collected</label>
          <input id="paymentsCollected" type="number" [(ngModel)]="paymentsCollected" name="paymentsCollected" required />
        </div>
        <div class="form-group">
          <label for="dueCreated">Due Created</label>
          <input id="dueCreated" type="number" [(ngModel)]="dueCreated" name="dueCreated" required />
        </div>
        <div class="form-group">
          <label for="cylinderBalance">Cylinder Balance</label>
          <input id="cylinderBalance" type="number" [(ngModel)]="cylinderBalance" name="cylinderBalance" required />
        </div>
        <div class="form-group">
          <label for="outstandingCylinders">Outstanding Cylinders</label>
          <input id="outstandingCylinders" type="number" [(ngModel)]="outstandingCylinders" name="outstandingCylinders" required />
        </div>
        <div class="form-group">
          <label for="stockReturned">Stock Returned</label>
          <input id="stockReturned" type="number" [(ngModel)]="stockReturned" name="stockReturned" required />
        </div>
        <div class="form-group">
          <label for="notes">Notes</label>
          <input id="notes" type="text" [(ngModel)]="notes" name="notes" />
        </div>
        <div class="form-actions">
          <button type="button" class="btn btn-secondary" (click)="onClose()">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="saving()">{{ saving() ? 'Saving...' : (entityId ? 'Update' : 'Create') }}</button>
        </div>
      </form>
    </app-dialog>
  `,
  styles: [`
    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; margin-bottom: 0.25rem; font-weight: 600; font-size: 0.9rem; }
    .form-group input, .form-group select { width: 100%; padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.5rem; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; border: 1px solid #ddd; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .btn-secondary { background: white; color: #333; }
  `],
})
export class DailySalesSummaryFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  vehicleLoadingId = '';
  truckId = '';
  driverId = '';
  salesmanId = '';
  totalSales = 0;
  cashSales = 0;
  creditSales = 0;
  packagesSold = 0;
  refillsSold = 0;
  emptyCylindersSold = 0;
  accessoriesSold = 0;
  paymentsCollected = 0;
  dueCreated = 0;
  cylinderBalance = 0;
  outstandingCylinders = 0;
  stockReturned = 0;
  notes = '';
  saving = signal(false);

  vehicleLoadings = signal<VehicleLoading[]>([]);
  trucks = signal<Truck[]>([]);
  drivers = signal<Driver[]>([]);
  salesmen = signal<Salesman[]>([]);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      this.loadDropdownData();
      if (this.entityId) {
        this.api.getById<DailySalesSummary>('dailysalessummaries', this.entityId).subscribe(summary => {
          this.vehicleLoadingId = summary.vehicleLoadingId;
          this.truckId = summary.truckId;
          this.driverId = summary.driverId;
          this.salesmanId = summary.salesmanId;
          this.totalSales = summary.totalSales;
          this.cashSales = summary.cashSales;
          this.creditSales = summary.creditSales;
          this.packagesSold = summary.packagesSold;
          this.refillsSold = summary.refillsSold;
          this.emptyCylindersSold = summary.emptyCylindersSold;
          this.accessoriesSold = summary.accessoriesSold;
          this.paymentsCollected = summary.paymentsCollected;
          this.dueCreated = summary.dueCreated;
          this.cylinderBalance = summary.cylinderBalance;
          this.outstandingCylinders = summary.outstandingCylinders;
          this.stockReturned = summary.stockReturned;
          this.notes = summary.notes;
        });
      } else {
        this.resetForm();
      }
    }
  }

  submit() {
    this.saving.set(true);
    const body = {
      vehicleLoadingId: this.vehicleLoadingId,
      truckId: this.truckId,
      driverId: this.driverId,
      salesmanId: this.salesmanId,
      totalSales: this.totalSales,
      cashSales: this.cashSales,
      creditSales: this.creditSales,
      packagesSold: this.packagesSold,
      refillsSold: this.refillsSold,
      emptyCylindersSold: this.emptyCylindersSold,
      accessoriesSold: this.accessoriesSold,
      paymentsCollected: this.paymentsCollected,
      dueCreated: this.dueCreated,
      cylinderBalance: this.cylinderBalance,
      outstandingCylinders: this.outstandingCylinders,
      stockReturned: this.stockReturned,
      notes: this.notes,
    };

    const req$ = this.entityId
      ? this.api.update('dailysalessummaries', this.entityId, body)
      : this.api.create('dailysalessummaries', body);

    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.saved.emit();
        this.resetForm();
      },
      error: () => this.saving.set(false),
    });
  }

  onClose() {
    this.resetForm();
    this.close.emit();
  }

  private loadDropdownData() {
    this.api.getAllList<Truck>('trucks').subscribe(data => this.trucks.set(data));
    this.api.getAllList<Driver>('drivers').subscribe(data => this.drivers.set(data));
    this.api.getAllList<Salesman>('salesmen').subscribe(data => this.salesmen.set(data));
    this.api.getAll<VehicleLoading>('vehicleloadings', 1, 1000).subscribe(data => this.vehicleLoadings.set(data.items));
  }

  private resetForm() {
    this.vehicleLoadingId = '';
    this.truckId = '';
    this.driverId = '';
    this.salesmanId = '';
    this.totalSales = 0;
    this.cashSales = 0;
    this.creditSales = 0;
    this.packagesSold = 0;
    this.refillsSold = 0;
    this.emptyCylindersSold = 0;
    this.accessoriesSold = 0;
    this.paymentsCollected = 0;
    this.dueCreated = 0;
    this.cylinderBalance = 0;
    this.outstandingCylinders = 0;
    this.stockReturned = 0;
    this.notes = '';
  }
}
