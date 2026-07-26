import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EntityListComponent, EntityConfig, RowAction } from '../../shared/entity-list.component';
import { ApiService } from '../../core/api.service';
import { CommissionPolicy } from '../../core/commission.models';
import { DialogComponent } from '../../shared/dialog.component';

interface EntityOption { id: string; name: string; }

@Component({
  selector: 'app-commission-policy-list',
  standalone: true,
  imports: [CommonModule, FormsModule, EntityListComponent, DialogComponent],
  template: `
    <app-entity-list
      [config]="config"
      [tabs]="tabs"
      [tabField]="'entityType'"
      [searchFields]="searchFields"
      [useCustomForm]="true"
      [reloadSignal]="reloadTick()"
      (newRequested)="openCreate()"
      (editRequested)="editPolicy($event)"
    />

    <app-dialog [open]="dialogOpen()" [title]="editingId ? 'Edit Policy' : 'New Policy'" (close)="dialogOpen.set(false)">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label>Name</label>
          <input type="text" [(ngModel)]="form.name" name="name" required />
        </div>
        <div class="form-group">
          <label>Description</label>
          <input type="text" [(ngModel)]="form.description" name="description" />
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Apply To</label>
            <select [(ngModel)]="form.entityType" name="entityType" (ngModelChange)="onEntityTypeChange()">
              <option [ngValue]="0">Salesman</option>
              <option [ngValue]="1">Customer</option>
              <option [ngValue]="2">Supplier</option>
              <option [ngValue]="3">Driver</option>
            </select>
          </div>
          <div class="form-group">
            <label>{{ entityLabel(form.entityType) }}</label>
            <select [(ngModel)]="form.entityId" name="entityId" required>
              <option value="" disabled>Select {{ entityLabel(form.entityType) }}...</option>
              @for (e of entityOptions(); track e.id) {
                <option [value]="e.id">{{ e.name }}</option>
              }
            </select>
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Calculation</label>
            <select [(ngModel)]="form.calculationType" name="calculationType">
              <option [ngValue]="0">Percentage of Sales</option>
              <option [ngValue]="1">Fixed Amount (on target)</option>
              <option [ngValue]="2">Per Unit</option>
              <option [ngValue]="3">Target Bonus</option>
              <option [ngValue]="4">Tiered Percentage</option>
            </select>
          </div>
          <div class="form-group">
            <label>Period</label>
            <select [(ngModel)]="form.periodType" name="periodType">
              <option [ngValue]="0">One Time</option>
              <option [ngValue]="1">Weekly</option>
              <option [ngValue]="2">Monthly</option>
              <option [ngValue]="3">Yearly</option>
            </select>
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Target Quantity</label>
            <input type="number" [(ngModel)]="form.targetQuantity" name="targetQuantity" required />
          </div>
          <div class="form-group">
            <label>Commission Value</label>
            <input type="number" [(ngModel)]="form.commissionValue" name="commissionValue" step="0.01" required />
          </div>
        </div>
        <div class="form-group">
          <label>Start Date</label>
          <input type="date" [(ngModel)]="form.startDate" name="startDate" required />
        </div>
        <div class="form-group">
          <label>End Date (optional)</label>
          <input type="date" [(ngModel)]="form.endDate" name="endDate" />
        </div>
        <div class="form-group">
          <label><input type="checkbox" [(ngModel)]="form.autoApply" name="autoApply" /> Auto-calculate at period end</label>
        </div>
        <div class="form-actions">
          <button type="button" class="btn-cancel" (click)="dialogOpen.set(false)">Cancel</button>
          <button type="submit" class="btn-save" [disabled]="saving()">Save</button>
        </div>
      </form>
    </app-dialog>
  `,
  styles: [`
    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; margin-bottom: 0.25rem; font-weight: 600; font-size: 0.9rem; color: var(--text-primary); }
    .form-group input, .form-group select {
      width: 100%; padding: 8px 12px; border: 1px solid var(--border-input); border-radius: 7px;
      font-size: 13px; box-sizing: border-box; background: var(--surface); color: var(--text-primary);
    }
    .form-group input:focus, .form-group select:focus { border-color: var(--primary); outline: none; box-shadow: 0 0 0 3px rgba(234,88,12,0.12); }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .form-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 1.5rem; }
    .btn-cancel {
      padding: 9px 18px; border-radius: 7px; border: 1px solid var(--border-input);
      background: var(--surface); color: var(--text-secondary); font-size: 13px; font-weight: 600; cursor: pointer;
    }
    .btn-cancel:hover { background: var(--fill-subtle); }
    .btn-save {
      padding: 9px 18px; border-radius: 7px; border: none; background: var(--primary);
      color: #fff; font-size: 13px; font-weight: 600; cursor: pointer; box-shadow: var(--shadow-btn);
    }
    .btn-save:hover { background: var(--primary-hover); }
    .btn-save:disabled { opacity: 0.5; cursor: not-allowed; }
  `]
})
export class CommissionPolicyListComponent {
  private api = inject(ApiService);

  dialogOpen = signal(false);
  saving = signal(false);
  editingId = '';
  reloadTick = signal(0);

  entityOptions = signal<EntityOption[]>([]);
  private allSalesmen: EntityOption[] = [];
  private allCustomers: EntityOption[] = [];
  private allSuppliers: EntityOption[] = [];
  private allDrivers: EntityOption[] = [];

