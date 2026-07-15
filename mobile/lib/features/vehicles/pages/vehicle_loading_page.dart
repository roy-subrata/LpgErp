import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class VehicleLoadingPage extends ConsumerStatefulWidget {
  const VehicleLoadingPage({super.key});

  @override
  ConsumerState<VehicleLoadingPage> createState() => _VehicleLoadingPageState();
}

class _VehicleLoadingPageState extends ConsumerState<VehicleLoadingPage> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Vehicle Loading'),
        backgroundColor: const Color(0xFF1A1A2E),
        foregroundColor: Colors.white,
      ),
      body: const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.local_shipping, size: 80, color: Colors.grey),
            SizedBox(height: 16),
            Text(
              'Vehicle Loading',
              style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
            ),
            SizedBox(height: 8),
            Text(
              'No active loading session',
              style: TextStyle(color: Colors.grey),
            ),
          ],
        ),
      ),
    );
  }
}
