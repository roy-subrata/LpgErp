import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Driver, DriverSettlement } from '../../core/models';

@Component({
  selector: 'app-driver-settlement-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Driver Settlement' : 'New Driver Settlement'" (close)="onClose()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label for="driverId">Driver</label>
          <select id="driverId" [(ngModel)]="driverId" name="driverId" required>
            <option value="">Select Driver</option>
            @for (driver of drivers(); track driver.id) {
              <option [value]="driver.id">{{ driver.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="tripCount">Trip Count</label>
          <input id="tripCount" type="number" [(ngModel)]="tripCount" name="tripCount" required />
        </div>
        <div class="form-group">
          <label for="fuelCost">Fuel Cost</label>
          <input id="fuelCost" type="number" [(ngModel)]="fuelCost" name="fuelCost" required />
        </div>
        <div class="form-group">
          <label for="allowance">Allowance</label>
          <input id="allowance" type="number" [(ngModel)]="allowance" name="allowance" required />
        </div>
        <div class="form-group">
          <label for="loadingCost">Loading Cost</label>
          <input id="loadingCost" type="number" [(ngModel)]="loadingCost" name="loadingCost" required />
        </div>
        <div class="form-group">
          <label for="unloadingCost">Unloading Cost</label>
          <input id="unloadingCost" type="number" [(ngModel)]="unloadingCost" name="unloadingCost" required />
        </div>
        <div class="form-group">
          <label for="tripIncome">Trip Income</label>
          <input id="tripIncome" type="number" [(ngModel)]="tripIncome" name="tripIncome" required />
        </div>
        <div class="form-group">
          <label for="companyPickupIncentive">Company Pickup Incentive</label>
          <input id="companyPickupIncentive" type="number" [(ngModel)]="companyPickupIncentive" name="companyPickupIncentive" required />
        </div>
        <div class="form-group">
          <label for="distance">Distance</label>
          <input id="distance" type="number" [(ngModel)]="distance" name="distance" />
        </div>
        <div class="form-group">
          <label for="notes">Notes</label>
          <input id="notes" type="text" [(ngModel)]="notes" name="notes" />
        </div>
        <div class="form-actions">
          <button type="button" class="btn btn-secondary" (click)="onClose()">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="saving()">Save</button>
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
export class DriverSettlementFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  driverId = '';
  tripCount = 0;
  fuelCost = 0;
  allowance = 0;
  loadingCost = 0;
  unloadingCost = 0;
  tripIncome = 0;
  companyPickupIncentive = 0;
  distance: number | null = null;
  notes = '';
  drivers = signal<Driver[]>([]);
  saving = signal(false);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      this.api.getAllList<Driver>('drivers').subscribe(data => this.drivers.set(data));
      if (this.entityId) {
        this.api.getById<DriverSettlement>('driversettlements', this.entityId).subscribe(settlement => {
          this.driverId = settlement.driverId;
          this.tripCount = settlement.tripCount;
          this.fuelCost = settlement.fuelCost;
          this.allowance = settlement.allowance;
          this.loadingCost = settlement.loadingCost;
          this.unloadingCost = settlement.unloadingCost;
          this.tripIncome = settlement.tripIncome;
          this.companyPickupIncentive = settlement.companyPickupIncentive;
          this.distance = settlement.distance ?? null;
          this.notes = settlement.notes;
        });
      } else {
        this.resetForm();
      }
    }
  }

  submit() {
    this.saving.set(true);
    const body = {
      driverId: this.driverId,
      tripCount: this.tripCount,
      fuelCost: this.fuelCost,
      allowance: this.allowance,
      loadingCost: this.loadingCost,
      unloadingCost: this.unloadingCost,
      tripIncome: this.tripIncome,
      companyPickupIncentive: this.companyPickupIncentive,
      distance: this.distance,
      notes: this.notes,
    };

    const req$ = this.entityId
      ? this.api.update('driversettlements', this.entityId, body)
      : this.api.create('driversettlements', body);

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

  private resetForm() {
    this.driverId = '';
    this.tripCount = 0;
    this.fuelCost = 0;
    this.allowance = 0;
    this.loadingCost = 0;
    this.unloadingCost = 0;
    this.tripIncome = 0;
    this.companyPickupIncentive = 0;
    this.distance = null;
    this.notes = '';
  }
}
