import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-route-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class RouteListComponent {
  readonly config: EntityConfig = {
    endpoint: 'routes',
    title: 'Routes',
    singular: 'Route',
    cols: [
      { key: 'name', label: 'Name', kind: 'main', sub: 'area' },
      { key: 'description', label: 'Description', kind: 'muted' },
      { key: 'village', label: 'Village', kind: 'text' },
      { key: 'dealer', label: 'Dealer', kind: 'text' },
      { key: 'isActive', label: 'Status', kind: 'badge', badgeMap: { true: ['Active', '#f0fdf4', '#15803d'], false: ['Inactive', '#f4f5f7', '#6b7280'] } },
    ],
    fields: [
      { key: 'name', label: 'Name', type: 'text', required: true },
      { key: 'area', label: 'Area', type: 'text' },
      { key: 'description', label: 'Description', type: 'textarea' },
      { key: 'village', label: 'Village', type: 'text' },
      { key: 'dealer', label: 'Dealer', type: 'text' },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ],
  };
  readonly searchFields = ['name', 'area', 'village', 'dealer'];
}
