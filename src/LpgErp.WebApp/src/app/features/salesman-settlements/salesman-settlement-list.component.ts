import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-salesman-settlement-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class SalesmanSettlementListComponent {
  readonly config: EntityConfig = {
    endpoint: 'salesmansettlements',
    title: 'Salesman Settlements',
    singular: 'Salesman Settlement',
    cols: [
      { key: 'salesmanName', label: 'Salesman', kind: 'main', sub: 'settlementDate' },
      { key: 'totalSales', label: 'Total Sales', kind: 'money' },
      { key: 'collection', label: 'Collection', kind: 'money' },
      { key: 'commission', label: 'Commission', kind: 'money' },
      { key: 'netSettlement', label: 'Net Settlement', kind: 'money' },
    ],
    fields: [
      { key: 'salesmanId', label: 'Salesman', type: 'select', endpoint: 'salesmen', optionLabel: 'name' },
      { key: 'settlementDate', label: 'Settlement Date', type: 'date' },
      { key: 'totalSales', label: 'Total Sales', type: 'number' },
      { key: 'collection', label: 'Collection', type: 'number' },
      { key: 'commission', label: 'Commission', type: 'number' },
      { key: 'dailyAllowance', label: 'Daily Allowance', type: 'number' },
      { key: 'bonus', label: 'Bonus', type: 'number' },
      { key: 'netSettlement', label: 'Net Settlement', type: 'number' },
      { key: 'notes', label: 'Notes', type: 'textarea' },
    ],
  };
  readonly searchFields = ['salesmanName'];
}
