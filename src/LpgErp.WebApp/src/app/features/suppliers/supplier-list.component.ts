import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { EntityListComponent, EntityConfig, RowAction } from '../../shared/entity-list.component';

@Component({
  selector: 'app-supplier-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" [rowActions]="rowActions" (rowAction)="onRowAction($event)" />`,
})
export class SupplierListComponent {
  private router = inject(Router);

  readonly rowActions: RowAction[] = [
    { key: 'account', icon: '📒', title: 'View account', show: () => true },
  ];

  onRowAction(e: { key: string; item: any }) {
    if (e.key === 'account') this.router.navigate(['/suppliers', e.item.id]);
  }

  readonly config: EntityConfig = {
    endpoint: 'suppliers',
    title: 'Suppliers',
    singular: 'Supplier',
    cols: [
      { key: 'name', label: 'Name', kind: 'main', sub: 'contactPerson' },
      { key: 'code', label: 'Code', kind: 'mono' },
      { key: 'phone', label: 'Phone', kind: 'text' },
      { key: 'email', label: 'Email', kind: 'text' },
      { key: 'isLpgCompany', label: 'LPG Company', kind: 'badge', badgeMap: { true: ['Yes', '#f0fdf4', '#15803d'], false: ['No', '#f4f5f7', '#6b7280'] } },
      { key: 'commissionBalance', label: 'Commission Bal.', kind: 'money' },
      { key: 'isActive', label: 'Status', kind: 'badge', badgeMap: { true: ['Active', '#f0fdf4', '#15803d'], false: ['Inactive', '#f4f5f7', '#6b7280'] } },
    ],
    fields: [
      { key: 'name', label: 'Name', type: 'text', required: true },
      { key: 'code', label: 'Code', type: 'text', required: true, mono: true },
      { key: 'contactPerson', label: 'Contact Person', type: 'text' },
      { key: 'phone', label: 'Phone', type: 'text' },
      { key: 'email', label: 'Email', type: 'text' },
      { key: 'address', label: 'Address', type: 'text' },
      { key: 'isLpgCompany', label: 'LPG Company', type: 'toggle' },
      { key: 'commissionPerCylinder', label: 'Commission / Cylinder', type: 'number' },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ],
  };
  readonly searchFields = ['name', 'code', 'contactPerson', 'phone', 'email'];
}
