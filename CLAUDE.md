# LPG Cylinder Distribution ERP

## Build & Run

### Backend
```bash
# Build
dotnet build src/LpgErp.sln

# Test
dotnet test src/LpgErp.Api.Tests

# Run API
dotnet run --project src/LpgErp.Api
```

### Frontend
```bash
cd src/LpgErp.WebApp
npm install
ng serve          # dev server on :4200
ng build          # production build
ng test           # unit tests
```

### Mobile
```bash
cd mobile
flutter analyze
flutter test
flutter run
```

## Project Layout

```
src/
├── LpgErp.sln
├── LpgErp.Domain/              # Entities, interfaces (no dependencies)
├── LpgErp.Application/         # Services, DTOs, validators
├── LpgErp.Infrastructure/      # DbContext, repositories, migrations
├── LpgErp.Api/                 # Controllers, middleware, Program.cs
├── LpgErp.WebApp/              # Angular 20 frontend
└── LpgErp.Api.Tests/           # Backend tests
```

## Architecture Rules

- **Clean Architecture**: Domain has zero dependencies. Dependencies point inward.
- **No CQRS/MediatR**: Controllers call application services directly.
- **No NgRx**: Use Angular Signals for state management.
- **Vertical Slice**: Group features by business capability, not technical layer.
- **No EF entities in API**: Always use DTOs.
- **Transactions**: All sales/stock/payment writes use explicit transactions.
- **Money**: Always `decimal`, never `float`.
- **Validation**: FluentValidation on backend, Reactive Forms on frontend.

## Key Business Concept

**Gas and Cylinders are separate assets with independent lifecycles.**
Every transaction must distinguish cylinder operations from gas operations.

## Code Style

- Follow `team/standards/*.md` for all code conventions
- Feature specs in `team/templates/feature-spec.md` before implementation
- Agent roles defined in `team/agents/*.md`
