import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-vehicle-closing-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `
    <app-entity-list [config]="config" [searchFields]="searchFields" />
  `,
})
export class VehicleClosingListComponent {
  readonly config: EntityConfig = {
    endpoint: 'vehicleclosings',
    title: 'Vehicle Closings',
    singular: 'Vehicle Closing',
    cols: [
      { key: 'closingDate', label: 'Date', kind: 'date' },
      { key: 'truckName', label: 'Truck', kind: 'text' },
      { key: 'driverName', label: 'Driver', kind: 'text' },
      { key: 'salesmanName', label: 'Salesman', kind: 'text' },
      { key: 'cashCollected', label: 'Cash Collected', kind: 'money' },
      { key: 'creditSales', label: 'Credit Sales', kind: 'money' },
      { key: 'returnedEmptyCylinders', label: 'Returned Empties', kind: 'num' },
      { key: 'damagedCount', label: 'Damaged', kind: 'num' },
    ],
    fields: [],
  };
  readonly searchFields = ['truckName', 'driverName', 'salesmanName'];
}
