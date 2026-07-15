class ApiResponse<T> {
  final T? data;
  final String? error;
  final int? statusCode;

  ApiResponse({this.data, this.error, this.statusCode});

  bool get isSuccess => error == null && statusCode != null && statusCode! >= 200 && statusCode! < 300;
}

class Customer {
  final String id;
  final String name;
  final String? code;
  final String type;
  final String? phone;
  final String? email;
  final String? address;
  final double creditLimit;
  final bool isActive;

  Customer({
    required this.id,
    required this.name,
    this.code,
    required this.type,
    this.phone,
    this.email,
    this.address,
    this.creditLimit = 0,
    this.isActive = true,
  });

  factory Customer.fromJson(Map<String, dynamic> json) {
    return Customer(
      id: json['id'],
      name: json['name'],
      code: json['code'],
      type: json['type'],
      phone: json['phone'],
      email: json['email'],
      address: json['address'],
      creditLimit: (json['creditLimit'] ?? 0).toDouble(),
      isActive: json['isActive'] ?? true,
    );
  }
}

class Product {
  final String id;
  final String name;
  final String? code;
  final String type;
  final double purchasePrice;
  final double salePrice;
  final int currentStock;
  final bool isActive;

  Product({
    required this.id,
    required this.name,
    this.code,
    required this.type,
    this.purchasePrice = 0,
    required this.salePrice,
    this.currentStock = 0,
    this.isActive = true,
  });

  factory Product.fromJson(Map<String, dynamic> json) {
    return Product(
      id: json['id'],
      name: json['name'],
      code: json['code'],
      type: json['type'],
      purchasePrice: (json['purchasePrice'] ?? 0).toDouble(),
      salePrice: (json['salePrice'] ?? 0).toDouble(),
      currentStock: json['currentStock'] ?? 0,
      isActive: json['isActive'] ?? true,
    );
  }
}

class SalesOrderItem {
  final String productId;
  final String productName;
  final int quantity;
  final double unitPrice;

  SalesOrderItem({
    required this.productId,
    required this.productName,
    required this.quantity,
    required this.unitPrice,
  });

  double get totalPrice => quantity * unitPrice;

  Map<String, dynamic> toJson() => {
        'productId': productId,
        'quantity': quantity,
        'unitPrice': unitPrice,
      };
}

class Warehouse {
  final String id;
  final String name;
  final String? code;
  final String? address;
  final bool isActive;

  Warehouse({
    required this.id,
    required this.name,
    this.code,
    this.address,
    this.isActive = true,
  });

  factory Warehouse.fromJson(Map<String, dynamic> json) {
    return Warehouse(
      id: json['id'],
      name: json['name'],
      code: json['code'],
      address: json['address'],
      isActive: json['isActive'] ?? true,
    );
  }
}

class Brand {
  final String id;
  final String name;
  final String? code;
  final bool isActive;

  Brand({
    required this.id,
    required this.name,
    this.code,
    this.isActive = true,
  });

  factory Brand.fromJson(Map<String, dynamic> json) {
    return Brand(
      id: json['id'],
      name: json['name'],
      code: json['code'],
      isActive: json['isActive'] ?? true,
    );
  }
}
