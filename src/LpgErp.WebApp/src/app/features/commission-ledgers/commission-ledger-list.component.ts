import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig, RowAction } from '../../shared/entity-list.component';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-commission-ledger-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `
    <app-entity-list
      [config]="config"
      [tabs]="tabs"
      [tabField]="'entityType'"
      [searchFields]="searchFields"
      [useCustomForm]="true"
      [reloadSignal]="reloadTick()"
    />
  `,
  styles: [`
    :host { display: block; }
  `]
})
export class CommissionLedgerListComponent {
  private api = inject(ApiService);

  reloadTick = signal(0);

  readonly tabs = [
    { label: 'Salesman', value: '0' },
    { label: 'Customer', value: '1' },
    { label: 'Supplier', value: '2' },
    { label: 'Driver', value: '3' },
  ];

  readonly config: EntityConfig = {
    endpoint: 'commissionledgers',
    title: 'Commission Ledger',
    singular: 'Commission Entry',
    cols: [
      { key: 'periodKey', label: 'Period', kind: 'main', sub: 'periodStart' },
      { key: 'entityType', label: 'Type', kind: 'badge', badgeMap: { '0': ['Salesman', '#eff6ff', '#1d4ed8'], '1': ['Customer', '#f0fdf4', '#15803d'], '2': ['Supplier', '#faf5ff', '#7e22ce'], '3': ['Driver', '#fff7ed', '#c2410c'] } },
      { key: 'policyName', label: 'Policy', kind: 'text' },
      { key: 'actualQuantity', label: 'Quantity', kind: 'num' },
      { key: 'actualAmount', label: 'Amount', kind: 'money' },
      { key: 'commissionEarned', label: 'Commission', kind: 'money' },
      { key: 'status', label: 'Status', kind: 'badge', badgeMap: { '0': ['Pending', '#f4f5f7', '#6b7280'], '1': ['Earned', '#fefce8', '#a16207'], '2': ['Applied', '#f0fdf4', '#15803d'], '3': ['Expired', '#f4f5f7', '#6b7280'], '4': ['Cancelled', '#f4f5f7', '#6b7280'] } },
    ],
    fields: [],
  };
  readonly searchFields = ['periodKey', 'policyName'];
}
