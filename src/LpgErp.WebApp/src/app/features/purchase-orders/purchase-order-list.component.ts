import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig, RowAction } from '../../shared/entity-list.component';
import { PurchaseOrderFormComponent } from './purchase-order-form.component';
import { PurchaseOrderReceiveComponent } from './purchase-order-receive.component';

@Component({
  selector: 'app-purchase-order-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent, PurchaseOrderFormComponent, PurchaseOrderReceiveComponent],
  template: `
    <app-entity-list
      [config]="config"
      [searchFields]="searchFields"
      [useCustomForm]="true"
      [rowActions]="rowActions"
      [reloadSignal]="reloadTick()"
      (newRequested)="openForm(null)"
      (editRequested)="openForm($event.id)"
      (rowAction)="onRowAction($event)"
    />
    <app-purchase-order-form
      [open]="formOpen()"
      [entityId]="editId()"
      (close)="formOpen.set(false)"
      (saved)="onSaved()"
    />
    <app-purchase-order-receive
      [open]="receiveOpen()"
      [entityId]="receiveId()"
      (close)="receiveOpen.set(false)"
      (received)="onReceived()"
    />
  `,
})
export class PurchaseOrderListComponent {
  formOpen = signal(false);
  editId = signal<string | null>(null);
  receiveOpen = signal(false);
  receiveId = signal<string | null>(null);
  reloadTick = signal(0);

  // "Receive" is only offered for orders that are confirmed, in transit, or partially received
  // (PurchaseOrderStatus 1/2/3) — not drafts, fully-received, or cancelled. Enums serialize as numbers.
  readonly rowActions: RowAction[] = [
    {
      key: 'receive',
      icon: '📥',
      title: 'Receive goods',
      show: (po) => [1, 2, 3].includes(po.status),
    },
  ];

  openForm(id: string | null) {
    this.editId.set(id);
    this.formOpen.set(true);
  }

  onRowAction(e: { key: string; item: any }) {
    if (e.key === 'receive') {
      this.receiveId.set(e.item.id);
      this.receiveOpen.set(true);
    }
  }

  onReceived() {
    this.receiveOpen.set(false);
    this.reloadTick.update(v => v + 1);
  }

  onSaved() {
    this.formOpen.set(false);
    this.reloadTick.update(v => v + 1);
  }

  readonly config: EntityConfig = {
    endpoint: 'purchaseorders',
    title: 'Purchase Orders',
    singular: 'Purchase Order',
    cols: [
      { key: 'orderNumber', label: 'Order Number', kind: 'mono', width: '130px' },
      { key: 'supplierName', label: 'Supplier', kind: 'main' },
      { key: 'warehouseName', label: 'Warehouse', kind: 'text' },
      { key: 'orderDate', label: 'Order Date', kind: 'date' },
      { key: 'totalAmount', label: 'Total Amount', kind: 'money' },
      { key: 'commissionApplied', label: 'Commission Applied', kind: 'money' },
      { key: 'netPayable', label: 'Net Payable', kind: 'money' },
      {
        key: 'status',
        label: 'Status',
        kind: 'badge',
        // Keyed by the serialized PurchaseOrderStatus values (enums serialize as numbers).
        // [label, background, color] so the label is explicit and independent of the shared status map.
        badgeMap: {
          '0': ['Draft', '#f4f5f7', '#6b7280'],
          '1': ['Confirmed', '#dbeafe', '#1d4ed8'],
          '2': ['In Transit', '#e0e7ff', '#4338ca'],
          '3': ['Partially Received', '#fef3c7', '#92400e'],
          '4': ['Received', '#dcfce7', '#166534'],
          '5': ['Cancelled', '#fee2e2', '#991b1b'],
        },
      },
    ],
    fields: [
      { key: 'supplierId', label: 'Supplier', type: 'select', endpoint: 'suppliers', optionLabel: 'name' },
      { key: 'warehouseId', label: 'Warehouse', type: 'select', endpoint: 'warehouses', optionLabel: 'name' },
      { key: 'orderDate', label: 'Order Date', type: 'date' },
      { key: 'expectedDeliveryDate', label: 'Expected Delivery Date', type: 'date' },
      { key: 'notes', label: 'Notes', type: 'textarea' },
    ],
  };
  readonly searchFields = ['orderNumber', 'supplierName', 'warehouseName'];
}
