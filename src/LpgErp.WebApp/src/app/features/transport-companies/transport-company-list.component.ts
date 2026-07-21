import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-transport-company-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class TransportCompanyListComponent {
  readonly config: EntityConfig = {
    endpoint: 'transportcompanies',
    title: 'Transport Companies',
    singular: 'Transport Company',
    cols: [
      { key: 'name', label: 'Name', kind: 'main', sub: 'contactPerson' },
      { key: 'phone', label: 'Phone', kind: 'text' },
      { key: 'email', label: 'Email', kind: 'text' },
      { key: 'address', label: 'Address', kind: 'muted' },
      { key: 'isActive', label: 'Status', kind: 'badge', badgeMap: { true: ['Active', '#f0fdf4', '#15803d'], false: ['Inactive', '#f4f5f7', '#6b7280'] } },
    ],
    fields: [
      { key: 'name', label: 'Name', type: 'text', required: true },
      { key: 'contactPerson', label: 'Contact Person', type: 'text' },
      { key: 'phone', label: 'Phone', type: 'text' },
      { key: 'email', label: 'Email', type: 'text' },
      { key: 'address', label: 'Address', type: 'text' },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ],
  };
  readonly searchFields = ['name', 'contactPerson', 'phone', 'email'];
}
