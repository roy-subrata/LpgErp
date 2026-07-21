import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-brand-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class BrandListComponent {
  readonly config: EntityConfig = {
    endpoint: 'brands',
    title: 'Brands',
    singular: 'Brand',
    cols: [
      { key: 'name', label: 'Name', kind: 'main' },
      { key: 'code', label: 'Code', kind: 'mono' },
      { key: 'isActive', label: 'Status', kind: 'badge', badgeMap: { true: ['Active', '#f0fdf4', '#15803d'], false: ['Inactive', '#f4f5f7', '#6b7280'] } },
    ],
    fields: [
      { key: 'name', label: 'Name', type: 'text', required: true },
      { key: 'code', label: 'Code', type: 'text', required: true, mono: true },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ],
  };
  readonly searchFields = ['name', 'code'];
}
