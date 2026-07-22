import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
  },
  {
    path: 'reports',
    loadComponent: () =>
      import('./features/reports/reports.component').then(m => m.ReportsComponent),
  },
  {
    path: 'vehicle-loadings',
    loadComponent: () =>
      import('./features/vehicle-loadings/vehicle-loading-list.component').then(m => m.VehicleLoadingListComponent),
  },
  {
    path: 'vehicle-loadings/:id',
    loadComponent: () =>
      import('./features/vehicle-loadings/vehicle-loading-detail.component').then(m => m.VehicleLoadingDetailComponent),
  },
  {
    path: 'inventory',
    loadComponent: () =>
      import('./features/inventory/inventory.component').then(m => m.InventoryComponent),
  },
  {
    path: 'loading-history',
    loadComponent: () =>
      import('./features/loading-history/loading-history.component').then(m => m.LoadingHistoryComponent),
  },
  {
    path: 'sales-orders',
    loadComponent: () =>
      import('./features/sales-orders/sales-order-list.component').then(m => m.SalesOrderListComponent),
  },
  {
    path: 'customers',
    loadComponent: () =>
      import('./features/customers/customer-list.component').then(m => m.CustomerListComponent),
  },
  {
    path: 'salesmen',
    loadComponent: () =>
      import('./features/salesmen/salesman-list.component').then(m => m.SalesmanListComponent),
  },
  {
    path: 'brands',
    loadComponent: () =>
      import('./features/brands/brand-list.component').then(m => m.BrandListComponent),
  },
  {
    path: 'warehouses',
    loadComponent: () =>
      import('./features/warehouses/warehouse-list.component').then(m => m.WarehouseListComponent),
  },
  {
    path: 'products',
    loadComponent: () =>
      import('./features/products/product-list.component').then(m => m.ProductListComponent),
  },
  {
    path: 'suppliers',
    loadComponent: () =>
      import('./features/suppliers/supplier-list.component').then(m => m.SupplierListComponent),
  },
  {
    path: 'transport-companies',
    loadComponent: () =>
      import('./features/transport-companies/transport-company-list.component').then(m => m.TransportCompanyListComponent),
  },
  {
    path: 'cylinders',
    loadComponent: () =>
      import('./features/cylinders/cylinder-list.component').then(m => m.CylinderListComponent),
  },
  {
    path: 'cylinder-sizes',
    loadComponent: () =>
      import('./features/cylinder-sizes/cylinder-size-list.component').then(m => m.CylinderSizeListComponent),
  },
  {
    path: 'trucks',
    loadComponent: () =>
      import('./features/trucks/truck-list.component').then(m => m.TruckListComponent),
  },
  {
    path: 'drivers',
    loadComponent: () =>
      import('./features/drivers/driver-list.component').then(m => m.DriverListComponent),
  },
  {
    path: 'routes',
    loadComponent: () =>
      import('./features/routes/route-list.component').then(m => m.RouteListComponent),
  },
  {
    path: 'purchase-orders',
    loadComponent: () =>
      import('./features/purchase-orders/purchase-order-list.component').then(m => m.PurchaseOrderListComponent),
  },
  {
    path: 'payments',
    loadComponent: () =>
      import('./features/payments/payment-list.component').then(m => m.PaymentListComponent),
  },
  {
    path: 'stock-transfers',
    loadComponent: () =>
      import('./features/stock-transfers/stock-transfer-list.component').then(m => m.StockTransferListComponent),
  },
  {
    path: 'daily-sales-summaries',
    loadComponent: () =>
      import('./features/daily-sales-summaries/daily-sales-summary.component').then(m => m.DailySalesSummaryComponent),
  },
  {
    path: 'driver-settlements',
    loadComponent: () =>
      import('./features/driver-settlements/driver-settlement-list.component').then(m => m.DriverSettlementListComponent),
  },
  {
    path: 'salesman-settlements',
    loadComponent: () =>
      import('./features/salesman-settlements/salesman-settlement-list.component').then(m => m.SalesmanSettlementListComponent),
  },
  {
    path: 'cylinder-deposits',
    loadComponent: () =>
      import('./features/cylinder-deposits/cylinder-deposit-list.component').then(m => m.CylinderDepositListComponent),
  },
  {
    path: 'cylinder-exchanges',
    loadComponent: () =>
      import('./features/cylinder-exchanges/cylinder-exchange-list.component').then(m => m.CylinderExchangeListComponent),
  },
  {
    path: 'vehicle-closings',
    loadComponent: () =>
      import('./features/vehicle-closings/vehicle-closing-list.component').then(m => m.VehicleClosingListComponent),
  },
  {
    path: 'customer-cylinder-ledger',
    loadComponent: () =>
      import('./features/customer-cylinder-ledger/customer-cylinder-ledger.component').then(m => m.CustomerCylinderLedgerComponent),
  },
  {
    path: 'customer-gas-ledger',
    loadComponent: () =>
      import('./features/customer-gas-ledger/customer-gas-ledger.component').then(m => m.CustomerGasLedgerComponent),
  },
  {
    path: 'customer-credit',
    loadComponent: () =>
      import('./features/customer-credit/customer-credit.component').then(m => m.CustomerCreditComponent),
  },
  {
    path: 'advance-refills',
    loadComponent: () =>
      import('./features/advance-refills/advance-refill.component').then(m => m.AdvanceRefillComponent),
  },
  {
    path: 'customer-notifications',
    loadComponent: () =>
      import('./features/customer-notifications/customer-notification-list.component').then(m => m.CustomerNotificationListComponent),
  },
  { path: '**', redirectTo: 'dashboard' },
];
