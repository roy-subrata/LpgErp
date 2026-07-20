export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors: string[];
  pagination?: PaginationMeta;
}

export interface PaginationMeta {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PagedResult<T> {
  items: T[];
  pagination: PaginationMeta;
}

export interface Brand {
  id: string;
  name: string;
  code: string;
  isActive: boolean;
}

export interface Customer {
  id: string;
  name: string;
  code: string;
  type: number;
  contactPerson: string;
  phone: string;
  email: string;
  address: string;
  creditLimit: number;
  paymentDueDays: number;
  isActive: boolean;
}

export interface Product {
  id: string;
  name: string;
  code: string;
  type: number;
  brandId: string;
  brandName: string;
  cylinderSizeId: string;
  cylinderSizeName: string;
  purchasePrice: number;
  salePrice: number;
  currentStock: number;
  minimumStock: number;
  isActive: boolean;
}

export interface Warehouse {
  id: string;
  name: string;
  code: string;
  address: string;
  phone: string;
  isActive: boolean;
}

export interface Supplier {
  id: string;
  name: string;
  code: string;
  contactPerson: string;
  phone: string;
  email: string;
  address: string;
  isLpgCompany: boolean;
  brandId: string;
  brandName: string;
  isActive: boolean;
}

export interface Cylinder {
  id: string;
  brandId: string;
  brandName: string;
  cylinderSizeId: string;
  cylinderSizeName: string;
  serialNumber: string;
  status: number;
  currentWarehouseId: string;
  currentWarehouseName: string;
  hasGas: boolean;
}

export interface CylinderSize {
  id: string;
  brandId: string;
  brandName: string;
  name: string;
  weightKg: number;
  depositAmount: number;
  isActive: boolean;
}

export interface PurchaseOrder {
  id: string;
  orderNumber: string;
  supplierId: string;
  supplierName: string;
  warehouseId: string;
  warehouseName: string;
  transportCompanyId: string;
  transportCompanyName: string;
  transportationCost: number;
  status: number;
  totalAmount: number;
  commissionEarned: number;
  orderDate: string;
  expectedDeliveryDate: string;
  dueDate: string;
  receivedDate: string;
  notes: string;
  items: PurchaseOrderItem[];
}

export interface PurchaseOrderItem {
  id: string;
  productId: string;
  productName: string;
  orderedQuantity: number;
  receivedQuantity: number;
  unitPrice: number;
  totalPrice: number;
  damagedQuantity: number;
}

export interface SalesOrder {
  id: string;
  orderNumber: string;
  customerId: string;
  customerName: string;
  warehouseId: string;
  warehouseName: string;
  transportCompanyId: string;
  transportCompanyName: string;
  status: number;
  totalAmount: number;
  discount: number;
  netAmount: number;
  orderDate: string;
  dueDate: string;
  notes: string;
  isCreditSale: boolean;
  items: SalesOrderItem[];
}

export interface SalesOrderItem {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  cylinderExchangeQuantity: number;
}

export interface Payment {
  id: string;
  salesOrderId: string;
  salesOrderNumber: string;
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  method: number;
  direction: number;
  amount: number;
  paymentDate: string;
  reference: string;
  notes: string;
}

export interface Truck {
  id: string;
  plateNumber: string;
  name: string;
  capacity: number;
  isActive: boolean;
}

export interface Driver {
  id: string;
  name: string;
  phone: string;
  licenseNumber: string;
  isActive: boolean;
}

export interface Salesman {
  id: string;
  name: string;
  phone: string;
  code: string;
  isActive: boolean;
}

export interface Route {
  id: string;
  name: string;
  area: string;
  description: string;
  village: string;
  dealer: string;
  isActive: boolean;
}

export interface VehicleLoading {
  id: string;
  loadingDate: string;
  truckId: string;
  truckName: string;
  driverId: string;
  driverName: string;
  salesmanId: string;
  salesmanName: string;
  warehouseId: string;
  warehouseName: string;
  status: number;
  notes: string;
  items: VehicleLoadingItem[];
}

export interface VehicleLoadingItem {
  id: string;
  productId: string;
  productName: string;
  loadedQuantity: number;
  soldQuantity: number;
  returnedQuantity: number;
  damagedQuantity: number;
}

export interface DriverSettlement {
  id: string;
  driverId: string;
  driverName: string;
  settlementDate: string;
  vehicleLoadingId: string;
  tripCount: number;
  fuelCost: number;
  allowance: number;
  loadingCost: number;
  unloadingCost: number;
  tripIncome: number;
  companyPickupIncentive: number;
  distance: number;
  netSettlement: number;
  notes: string;
}

export interface SalesmanSettlement {
  id: string;
  salesmanId: string;
  salesmanName: string;
  settlementDate: string;
  totalSales: number;
  collection: number;
  commission: number;
  dailyAllowance: number;
  bonus: number;
  netSettlement: number;
  notes: string;
}

export interface CylinderDeposit {
  id: string;
  customerId: string;
  customerName: string;
  cylinderSizeId: string;
  cylinderSizeName: string;
  type: number;
  amount: number;
  quantity: number;
  depositDate: string;
  reference: string;
  notes: string;
}

export interface CylinderExchange {
  id: string;
  customerId: string;
  customerName: string;
  incomingBrandId: string;
  incomingBrandName: string;
  incomingCylinderSizeId: string;
  incomingCylinderSizeName: string;
  incomingQuantity: number;
  outgoingBrandId: string;
  outgoingBrandName: string;
  outgoingCylinderSizeId: string;
  outgoingCylinderSizeName: string;
  outgoingQuantity: number;
  exchangeCharge: number;
  exchangeDate: string;
  notes: string;
}

export interface CustomerNotification {
  id: string;
  customerId: string;
  customerName: string;
  type: number;
  title: string;
  message: string;
  isRead: boolean;
  isSent: boolean;
}

export interface StockMovement {
  id: string;
  productId: string;
  productName: string;
  type: number;
  quantity: number;
  fromWarehouseId: string;
  fromWarehouseName: string;
  toWarehouseId: string;
  toWarehouseName: string;
  reference: string;
  movementDate: string;
}

export interface StockTransferRequest {
  productId: string;
  fromWarehouseId: string;
  toWarehouseId: string;
  quantity: number;
  reference: string;
}

export interface StockTransferResponse {
  movementId: string;
  quantity: number;
  productName: string;
  fromWarehouseName: string;
  toWarehouseName: string;
}

export interface InventoryReport {
  productName: string;
  warehouseName: string;
  quantity: number;
  brandName: string;
  productType: string;
}

export interface SalesReport {
  orderNumber: string;
  customerName: string;
  orderDate: string;
  totalAmount: number;
  discount: number;
  netAmount: number;
  status: string;
}

export interface PurchaseReport {
  orderNumber: string;
  supplierName: string;
  orderDate: string;
  totalAmount: number;
  commissionEarned: number;
  status: string;
}

export interface CustomerReport {
  customerName: string;
  customerCode: string;
  creditLimit: number;
  totalPurchases: number;
  totalPayments: number;
  outstandingBalance: number;
  cylinderBalance: number;
}

export interface FinancialReport {
  totalSales: number;
  totalPayments: number;
  totalPurchases: number;
  totalPurchasePayments: number;
  accountsReceivable: number;
  supplierPayable: number;
  transportationExpenses: number;
  commissionBalance: number;
  depositLiability: number;
  netProfit: number;
}

export interface CustomerCylinderBalance {
  id: string;
  customerId: string;
  customerName: string;
  brandId: string;
  brandName: string;
  cylinderSizeId: string;
  cylinderSizeName: string;
  received: number;
  returned: number;
  outstanding: number;
}

export interface CustomerGasLedger {
  customerId: string;
  customerName: string;
  totalGasPurchases: number;
  totalCylinderPurchases: number;
  totalPayments: number;
  outstandingBalance: number;
  totalDeposits: number;
  recentTransactions: LedgerEntry[];
}

export interface LedgerEntry {
  date: string;
  description: string;
  debit: number;
  credit: number;
  runningBalance: number;
}

export interface CustomerCreditSummary {
  customerId: string;
  customerName: string;
  creditLimit: number;
  totalPurchases: number;
  totalPayments: number;
  outstandingBalance: number;
  creditUtilization: number;
  isOverCredit: boolean;
}

export interface CreditAgingReport {
  entries: CreditAgingEntry[];
  totalCurrent: number;
  totalDays30: number;
  totalDays60: number;
  totalDays90: number;
  totalDaysOver90: number;
}

export interface CreditAgingEntry {
  customerName: string;
  current: number;
  days30: number;
  days60: number;
  days90: number;
  daysOver90: number;
}

export interface OutstandingCylinder {
  customerId: string;
  customerName: string;
  brandId: string;
  brandName: string;
  cylinderSizeId: string;
  cylinderSizeName: string;
  outstanding: number;
}

export interface DailySalesSummary {
  id: string;
  summaryDate: string;
  vehicleLoadingId: string;
  truckId: string;
  truckName: string;
  driverId: string;
  driverName: string;
  salesmanId: string;
  salesmanName: string;
  totalSales: number;
  cashSales: number;
  creditSales: number;
  packagesSold: number;
  refillsSold: number;
  emptyCylindersSold: number;
  accessoriesSold: number;
  paymentsCollected: number;
  dueCreated: number;
  cylinderBalance: number;
  outstandingCylinders: number;
  stockReturned: number;
  notes: string;
}

export interface BrandInventory {
  brandName: string;
  emptyCylinderCount: number;
  filledCylinderCount: number;
  totalQuantity: number;
}

export interface LowStockAlert {
  productName: string;
  brandName: string;
  warehouseName: string;
  currentStock: number;
  minimumStock: number;
  deficit: number;
}

export interface VehicleLoadingReport {
  date: string;
  truckName: string;
  driverName: string;
  salesmanName: string;
  warehouseName: string;
  status: string;
  itemCount: number;
}

export interface CustomerSalesSummary {
  customerName: string;
  orderCount: number;
  totalPurchases: number;
  totalPayments: number;
  outstanding: number;
}

export interface ProductTypeSales {
  productType: string;
  quantitySold: number;
  totalRevenue: number;
}

export interface RouteSales {
  routeName: string;
  orderCount: number;
  totalSales: number;
  totalPayments: number;
  outstanding: number;
}

export interface BrandSales {
  brandName: string;
  quantitySold: number;
  totalRevenue: number;
}

export interface VehicleReconciliation {
  date: string;
  truckName: string;
  driverName: string;
  salesmanName: string;
  totalLoaded: number;
  totalSold: number;
  totalReturned: number;
  totalDamaged: number;
  variance: number;
  cashCollected: number;
  creditSales: number;
}

export interface DriverProductivity {
  driverName: string;
  tripCount: number;
  totalFuelCost: number;
  totalSettlement: number;
  netPayout: number;
}

export interface SalesmanProductivity {
  salesmanName: string;
  orderCount: number;
  totalSales: number;
  totalCollection: number;
  totalCommission: number;
}

export interface CylinderMovement {
  date: string;
  productName: string;
  type: string;
  quantity: number;
  fromWarehouse: string;
  toWarehouse: string;
  reference: string;
}

export interface TransportCompany {
  id: string;
  name: string;
  contactPerson: string;
  phone: string;
  email: string;
  address: string;
  isActive: boolean;
}

export interface CylinderSizeSales {
  brandName: string;
  cylinderSizeName: string;
  weightKg: number;
  quantitySold: number;
  totalRevenue: number;
}

export interface AdvanceRefillReport {
  customerName: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalAmount: number;
  orderDate: string;
  orderNumber: string;
}

export interface RefillHistory {
  customerName: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalAmount: number;
  orderDate: string;
  orderNumber: string;
  status: string;
}

export interface VehicleSales {
  truckName: string;
  driverName: string;
  salesmanName: string;
  orderCount: number;
  totalSales: number;
  totalPayments: number;
}

export interface CreditPurchase {
  orderNumber: string;
  supplierName: string;
  orderDate: string;
  totalAmount: number;
  amountPaid: number;
  outstanding: number;
}

export interface CashFlowEntry {
  date: string;
  cashIn: number;
  cashOut: number;
  netCashFlow: number;
}

export interface PnLCategory {
  category: string;
  amount: number;
  isIncome: boolean;
}

export interface VehicleClosing {
  id: string;
  vehicleLoadingId: string;
  closingDate: string;
  cashCollected: number;
  creditSales: number;
  outstandingAmount: number;
  cylinderExchanges: number;
  returnedEmptyCylinders: number;
  damagedCount: number;
  leakageCount: number;
  variance: number;
  notes: string;
  items: VehicleClosingItem[];
}

export interface VehicleClosingItem {
  id: string;
  productId: string;
  productName: string;
  loadedQuantity: number;
  soldQuantity: number;
  returnedQuantity: number;
  damagedQuantity: number;
}
