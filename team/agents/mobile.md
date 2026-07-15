# Mobile Agent

## Role

Implements Flutter mobile application for field sales and delivery operations in the LPG management system. Handles offline-capable mobile sales, vehicle tracking, and route management.

## Must Load

- `team/docs/business-requirements.md`
- Flutter and Riverpod documentation

## Technology Stack

- Flutter
- Riverpod (State Management)
- Dio (HTTP Client)
- GoRouter (Navigation)

## Core Features

### Mobile Sales
- Point of Sale for field sales
- Cash and credit transactions
- Package, refill, empty cylinder, and accessory sales
- Cylinder exchange tracking
- Customer signature capture

### Vehicle Management
- Vehicle loading at warehouse
- Daily route tracking
- GPS location (optional)
- Vehicle closing/reconciliation

### Customer Management
- Customer list with search
- Customer cylinder balance view
- Payment collection
- Due amount tracking

### Offline Support
- Local SQLite database for offline sales
- Sync when connection available
- Conflict resolution

## App Structure

```
mobile/lib/
├── features/
│   ├── auth/
│   ├── sales/
│   │   ├── pages/
│   │   ├── models/
│   │   └── providers/
│   ├── vehicles/
│   ├── customers/
│   └── reports/
├── shared/
│   ├── models/
│   ├── services/
│   └── widgets/
├── core/
│   ├── api/
│   ├── database/
│   └── utils/
└── main.dart
```

## State Management

### Provider Pattern
```dart
final salesProvider = StateNotifierProvider<SalesNotifier, SalesState>((ref) {
  return SalesNotifier(ref);
});
```

### API Service
```dart
class ApiService {
  final Dio _dio;
  
  Future<List<Customer>> getCustomers() async {
    final response = await _dio.get('/api/v1/customers');
    return (response.data['data'] as List)
        .map((json) => Customer.fromJson(json))
        .toList();
  }
}
```

## Offline Database

Use SQLite (sqflite) for:
- Pending sales transactions
- Customer data cache
- Product catalog cache
- Vehicle inventory

## Key Scenarios

### Field Sale
1. Salesman opens app
2. Selects customer or creates new
3. Adds items (package, refill, empty, accessories)
4. Selects payment type (cash/credit)
5. Captures signature
6. Sale saved locally
7. Syncs when online

### Vehicle Loading
1. Driver checks in at warehouse
2. System shows expected loading list
3. Driver confirms quantities
4. Loading recorded
5. Vehicle deploys

### Vehicle Closing
1. Driver returns at end of day
2. System shows expected vs actual inventory
3. Driver enters sold quantities
4. System calculates variance
5. Cash collected recorded
6. Settlement generated

## API Integration

- REST API with JSON
- JWT authentication
- Retry logic for failed requests
- Timeout handling

## Offline Sync Strategy

1. Queue transactions locally
2. Check connectivity periodically
3. Sync when connection available
4. Resolve conflicts (server wins)
5. Show sync status to user

## Testing

```bash
flutter analyze
flutter test
flutter build apk
flutter build ios
```

## Anti-Patterns to Avoid

- Business logic in widgets
- Direct HTTP calls in UI (use providers)
- Hardcoded URLs
- Missing offline handling
- Large widget trees
