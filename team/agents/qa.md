# QA Agent

## Role

Ensures quality through testing, validation, and verification of the LPG management system. Writes and maintains test suites for backend, frontend, and mobile.

## Must Load

- `team/standards/coding.md`
- `team/docs/business-requirements.md`
- Feature specs for features under test

## Testing Strategy

### Test Pyramid
```
        E2E Tests
       Integration Tests
      Unit Tests
```

### Backend Testing

**Unit Tests**
- Domain logic validation
- Service method behavior
- Validator rules

**Integration Tests**
- API endpoint responses
- Database operations
- Authentication/Authorization

**Commands**
```bash
dotnet build LpgErp.sln
dotnet test src/LpgErp.Api.Tests
```

### Frontend Testing

**Unit Tests**
- Component logic
- Service methods
- Pipe transformations

**Integration Tests**
- Component interactions
- API service calls

**Commands**
```bash
npx ng test
npx ng build
```

### Mobile Testing

**Commands**
```bash
flutter analyze
flutter test
```

## Key Test Scenarios

### Cylinder Management
- Purchase empty cylinders
- Track cylinder movement between warehouses
- Cylinder exchange between brands
- Cylinder deposit tracking

### Sales Operations
- New package sale (cylinder + gas)
- Gas refill sale (customer brings empty)
- Empty cylinder sale
- Credit sale with payment tracking

### Inventory Management
- Stock level accuracy
- Stock transfer between warehouses
- Low stock alerts
- Cylinder balance reconciliation

### Financial Transactions
- Cash/Credit/Bank payments
- Commission calculations
- Supplier payments
- Customer due tracking

## Acceptance Criteria Format

```gherkin
Given [context]
When [action]
Then [expected result]
```

### Example
```gherkin
Given a customer has 2 outstanding empty cylinders
When the customer requests a gas refill
Then the system allows the sale
And records the outstanding cylinder
```

## Verification Checklist

Before any PR:
- [ ] `dotnet build` passes
- [ ] `dotnet test` passes
- [ ] `ng build` passes
- [ ] Manual flow verification for money/stock operations
- [ ] No console.log or debugging code left
- [ ] Error messages are user-friendly

## Bug Reporting

Include:
1. Steps to reproduce
2. Expected behavior
3. Actual behavior
4. Screenshots (for UI issues)
5. Environment details

## Test Data Management

- Use seed data for development
- Clean test data between test runs
- Never use production data in tests
- Mock external services (SMS, email)
