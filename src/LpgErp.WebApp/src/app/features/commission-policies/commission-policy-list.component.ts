import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { CommissionPolicy } from '../../core/commission.models';
import { DialogComponent } from '../../shared/dialog.component';

interface EntityOption { id: string; name: string; }

@Component({
  selector: 'app-commission-policy-list',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <div class="page-header">
      <h2>Commission Policies</h2>
      <button class="btn btn-primary" (click)="openCreate()">+ New Policy</button>
    </div>

    <div class="filters">
      <select [(ngModel)]="filterType" (change)="loadPolicies()">
        <option [ngValue]="-1">All Types</option>
        <option [ngValue]="0">Salesman</option>
        <option [ngValue]="1">Customer</option>
        <option [ngValue]="2">Supplier</option>
        <option [ngValue]="3">Driver</option>
      </select>
    </div>

    <table class="table">
      <thead>
        <tr>
          <th>Name</th>
          <th>Type</th>
          <th>Entity</th>
          <th>Target</th>
          <th>Commission</th>
          <th>Period</th>
          <th>Status</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        @for (item of items(); track item.id) {
          <tr>
            <td>
              <div class="policy-name">{{ item.name }}</div>
              <div class="policy-desc">{{ item.description || '-' }}</div>
            </td>
            <td><span class="badge" [class]="entityBadge(item.entityType)">{{ entityLabel(item.entityType) }}</span></td>
            <td>{{ item.entityName || getEntityNameLocal(item.entityType, item.entityId) || item.entityId }}</td>
            <td>{{ item.targetQuantity }} units</td>
            <td>
              @if (item.calculationType === 0) { {{ item.commissionValue }}% }
              @else if (item.calculationType === 1) { ৳{{ item.commissionValue }} }
              @else if (item.calculationType === 2) { ৳{{ item.commissionValue }}/unit }
              @else if (item.calculationType === 3) { ৳{{ item.commissionValue }} bonus }
              @else { Tiered }
            </td>
            <td>{{ periodLabel(item.periodType) }}</td>
            <td><span class="badge" [class]="item.isActive ? 'badge-green' : 'badge-gray'">{{ item.isActive ? 'Active' : 'Inactive' }}</span></td>
            <td>
              <button class="btn btn-sm" (click)="editPolicy(item)">Edit</button>
              <button class="btn btn-sm btn-danger" (click)="deletePolicy(item.id)">Delete</button>
            </td>
          </tr>
        } @empty {
          <tr><td colspan="8" class="text-center">No commission policies found</td></tr>
        }
      </tbody>
    </table>

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
          <button type="button" class="btn btn-secondary" (click)="dialogOpen.set(false)">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="saving()">Save</button>
        </div>
      </form>
    </app-dialog>
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .page-header h2 { margin: 0; }
    .filters { margin-bottom: 1rem; }
    .filters select { padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; }
    .table { width: 100%; border-collapse: collapse; }
    .table th, .table td { padding: 0.75rem; text-align: left; border-bottom: 1px solid #eee; }
    .table th { font-weight: 600; font-size: 0.85rem; color: #666; }
    .policy-name { font-weight: 600; }
    .policy-desc { font-size: 0.8rem; color: #888; }
    .badge { padding: 0.2rem 0.6rem; border-radius: 12px; font-size: 0.75rem; font-weight: 600; }
    .badge-blue { background: #eff6ff; color: #1d4ed8; }
    .badge-green { background: #f0fdf4; color: #15803d; }
    .badge-purple { background: #faf5ff; color: #7e22ce; }
    .badge-gray { background: #f4f5f7; color: #6b7280; }
    .badge-orange { background: #fff7ed; color: #c2410c; }
    .text-center { text-align: center; color: #999; }
    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; margin-bottom: 0.25rem; font-weight: 600; font-size: 0.9rem; }
    .form-group input, .form-group select { width: 100%; padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.5rem; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; border: 1px solid #ddd; }
    .btn-sm { padding: 0.3rem 0.6rem; font-size: 0.8rem; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .btn-secondary { background: white; color: #333; }
    .btn-danger { background: #fee2e2; color: #dc2626; border-color: #fca5a5; }
  `]
})
export class CommissionPolicyListComponent implements OnInit {
  private api = inject(ApiService);

  items = signal<CommissionPolicy[]>([]);
  dialogOpen = signal(false);
  saving = signal(false);
  editingId = '';
  filterType = -1;

  entityOptions = signal<EntityOption[]>([]);
  private allSalesmen: EntityOption[] = [];
  private allCustomers: EntityOption[] = [];
  private allSuppliers: EntityOption[] = [];
  private allDrivers: EntityOption[] = [];

  form = this.getEmptyForm();

  ngOnInit() {
    this.loadPolicies();
    this.loadEntityLists();
  }

  loadPolicies() {
    this.api.getAll<CommissionPolicy>('commissionpolicies').subscribe(data => {
      let items = data.items || [];
      if (this.filterType >= 0) items = items.filter(i => i.entityType === this.filterType);
      this.items.set(items);
    });
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
    const body = { ...this.form, entityId: this.form.entityId, startDate: new Date(this.form.startDate).toISOString(), endDate: this.form.endDate ? new Date(this.form.endDate).toISOString() : null };
    const req$ = this.editingId
      ? this.api.update('commissionpolicies', this.editingId, body)
      : this.api.create('commissionpolicies', body);
    req$.subscribe({
      next: () => { this.saving.set(false); this.dialogOpen.set(false); this.loadPolicies(); },
      error: () => this.saving.set(false)
    });
  }

  deletePolicy(id: string) {
    if (!confirm('Delete this policy?')) return;
    this.api.delete('commissionpolicies', id).subscribe(() => this.loadPolicies());
  }

  entityLabel(type: number): string { return ['Salesman', 'Customer', 'Supplier', 'Driver'][type] || 'Unknown'; }
  entityBadge(type: number): string { return ['badge-blue', 'badge-green', 'badge-purple', 'badge-orange'][type] || ''; }
  periodLabel(type: number): string { return ['One Time', 'Weekly', 'Monthly', 'Yearly'][type] || ''; }

  getEntityNameLocal(type: number, id: string): string {
    const list = this.getEntitiesForType(type);
    return list.find(e => e.id === id)?.name || '';
  }

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
