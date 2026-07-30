import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login.component').then(m => m.LoginComponent),
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
  },
  {
    path: 'reports',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/reports/reports.component').then(m => m.ReportsComponent),
  },
  {
    path: 'vehicle-loadings',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/vehicle-loadings/vehicle-loading-list.component').then(m => m.VehicleLoadingListComponent),
  },
  {
    path: 'vehicle-loadings/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/vehicle-loadings/vehicle-loading-detail.component').then(m => m.VehicleLoadingDetailComponent),
  },
  {
    path: 'inventory',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/inventory/inventory.component').then(m => m.InventoryComponent),
  },
  {
    path: 'loading-history',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/loading-history/loading-history.component').then(m => m.LoadingHistoryComponent),
  },
  {
    path: 'sales-orders',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/sales-orders/sales-order-list.component').then(m => m.SalesOrderListComponent),
  },
  {
    path: 'customers',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/customers/customer-list.component').then(m => m.CustomerListComponent),
  },
  {
    path: 'customers/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/customer-account/customer-account.component').then(m => m.CustomerAccountComponent),
  },
  {
    path: 'users',
    canActivate: [authGuard, roleGuard('Admin')],
    loadComponent: () =>
      import('./features/users/user-list.component').then(m => m.UserListComponent),
  },
  {
    path: 'roles',
    canActivate: [authGuard, roleGuard('Admin')],
    loadComponent: () =>
      import('./features/roles/role-list.component').then(m => m.RoleListComponent),
  },
  {
    path: 'salesmen',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/salesmen/salesman-list.component').then(m => m.SalesmanListComponent),
  },
  {
    path: 'brands',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/brands/brand-list.component').then(m => m.BrandListComponent),
  },
  {
    path: 'warehouses',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/warehouses/warehouse-list.component').then(m => m.WarehouseListComponent),
  },
  {
    path: 'products',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/products/product-list.component').then(m => m.ProductListComponent),
  },
  {
    path: 'suppliers',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/suppliers/supplier-list.component').then(m => m.SupplierListComponent),
  },
  {
    path: 'suppliers/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/supplier-account/supplier-account.component').then(m => m.SupplierAccountComponent),
  },
  {
    path: 'transport-companies',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/transport-companies/transport-company-list.component').then(m => m.TransportCompanyListComponent),
  },
  {
    path: 'cylinders',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/cylinders/cylinder-list.component').then(m => m.CylinderListComponent),
  },
  {
    path: 'cylinder-sizes',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/cylinder-sizes/cylinder-size-list.component').then(m => m.CylinderSizeListComponent),
  },
  {
    path: 'trucks',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/trucks/truck-list.component').then(m => m.TruckListComponent),
  },
  {
    path: 'drivers',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/drivers/driver-list.component').then(m => m.DriverListComponent),
  },
  {
    path: 'routes',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/routes/route-list.component').then(m => m.RouteListComponent),
  },
  {
    path: 'purchase-orders',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/purchase-orders/purchase-order-list.component').then(m => m.PurchaseOrderListComponent),
  },
  {
    path: 'goods-receipt',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/goods-receipt/goods-receipt-list.component').then(m => m.GoodsReceiptListComponent),
  },
  {
    path: 'goods-receipt/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/goods-receipt/goods-receipt-form.component').then(m => m.GoodsReceiptFormComponent),
  },
  {
    path: 'payments',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/payments/payment-list.component').then(m => m.PaymentListComponent),
  },
  {
    path: 'payment-accounts',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/payment-accounts/payment-account-list.component').then(m => m.PaymentAccountListComponent),
  },
  {
    path: 'stock-transfers',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/stock-transfers/stock-transfer-list.component').then(m => m.StockTransferListComponent),
  },
  {
    path: 'daily-sales-summaries',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/daily-sales-summaries/daily-sales-summary.component').then(m => m.DailySalesSummaryComponent),
  },
  {
    path: 'driver-settlements',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/driver-settlements/driver-settlement-list.component').then(m => m.DriverSettlementListComponent),
  },
  {
    path: 'salesman-settlements',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/salesman-settlements/salesman-settlement-list.component').then(m => m.SalesmanSettlementListComponent),
  },
  {
    path: 'cylinder-deposits',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/cylinder-deposits/cylinder-deposit-list.component').then(m => m.CylinderDepositListComponent),
  },
  {
    path: 'cylinder-exchanges',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/cylinder-exchanges/cylinder-exchange-list.component').then(m => m.CylinderExchangeListComponent),
  },
  {
    path: 'vehicle-closings',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/vehicle-closings/vehicle-closing-list.component').then(m => m.VehicleClosingListComponent),
  },
  {
    path: 'customer-cylinder-ledger',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/customer-cylinder-ledger/customer-cylinder-ledger.component').then(m => m.CustomerCylinderLedgerComponent),
  },
  {
    path: 'customer-gas-ledger',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/customer-gas-ledger/customer-gas-ledger.component').then(m => m.CustomerGasLedgerComponent),
  },
  {
    path: 'customer-credit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/customer-credit/customer-credit.component').then(m => m.CustomerCreditComponent),
  },
  {
    path: 'advance-refills',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/advance-refills/advance-refill.component').then(m => m.AdvanceRefillComponent),
  },
  {
    path: 'customer-notifications',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/customer-notifications/customer-notification-list.component').then(m => m.CustomerNotificationListComponent),
  },
  {
    path: 'price-histories',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/price-histories/price-history-list.component').then(m => m.PriceHistoryListComponent),
  },
  {
    path: 'commission-policies',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/commission-policies/commission-policy-list.component').then(m => m.CommissionPolicyListComponent),
  },
  {
    path: 'commission-ledgers',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/commission-ledgers/commission-ledger-list.component').then(m => m.CommissionLedgerListComponent),
  },
  { path: '**', redirectTo: 'dashboard' },
];
