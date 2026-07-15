# Backend Agent

## Role

Implements backend functionality following Clean Architecture and DDD principles. Writes API endpoints, application services, domain logic, and database operations for the LPG management system.

## Must Load

- `team/standards/architecture.md`
- `team/standards/coding.md`
- `team/standards/database.md`
- `team/standards/api.md`
- `team/docs/business-requirements.md`

## Technology Stack

- .NET 10 / ASP.NET Core
- Clean Architecture
- DDD (Domain-Driven Design)
- Vertical Slice Architecture
- Entity Framework Core
- MSSQL
- FluentValidation
- Serilog
- OpenTelemetry

## Core Concepts

### Dual Asset Tracking
Gas and Cylinders are separate assets with independent lifecycles:
- Cylinder: reusable, returnable asset (tracks ownership, movement, deposits)
- Gas: consumable product (tracks inventory, consumption, refills)

### Key Entities

**Cylinder Management**
- Cylinder (Id, BrandId, Size, SerialNumber, Status, WarehouseId)
- CylinderDeposit (Id, CylinderId, Amount, Status)

**Inventory**
- GasStock (Id, WarehouseId, Quantity, Unit)
- CylinderStock (Id, WarehouseId, BrandId, Size, Filled/Empty counts)

**Sales & Purchase**
- PurchaseOrder (Id, SupplierId, WarehouseId, TotalAmount, Status)
- SalesOrder (Id, CustomerId, WarehouseId, TotalAmount, PaymentType)
- SalesOrderItem (Id, SalesOrderId, ProductId, Quantity, UnitPrice)

**Customer Tracking**
- CustomerCylinderBalance (Id, CustomerId, BrandId, Size, Received, Returned, Outstanding)

## Architecture Patterns

### Vertical Slice Structure
```
src/LpgErp.Application/Features/
├── Purchases/
│   ├── CreatePurchase/
│   │   ├── CreatePurchaseCommand.cs
│   │   ├── CreatePurchaseHandler.cs
│   │   ├── CreatePurchaseValidator.cs
│   │   └── CreatePurchaseResponse.cs
│   └── GetPurchase/
│       ├── GetPurchaseQuery.cs
│       ├── GetPurchaseHandler.cs
│       └── GetPurchaseResponse.cs
└── Sales/
    └── ...
```

### Transaction Pattern
All sales/stock/payment writes go through transactions:
```csharp
await using var strategy = await _context.Database.CreateExecutionStrategy();
await strategy.ExecuteAsync(async () =>
{
    await using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // Business logic here
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
});
```

### Money Handling
- All money amounts use `decimal`
- Stock movements go through lots (FIFO for gas)

## Key Business Rules

1. **Purchase Commission**: Auto-adjust commission against next purchase
2. **Cylinder Exchange**: Track incoming/outgoing brands with exchange charges
3. **Advance Refill**: Track outstanding empty cylinders as customer liability
4. **Vehicle Loading/Unloading**: Track inventory movement from warehouse to vehicle
5. **Daily Reconciliation**: Vehicle closing with variance tracking

## Entity Framework Rules

- Never expose EF entities through APIs
- Use DTOs for all responses
- Configure relationships in `IEntityTypeConfiguration<T>`
- Use `builder.ToTable("...")` for table naming
- Soft delete for transactional data

## Anti-Patterns to Avoid

- Placing business logic in controllers
- Using `float` for money (use `decimal`)
- Exposing entities directly to frontend
- Manual service instantiation (use DI)
- Synchronous database access
