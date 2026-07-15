import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../core/models/models.dart';

class PosPage extends ConsumerStatefulWidget {
  const PosPage({super.key});

  @override
  ConsumerState<PosPage> createState() => _PosPageState();
}

class _PosPageState extends ConsumerState<PosPage> {
  Customer? _selectedCustomer;
  final List<SalesOrderItem> _items = [];
  String _paymentType = 'Cash';

  double get _totalAmount => _items.fold(0, (sum, item) => sum + item.totalPrice);

  void _addItem(Product product) {
    setState(() {
      final existing = _items.where((i) => i.productId == product.id).toList();
      if (existing.isNotEmpty) {
        final index = _items.indexOf(existing.first);
        _items[index] = SalesOrderItem(
          productId: product.id,
          productName: product.name,
          quantity: existing.first.quantity + 1,
          unitPrice: existing.first.unitPrice,
        );
      } else {
        _items.add(SalesOrderItem(
          productId: product.id,
          productName: product.name,
          quantity: 1,
          unitPrice: product.salePrice,
        ));
      }
    });
  }

  void _removeItem(int index) {
    setState(() {
      _items.removeAt(index);
    });
  }

  void _submitSale() {
    if (_selectedCustomer == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please select a customer')),
      );
      return;
    }
    if (_items.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please add items')),
      );
      return;
    }

    // TODO: Submit to API
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Sale submitted successfully')),
    );

    setState(() {
      _items.clear();
      _selectedCustomer = null;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Point of Sale'),
        backgroundColor: const Color(0xFF1A1A2E),
        foregroundColor: Colors.white,
      ),
      body: Column(
        children: [
          // Customer selector
          Padding(
            padding: const EdgeInsets.all(16),
            child: DropdownButtonFormField<Customer>(
              value: _selectedCustomer,
              decoration: const InputDecoration(
                labelText: 'Select Customer',
                border: OutlineInputBorder(),
                prefixIcon: Icon(Icons.person),
              ),
              items: const [], // TODO: Load from API
              onChanged: (customer) {
                setState(() => _selectedCustomer = customer);
              },
            ),
          ),

          // Cart items
          Expanded(
            child: _items.isEmpty
                ? const Center(child: Text('No items in cart'))
                : ListView.builder(
                    itemCount: _items.length,
                    itemBuilder: (context, index) {
                      final item = _items[index];
                      return ListTile(
                        title: Text(item.productName),
                        subtitle: Text(
                          '${item.quantity} x \$${item.unitPrice.toStringAsFixed(2)}',
                        ),
                        trailing: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Text(
                              '\$${item.totalPrice.toStringAsFixed(2)}',
                              style: const TextStyle(fontWeight: FontWeight.bold),
                            ),
                            IconButton(
                              icon: const Icon(Icons.delete, color: Colors.red),
                              onPressed: () => _removeItem(index),
                            ),
                          ],
                        ),
                      );
                    },
                  ),
          ),

          // Payment type and total
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: Colors.grey[100],
              border: Border(top: BorderSide(color: Colors.grey[300]!)),
            ),
            child: Column(
              children: [
                Row(
                  children: [
                    const Text('Payment: '),
                    Expanded(
                      child: SegmentedButton<String>(
                        segments: const [
                          ButtonSegment(value: 'Cash', label: Text('Cash')),
                          ButtonSegment(value: 'Credit', label: Text('Credit')),
                        ],
                        selected: {_paymentType},
                        onSelectionChanged: (selected) {
                          setState(() => _paymentType = selected.first);
                        },
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      'Total: \$${_totalAmount.toStringAsFixed(2)}',
                      style: const TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    ElevatedButton(
                      onPressed: _items.isEmpty ? null : _submitSale,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xFF1A1A2E),
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(
                          horizontal: 32,
                          vertical: 16,
                        ),
                      ),
                      child: const Text('Submit Sale'),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () {
          // TODO: Show product catalog
        },
        backgroundColor: const Color(0xFF1A1A2E),
        child: const Icon(Icons.add, color: Colors.white),
      ),
    );
  }
}
