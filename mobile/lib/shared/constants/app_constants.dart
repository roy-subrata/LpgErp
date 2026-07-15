class AppConstants {
  static const String appName = 'LPG ERP';
  static const String baseUrl = 'http://10.0.2.2:5000/api/v1';

  static const double defaultPadding = 16.0;
  static const double defaultBorderRadius = 8.0;

  static const Color primaryColor = Color(0xFF1A1A2E);
  static const Color secondaryColor = Color(0xFF16213E);
  static const Color accentColor = Color(0xFF0F3460);

  static const List<String> paymentTypes = ['Cash', 'Credit', 'Bank', 'Mobile Banking'];

  static const List<String> productTypes = [
    'EmptyCylinder',
    'GasRefill',
    'NewPackage',
    'Accessory',
  ];
}
