import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../features/auth/pages/login_page.dart';
import '../../features/sales/pages/pos_page.dart';
import '../../features/customers/pages/customer_list_page.dart';
import '../../features/vehicles/pages/vehicle_loading_page.dart';
import '../../features/reports/pages/daily_summary_page.dart';
import '../../shared/widgets/main_scaffold.dart';

final appRouterProvider = Provider<GoRouter>((ref) {
  return GoRouter(
    initialLocation: '/login',
    routes: [
      GoRoute(
        path: '/login',
        builder: (context, state) => const LoginPage(),
      ),
      ShellRoute(
        builder: (context, state, child) => MainScaffold(child: child),
        routes: [
          GoRoute(
            path: '/pos',
            builder: (context, state) => const PosPage(),
          ),
          GoRoute(
            path: '/customers',
            builder: (context, state) => const CustomerListPage(),
          ),
          GoRoute(
            path: '/vehicle-loading',
            builder: (context, state) => const VehicleLoadingPage(),
          ),
          GoRoute(
            path: '/daily-summary',
            builder: (context, state) => const DailySummaryPage(),
          ),
        ],
      ),
    ],
  );
});
