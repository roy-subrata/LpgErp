import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
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
    path: 'customers',
    loadComponent: () =>
      import('./features/customers/customer-list.component').then(m => m.CustomerListComponent),
  },
  {
    path: 'products',
    loadComponent: () =>
      import('./features/products/product-list.component').then(m => m.ProductListComponent),
  },
  { path: '**', redirectTo: 'dashboard' },
];
