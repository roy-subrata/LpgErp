# Frontend Agent

## Role

Implements Angular frontend for the LPG management system. Builds user interfaces for warehouse operations, sales, purchasing, inventory management, and reporting.

## Must Load

- `team/standards/angular.md`
- `team/standards/coding.md`
- `team/docs/business-requirements.md`

## Technology Stack

- Angular 20
- Standalone Components
- Signals
- Tailwind CSS
- PrimeNg

## Core Features

### Dashboard
- Daily sales summary
- Stock levels (gas, cylinders by brand/size)
- Pending deliveries
- Outstanding payments
- Vehicle status

### Inventory Management
- Warehouse stock view (filled/empty cylinders, gas)
- Stock transfer between warehouses
- Low stock alerts
- Cylinder movement history

### Sales Module
- Point of Sale (POS) for walk-in customers
- Sales order creation (New Package, Gas Refill, Empty Cylinder, Accessories)
- Customer cylinder balance tracking
- Credit sales management

### Purchase Module
- Purchase order creation
- Goods receiving
- Supplier management
- Commission tracking

### Vehicle Management
- Vehicle loading/unloading
- Daily route tracking
- Vehicle closing/reconciliation
- Driver/salesman settlement

### Customer Module
- Customer profiles
- Cylinder ledger
- Payment history
- Credit management

## Component Structure

```
src/app/features/
├── dashboard/
├── inventory/
│   ├── warehouse-stock/
│   ├── stock-transfer/
│   └── cylinder-movement/
├── sales/
│   ├── pos/
│   ├── sales-orders/
│   └── customer-balance/
├── purchases/
│   ├── purchase-orders/
│   ├── goods-receiving/
│   └── suppliers/
├── vehicles/
│   ├── vehicle-loading/
│   ├── route-tracking/
│   └── vehicle-closing/
├── customers/
│   ├── customer-list/
│   ├── customer-detail/
│   └── cylinder-ledger/
└── reports/
    ├── inventory-reports/
    ├── sales-reports/
    └── financial-reports/
```

## Key Patterns

### Smart vs Presentational Components
- Smart components handle API calls and state
- Presentational components only display UI
- Business logic stays in services

### Signal Usage
```typescript
// Local state
products = signal<Product[]>([]);
selectedProduct = signal<Product | null>(null);

// Computed values
filteredProducts = computed(() => 
  this.products().filter(p => p.isActive)
);
```

### Form Handling
- Use Reactive Forms for complex forms
- Create reusable validators
- Separate form models from API DTOs

## UI Guidelines

- Use PrimeNg components for tables, dialogs, forms
- Tailwind CSS for layout and styling
- Consistent spacing and typography
- Mobile-responsive design
- Loading states for async operations
- Error messages for failed operations

## Anti-Patterns to Avoid

- Direct HTTP calls in components (use services)
- Business logic in templates
- Using `any` type
- Mutating shared state directly
- Large components (200-300 lines max)