  form = this.getEmptyForm();

  readonly tabs = [
    { label: 'Salesman', value: '0' },
    { label: 'Customer', value: '1' },
    { label: 'Supplier', value: '2' },
    { label: 'Driver', value: '3' },
  ];

  readonly config: EntityConfig = {
    endpoint: 'commissionpolicies',
    title: 'Commission Policies',
    singular: 'Commission Policy',
    cols: [
      { key: 'name', label: 'Name', kind: 'main', sub: 'description' },
      { key: 'entityType', label: 'Type', kind: 'badge', badgeMap: { '0': ['Salesman', '#eff6ff', '#1d4ed8'], '1': ['Customer', '#f0fdf4', '#15803d'], '2': ['Supplier', '#faf5ff', '#7e22ce'], '3': ['Driver', '#fff7ed', '#c2410c'] } },
      { key: 'targetQuantity', label: 'Target', kind: 'num' },
      { key: 'commissionValue', label: 'Commission', kind: 'money' },
      { key: 'periodType', label: 'Period', kind: 'badge', badgeMap: { '0': ['One Time', '#f4f5f7', '#6b7280'], '1': ['Weekly', '#eff6ff', '#1d4ed8'], '2': ['Monthly', '#f0fdf4', '#15803d'], '3': ['Yearly', '#faf5ff', '#7e22ce'] } },
      { key: 'isActive', label: 'Status', kind: 'badge', badgeMap: { true: ['Active', '#f0fdf4', '#15803d'], false: ['Inactive', '#f4f5f7', '#6b7280'] } },
    ],
    fields: [
      { key: 'name', label: 'Name', type: 'text', required: true },
      { key: 'description', label: 'Description', type: 'text' },
    ],
  };
  readonly searchFields = ['name', 'description'];

  ngOnInit() {
    this.loadEntityLists();
  }

  private loadEntityLists() {
    this.api.getAll<{ id: string; name: string }>('salesmen', 1, 200).subscribe(d => this.allSalesmen = (d.items || []).map(e => ({ id: e.id, name: e.name })));
    this.api.getAll<{ id: string; name: string }>('customers', 1, 200).subscribe(d => this.allCustomers = (d.items || []).map(e => ({ id: e.id, name: e.name })));
    this.api.getAll<{ id: string; name: string }>('suppliers', 1, 200).subscribe(d => this.allSuppliers = (d.items || []).map(e => ({ id: e.id, name: e.name })));
    this.api.getAll<{ id: string; name: string }>('drivers', 1, 200).subscribe(d => this.allDrivers = (d.items || []).map(e => ({ id: e.id, name: e.name })));
  }

  onEntityTypeChange() {
    this.form.entityId = '';
    this.entityOptions.set(this.getEntitiesForType(this.form.entityType));
  }

  openCreate() {
    this.editingId = '';
    this.form = this.getEmptyForm();
    this.entityOptions.set(this.getEntitiesForType(0));
    this.dialogOpen.set(true);
  }

  editPolicy(item: CommissionPolicy) {
    this.editingId = item.id;
    this.form = {
      name: item.name,
      description: item.description || '',
      entityType: item.entityType,
      entityId: item.entityId,
      calculationType: item.calculationType,
      periodType: item.periodType,
      productId: item.productId || '',
      brandId: item.brandId || '',
      cylinderSizeId: item.cylinderSizeId || '',
      targetQuantity: item.targetQuantity,
      commissionValue: item.commissionValue,
      tierConfig: item.tierConfig || '',
      autoApply: item.autoApply,
      isActive: item.isActive,
      startDate: item.startDate?.split('T')[0] || '',
      endDate: item.endDate?.split('T')[0] || ''
    };
    this.entityOptions.set(this.getEntitiesForType(item.entityType));
    this.dialogOpen.set(true);
  }

  submit() {
    this.saving.set(true);
    const body = {
      ...this.form,
      entityId: this.form.entityId,
      productId: this.form.productId || null,
      brandId: this.form.brandId || null,
      cylinderSizeId: this.form.cylinderSizeId || null,
      startDate: new Date(this.form.startDate).toISOString(),
      endDate: this.form.endDate ? new Date(this.form.endDate).toISOString() : null
    };
    const req$ = this.editingId
      ? this.api.update('commissionpolicies', this.editingId, body)
      : this.api.create('commissionpolicies', body);
    req$.subscribe({
      next: () => { this.saving.set(false); this.dialogOpen.set(false); this.reloadTick.update(v => v + 1); },
      error: (err) => { this.saving.set(false); console.error('Commission policy save error:', err); }
    });
  }

  entityLabel(type: number): string { return ['Salesman', 'Customer', 'Supplier', 'Driver'][type] || 'Unknown'; }

  private getEntitiesForType(type: number): EntityOption[] {
    return [this.allSalesmen, this.allCustomers, this.allSuppliers, this.allDrivers][type] || [];
  }

  private getEmptyForm() {
    return {
      name: '', description: '', entityType: 0, entityId: '', calculationType: 2, periodType: 2,
      productId: '', brandId: '', cylinderSizeId: '', targetQuantity: 0, commissionValue: 0,
      tierConfig: '', autoApply: true, isActive: true,
      startDate: new Date().toISOString().split('T')[0], endDate: ''
    };
  }
}
