import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-daily-sales-summary',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class DailySalesSummaryComponent {
  readonly config: EntityConfig = {
    endpoint: 'dailysales',
    title: 'Daily Sales',
    singular: 'Daily Sales Summary',
    cols: [
      { key: 'summaryDate', label: 'Date', kind: 'date', width: '120px' },
      { key: 'truckName', label: 'Truck', kind: 'text' },
      { key: 'salesmanName', label: 'Salesman', kind: 'main' },
      { key: 'totalSales', label: 'Total Sales', kind: 'money' },
      { key: 'cashSales', label: 'Cash Sales', kind: 'money' },
      { key: 'creditSales', label: 'Credit Sales', kind: 'money' },
      { key: 'packagesSold', label: 'Packages', kind: 'num' },
      { key: 'refillsSold', label: 'Refills', kind: 'num' },
    ],
    fields: [
      { key: 'vehicleLoadingId', label: 'Vehicle Loading', type: 'select' },
      { key: 'summaryDate', label: 'Summary Date', type: 'date' },
      { key: 'notes', label: 'Notes', type: 'textarea' },
    ],
  };
  readonly searchFields = ['truckName', 'salesmanName'];
}
