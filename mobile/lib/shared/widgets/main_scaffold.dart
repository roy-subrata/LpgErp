import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class MainScaffold extends StatelessWidget {
  final Widget child;

  const MainScaffold({super.key, required this.child});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: child,
      bottomNavigationBar: NavigationBar(
        selectedIndex: _getSelectedIndex(context),
        onDestinationSelected: (index) => _onTap(context, index),
        destinations: const [
          NavigationDestination(
            icon: Icon(Icons.point_of_sale),
            label: 'POS',
          ),
          NavigationDestination(
            icon: Icon(Icons.people),
            label: 'Customers',
          ),
          NavigationDestination(
            icon: Icon(Icons.local_shipping),
            label: 'Vehicle',
          ),
          NavigationDestination(
            icon: Icon(Icons.assessment),
            label: 'Summary',
          ),
        ],
      ),
    );
  }

  int _getSelectedIndex(BuildContext context) {
    final location = GoRouterState.of(context).uri.toString();
    if (location.startsWith('/pos')) return 0;
    if (location.startsWith('/customers')) return 1;
    if (location.startsWith('/vehicle')) return 2;
    if (location.startsWith('/daily')) return 3;
    return 0;
  }

  void _onTap(BuildContext context, int index) {
    switch (index) {
      case 0:
        context.go('/pos');
        break;
      case 1:
        context.go('/customers');
        break;
      case 2:
        context.go('/vehicle-loading');
        break;
      case 3:
        context.go('/daily-summary');
        break;
    }
  }
}
