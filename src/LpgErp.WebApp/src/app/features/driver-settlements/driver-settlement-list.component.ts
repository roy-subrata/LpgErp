import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-driver-settlement-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class DriverSettlementListComponent {
  readonly config: EntityConfig = {
    endpoint: 'driversettlements',
    title: 'Driver Settlements',
    singular: 'Driver Settlement',
    cols: [
      { key: 'driverName', label: 'Driver', kind: 'main', sub: 'settlementDate' },
      { key: 'tripCount', label: 'Trips', kind: 'num' },
      { key: 'fuelCost', label: 'Fuel Cost', kind: 'money' },
      { key: 'allowance', label: 'Allowance', kind: 'money' },
      { key: 'netSettlement', label: 'Net Settlement', kind: 'money' },
    ],
    fields: [
      { key: 'driverId', label: 'Driver', type: 'select', endpoint: 'drivers', optionLabel: 'name' },
      { key: 'vehicleLoadingId', label: 'Vehicle Loading', type: 'select', endpoint: 'vehicleloadings', optionLabel: 'truckName' },
      { key: 'settlementDate', label: 'Settlement Date', type: 'date' },
      { key: 'tripCount', label: 'Trip Count', type: 'number' },
      { key: 'fuelCost', label: 'Fuel Cost', type: 'number' },
      { key: 'allowance', label: 'Allowance', type: 'number' },
      { key: 'netSettlement', label: 'Net Settlement', type: 'number' },
      { key: 'notes', label: 'Notes', type: 'textarea' },
    ],
  };
  readonly searchFields = ['driverName'];
}
