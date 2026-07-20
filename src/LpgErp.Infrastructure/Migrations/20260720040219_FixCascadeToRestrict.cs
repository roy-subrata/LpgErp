using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LpgErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeToRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerNotifications_Customers_CustomerId",
                table: "CustomerNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderDeposits_Customers_CustomerId",
                table: "CylinderDeposits");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderDeposits_CylinderSizes_CylinderSizeId",
                table: "CylinderDeposits");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderExchanges_Brands_IncomingBrandId",
                table: "CylinderExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderExchanges_Brands_OutgoingBrandId",
                table: "CylinderExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderExchanges_Customers_CustomerId",
                table: "CylinderExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderExchanges_CylinderSizes_IncomingCylinderSizeId",
                table: "CylinderExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderExchanges_CylinderSizes_OutgoingCylinderSizeId",
                table: "CylinderExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderExchanges_SalesOrders_SalesOrderId",
                table: "CylinderExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_Cylinders_Brands_BrandId",
                table: "Cylinders");

            migrationBuilder.DropForeignKey(
                name: "FK_Cylinders_CylinderSizes_CylinderSizeId",
                table: "Cylinders");

            migrationBuilder.DropForeignKey(
                name: "FK_Cylinders_Warehouses_CurrentWarehouseId",
                table: "Cylinders");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderSizes_Brands_BrandId",
                table: "CylinderSizes");

            migrationBuilder.DropForeignKey(
                name: "FK_DailySalesSummaries_Drivers_DriverId",
                table: "DailySalesSummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_DailySalesSummaries_Salesmen_SalesmanId",
                table: "DailySalesSummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_DailySalesSummaries_Trucks_TruckId",
                table: "DailySalesSummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_DailySalesSummaries_VehicleLoadings_VehicleLoadingId",
                table: "DailySalesSummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverSettlements_Drivers_DriverId",
                table: "DriverSettlements");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverSettlements_VehicleLoadings_VehicleLoadingId",
                table: "DriverSettlements");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PurchaseOrders_PurchaseOrderId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_SalesOrders_SalesOrderId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_CylinderSizes_CylinderSizeId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_Products_ProductId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Suppliers_SupplierId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_TransportCompanies_TransportCompanyId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Warehouses_WarehouseId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesmanSettlements_Salesmen_SalesmanId",
                table: "SalesmanSettlements");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderItems_Products_ProductId",
                table: "SalesOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderItems_SalesOrders_SalesOrderId",
                table: "SalesOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Customers_CustomerId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Routes_RouteId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_TransportCompanies_TransportCompanyId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Warehouses_WarehouseId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_StockLevels_Products_ProductId",
                table: "StockLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_StockLevels_Warehouses_WarehouseId",
                table: "StockLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Products_ProductId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_PurchaseOrders_PurchaseOrderId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_SalesOrders_SalesOrderId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Warehouses_FromWarehouseId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Warehouses_ToWarehouseId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleClosingItems_Products_ProductId",
                table: "VehicleClosingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleClosingItems_VehicleClosings_VehicleClosingId",
                table: "VehicleClosingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleClosings_VehicleLoadings_VehicleLoadingId",
                table: "VehicleClosings");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLoadingItems_Products_ProductId",
                table: "VehicleLoadingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLoadingItems_VehicleLoadings_VehicleLoadingId",
                table: "VehicleLoadingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLoadings_Drivers_DriverId",
                table: "VehicleLoadings");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLoadings_Routes_RouteId",
                table: "VehicleLoadings");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLoadings_Salesmen_SalesmanId",
                table: "VehicleLoadings");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLoadings_Trucks_TruckId",
                table: "VehicleLoadings");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLoadings_Warehouses_WarehouseId",
                table: "VehicleLoadings");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerNotifications_Customers_CustomerId",
                table: "CustomerNotifications",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderDeposits_Customers_CustomerId",
                table: "CylinderDeposits",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderDeposits_CylinderSizes_CylinderSizeId",
                table: "CylinderDeposits",
                column: "CylinderSizeId",
                principalTable: "CylinderSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderExchanges_Brands_IncomingBrandId",
                table: "CylinderExchanges",
                column: "IncomingBrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderExchanges_Brands_OutgoingBrandId",
                table: "CylinderExchanges",
                column: "OutgoingBrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderExchanges_Customers_CustomerId",
                table: "CylinderExchanges",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderExchanges_CylinderSizes_IncomingCylinderSizeId",
                table: "CylinderExchanges",
                column: "IncomingCylinderSizeId",
                principalTable: "CylinderSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderExchanges_CylinderSizes_OutgoingCylinderSizeId",
                table: "CylinderExchanges",
                column: "OutgoingCylinderSizeId",
                principalTable: "CylinderSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderExchanges_SalesOrders_SalesOrderId",
                table: "CylinderExchanges",
                column: "SalesOrderId",
                principalTable: "SalesOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cylinders_Brands_BrandId",
                table: "Cylinders",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cylinders_CylinderSizes_CylinderSizeId",
                table: "Cylinders",
                column: "CylinderSizeId",
                principalTable: "CylinderSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cylinders_Warehouses_CurrentWarehouseId",
                table: "Cylinders",
                column: "CurrentWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderSizes_Brands_BrandId",
                table: "CylinderSizes",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailySalesSummaries_Drivers_DriverId",
                table: "DailySalesSummaries",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailySalesSummaries_Salesmen_SalesmanId",
                table: "DailySalesSummaries",
                column: "SalesmanId",
                principalTable: "Salesmen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailySalesSummaries_Trucks_TruckId",
                table: "DailySalesSummaries",
                column: "TruckId",
                principalTable: "Trucks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailySalesSummaries_VehicleLoadings_VehicleLoadingId",
                table: "DailySalesSummaries",
                column: "VehicleLoadingId",
                principalTable: "VehicleLoadings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverSettlements_Drivers_DriverId",
                table: "DriverSettlements",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverSettlements_VehicleLoadings_VehicleLoadingId",
                table: "DriverSettlements",
                column: "VehicleLoadingId",
                principalTable: "VehicleLoadings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PurchaseOrders_PurchaseOrderId",
                table: "Payments",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_SalesOrders_SalesOrderId",
                table: "Payments",
                column: "SalesOrderId",
                principalTable: "SalesOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_CylinderSizes_CylinderSizeId",
                table: "Products",
                column: "CylinderSizeId",
                principalTable: "CylinderSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_Products_ProductId",
                table: "PurchaseOrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseOrderItems",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Suppliers_SupplierId",
                table: "PurchaseOrders",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_TransportCompanies_TransportCompanyId",
                table: "PurchaseOrders",
                column: "TransportCompanyId",
                principalTable: "TransportCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Warehouses_WarehouseId",
                table: "PurchaseOrders",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesmanSettlements_Salesmen_SalesmanId",
                table: "SalesmanSettlements",
                column: "SalesmanId",
                principalTable: "Salesmen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderItems_Products_ProductId",
                table: "SalesOrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderItems_SalesOrders_SalesOrderId",
                table: "SalesOrderItems",
                column: "SalesOrderId",
                principalTable: "SalesOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Customers_CustomerId",
                table: "SalesOrders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Routes_RouteId",
                table: "SalesOrders",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_TransportCompanies_TransportCompanyId",
                table: "SalesOrders",
                column: "TransportCompanyId",
                principalTable: "TransportCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Warehouses_WarehouseId",
                table: "SalesOrders",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockLevels_Products_ProductId",
                table: "StockLevels",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockLevels_Warehouses_WarehouseId",
                table: "StockLevels",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Products_ProductId",
                table: "StockMovements",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_PurchaseOrders_PurchaseOrderId",
                table: "StockMovements",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_SalesOrders_SalesOrderId",
                table: "StockMovements",
                column: "SalesOrderId",
                principalTable: "SalesOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Warehouses_FromWarehouseId",
                table: "StockMovements",
                column: "FromWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Warehouses_ToWarehouseId",
                table: "StockMovements",
                column: "ToWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleClosingItems_Products_ProductId",
                table: "VehicleClosingItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleClosingItems_VehicleClosings_VehicleClosingId",
                table: "VehicleClosingItems",
                column: "VehicleClosingId",
                principalTable: "VehicleClosings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleClosings_VehicleLoadings_VehicleLoadingId",
                table: "VehicleClosings",
                column: "VehicleLoadingId",
                principalTable: "VehicleLoadings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLoadingItems_Products_ProductId",
                table: "VehicleLoadingItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLoadingItems_VehicleLoadings_VehicleLoadingId",
                table: "VehicleLoadingItems",
                column: "VehicleLoadingId",
                principalTable: "VehicleLoadings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLoadings_Drivers_DriverId",
                table: "VehicleLoadings",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLoadings_Routes_RouteId",
                table: "VehicleLoadings",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLoadings_Salesmen_SalesmanId",
                table: "VehicleLoadings",
                column: "SalesmanId",
                principalTable: "Salesmen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLoadings_Trucks_TruckId",
                table: "VehicleLoadings",
                column: "TruckId",
                principalTable: "Trucks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLoadings_Warehouses_WarehouseId",
                table: "VehicleLoadings",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerNotifications_Customers_CustomerId",
                table: "CustomerNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderDeposits_Customers_CustomerId",
                table: "CylinderDeposits");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderDeposits_CylinderSizes_CylinderSizeId",
                table: "CylinderDeposits");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderExchanges_Brands_IncomingBrandId",
                table: "CylinderExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderExchanges_Brands_OutgoingBrandId",
                table: "CylinderExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderExchanges_Customers_CustomerId",
                table: "CylinderExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderExchanges_CylinderSizes_IncomingCylinderSizeId",
                table: "CylinderExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderExchanges_CylinderSizes_OutgoingCylinderSizeId",
                table: "CylinderExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderExchanges_SalesOrders_SalesOrderId",
                table: "CylinderExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_Cylinders_Brands_BrandId",
                table: "Cylinders");

            migrationBuilder.DropForeignKey(
                name: "FK_Cylinders_CylinderSizes_CylinderSizeId",
                table: "Cylinders");

            migrationBuilder.DropForeignKey(
                name: "FK_Cylinders_Warehouses_CurrentWarehouseId",
                table: "Cylinders");

            migrationBuilder.DropForeignKey(
                name: "FK_CylinderSizes_Brands_BrandId",
                table: "CylinderSizes");

            migrationBuilder.DropForeignKey(
                name: "FK_DailySalesSummaries_Drivers_DriverId",
                table: "DailySalesSummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_DailySalesSummaries_Salesmen_SalesmanId",
                table: "DailySalesSummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_DailySalesSummaries_Trucks_TruckId",
                table: "DailySalesSummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_DailySalesSummaries_VehicleLoadings_VehicleLoadingId",
                table: "DailySalesSummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverSettlements_Drivers_DriverId",
                table: "DriverSettlements");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverSettlements_VehicleLoadings_VehicleLoadingId",
                table: "DriverSettlements");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PurchaseOrders_PurchaseOrderId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_SalesOrders_SalesOrderId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_CylinderSizes_CylinderSizeId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_Products_ProductId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Suppliers_SupplierId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_TransportCompanies_TransportCompanyId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Warehouses_WarehouseId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesmanSettlements_Salesmen_SalesmanId",
                table: "SalesmanSettlements");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderItems_Products_ProductId",
                table: "SalesOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderItems_SalesOrders_SalesOrderId",
                table: "SalesOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Customers_CustomerId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Routes_RouteId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_TransportCompanies_TransportCompanyId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Warehouses_WarehouseId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_StockLevels_Products_ProductId",
                table: "StockLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_StockLevels_Warehouses_WarehouseId",
                table: "StockLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Products_ProductId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_PurchaseOrders_PurchaseOrderId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_SalesOrders_SalesOrderId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Warehouses_FromWarehouseId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Warehouses_ToWarehouseId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleClosingItems_Products_ProductId",
                table: "VehicleClosingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleClosingItems_VehicleClosings_VehicleClosingId",
                table: "VehicleClosingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleClosings_VehicleLoadings_VehicleLoadingId",
                table: "VehicleClosings");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLoadingItems_Products_ProductId",
                table: "VehicleLoadingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLoadingItems_VehicleLoadings_VehicleLoadingId",
                table: "VehicleLoadingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLoadings_Drivers_DriverId",
                table: "VehicleLoadings");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLoadings_Routes_RouteId",
                table: "VehicleLoadings");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLoadings_Salesmen_SalesmanId",
                table: "VehicleLoadings");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLoadings_Trucks_TruckId",
                table: "VehicleLoadings");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLoadings_Warehouses_WarehouseId",
                table: "VehicleLoadings");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerNotifications_Customers_CustomerId",
                table: "CustomerNotifications",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderDeposits_Customers_CustomerId",
                table: "CylinderDeposits",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderDeposits_CylinderSizes_CylinderSizeId",
                table: "CylinderDeposits",
                column: "CylinderSizeId",
                principalTable: "CylinderSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderExchanges_Brands_IncomingBrandId",
                table: "CylinderExchanges",
                column: "IncomingBrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderExchanges_Brands_OutgoingBrandId",
                table: "CylinderExchanges",
                column: "OutgoingBrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderExchanges_Customers_CustomerId",
                table: "CylinderExchanges",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderExchanges_CylinderSizes_IncomingCylinderSizeId",
                table: "CylinderExchanges",
                column: "IncomingCylinderSizeId",
                principalTable: "CylinderSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderExchanges_CylinderSizes_OutgoingCylinderSizeId",
                table: "CylinderExchanges",
                column: "OutgoingCylinderSizeId",
                principalTable: "CylinderSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderExchanges_SalesOrders_SalesOrderId",
                table: "CylinderExchanges",
                column: "SalesOrderId",
                principalTable: "SalesOrders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cylinders_Brands_BrandId",
                table: "Cylinders",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cylinders_CylinderSizes_CylinderSizeId",
                table: "Cylinders",
                column: "CylinderSizeId",
                principalTable: "CylinderSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cylinders_Warehouses_CurrentWarehouseId",
                table: "Cylinders",
                column: "CurrentWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderSizes_Brands_BrandId",
                table: "CylinderSizes",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DailySalesSummaries_Drivers_DriverId",
                table: "DailySalesSummaries",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DailySalesSummaries_Salesmen_SalesmanId",
                table: "DailySalesSummaries",
                column: "SalesmanId",
                principalTable: "Salesmen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DailySalesSummaries_Trucks_TruckId",
                table: "DailySalesSummaries",
                column: "TruckId",
                principalTable: "Trucks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DailySalesSummaries_VehicleLoadings_VehicleLoadingId",
                table: "DailySalesSummaries",
                column: "VehicleLoadingId",
                principalTable: "VehicleLoadings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverSettlements_Drivers_DriverId",
                table: "DriverSettlements",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverSettlements_VehicleLoadings_VehicleLoadingId",
                table: "DriverSettlements",
                column: "VehicleLoadingId",
                principalTable: "VehicleLoadings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PurchaseOrders_PurchaseOrderId",
                table: "Payments",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_SalesOrders_SalesOrderId",
                table: "Payments",
                column: "SalesOrderId",
                principalTable: "SalesOrders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_CylinderSizes_CylinderSizeId",
                table: "Products",
                column: "CylinderSizeId",
                principalTable: "CylinderSizes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_Products_ProductId",
                table: "PurchaseOrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseOrderItems",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Suppliers_SupplierId",
                table: "PurchaseOrders",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_TransportCompanies_TransportCompanyId",
                table: "PurchaseOrders",
                column: "TransportCompanyId",
                principalTable: "TransportCompanies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Warehouses_WarehouseId",
                table: "PurchaseOrders",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesmanSettlements_Salesmen_SalesmanId",
                table: "SalesmanSettlements",
                column: "SalesmanId",
                principalTable: "Salesmen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderItems_Products_ProductId",
                table: "SalesOrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderItems_SalesOrders_SalesOrderId",
                table: "SalesOrderItems",
                column: "SalesOrderId",
                principalTable: "SalesOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Customers_CustomerId",
                table: "SalesOrders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Routes_RouteId",
                table: "SalesOrders",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_TransportCompanies_TransportCompanyId",
                table: "SalesOrders",
                column: "TransportCompanyId",
                principalTable: "TransportCompanies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Warehouses_WarehouseId",
                table: "SalesOrders",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockLevels_Products_ProductId",
                table: "StockLevels",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockLevels_Warehouses_WarehouseId",
                table: "StockLevels",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Products_ProductId",
                table: "StockMovements",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_PurchaseOrders_PurchaseOrderId",
                table: "StockMovements",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_SalesOrders_SalesOrderId",
                table: "StockMovements",
                column: "SalesOrderId",
                principalTable: "SalesOrders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Warehouses_FromWarehouseId",
                table: "StockMovements",
                column: "FromWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Warehouses_ToWarehouseId",
                table: "StockMovements",
                column: "ToWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleClosingItems_Products_ProductId",
                table: "VehicleClosingItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleClosingItems_VehicleClosings_VehicleClosingId",
                table: "VehicleClosingItems",
                column: "VehicleClosingId",
                principalTable: "VehicleClosings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleClosings_VehicleLoadings_VehicleLoadingId",
                table: "VehicleClosings",
                column: "VehicleLoadingId",
                principalTable: "VehicleLoadings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLoadingItems_Products_ProductId",
                table: "VehicleLoadingItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLoadingItems_VehicleLoadings_VehicleLoadingId",
                table: "VehicleLoadingItems",
                column: "VehicleLoadingId",
                principalTable: "VehicleLoadings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLoadings_Drivers_DriverId",
                table: "VehicleLoadings",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLoadings_Routes_RouteId",
                table: "VehicleLoadings",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLoadings_Salesmen_SalesmanId",
                table: "VehicleLoadings",
                column: "SalesmanId",
                principalTable: "Salesmen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLoadings_Trucks_TruckId",
                table: "VehicleLoadings",
                column: "TruckId",
                principalTable: "Trucks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLoadings_Warehouses_WarehouseId",
                table: "VehicleLoadings",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
