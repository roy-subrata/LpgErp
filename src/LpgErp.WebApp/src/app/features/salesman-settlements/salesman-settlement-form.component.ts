import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Salesman, SalesmanSettlement } from '../../core/models';

@Component({
  selector: 'app-salesman-settlement-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Salesman Settlement' : 'New Salesman Settlement'" (close)="onClose()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label for="salesmanId">Salesman</label>
          <select id="salesmanId" [(ngModel)]="salesmanId" name="salesmanId" required>
            <option value="">Select Salesman</option>
            @for (salesman of salesmen(); track salesman.id) {
              <option [value]="salesman.id">{{ salesman.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="totalSales">Total Sales</label>
          <input id="totalSales" type="number" [(ngModel)]="totalSales" name="totalSales" required />
        </div>
        <div class="form-group">
          <label for="collection">Collection</label>
          <input id="collection" type="number" [(ngModel)]="collection" name="collection" required />
        </div>
        <div class="form-group">
          <label for="commission">Commission</label>
          <input id="commission" type="number" [(ngModel)]="commission" name="commission" required />
        </div>
        <div class="form-group">
          <label for="dailyAllowance">Daily Allowance</label>
          <input id="dailyAllowance" type="number" [(ngModel)]="dailyAllowance" name="dailyAllowance" required />
        </div>
        <div class="form-group">
          <label for="bonus">Bonus</label>
          <input id="bonus" type="number" [(ngModel)]="bonus" name="bonus" required />
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
export class SalesmanSettlementFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  salesmanId = '';
  totalSales = 0;
  collection = 0;
  commission = 0;
  dailyAllowance = 0;
  bonus = 0;
  notes = '';
  salesmen = signal<Salesman[]>([]);
  saving = signal(false);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      this.api.getAllList<Salesman>('salesmen').subscribe(data => this.salesmen.set(data));
      if (this.entityId) {
        this.api.getById<SalesmanSettlement>('salesmansettlements', this.entityId).subscribe(settlement => {
          this.salesmanId = settlement.salesmanId;
          this.totalSales = settlement.totalSales;
          this.collection = settlement.collection;
          this.commission = settlement.commission;
          this.dailyAllowance = settlement.dailyAllowance;
          this.bonus = settlement.bonus;
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
      salesmanId: this.salesmanId,
      totalSales: this.totalSales,
      collection: this.collection,
      commission: this.commission,
      dailyAllowance: this.dailyAllowance,
      bonus: this.bonus,
      notes: this.notes,
    };

    const req$ = this.entityId
      ? this.api.update('salesmansettlements', this.entityId, body)
      : this.api.create('salesmansettlements', body);

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
    this.salesmanId = '';
    this.totalSales = 0;
    this.collection = 0;
    this.commission = 0;
    this.dailyAllowance = 0;
    this.bonus = 0;
    this.notes = '';
  }
}
