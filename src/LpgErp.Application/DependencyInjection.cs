using System.Reflection;
using FluentValidation;
using LpgErp.Application.Features.Brands;
using LpgErp.Application.Features.Customers;
using LpgErp.Application.Features.Products;
using LpgErp.Application.Features.Warehouses;
using LpgErp.Application.Features.Suppliers;
using LpgErp.Application.Features.Cylinders;
using LpgErp.Application.Features.CylinderSizes;
using LpgErp.Application.Features.Trucks;
using LpgErp.Application.Features.Drivers;
using LpgErp.Application.Features.Salesmen;
using LpgErp.Application.Features.PurchaseOrders;
using LpgErp.Application.Features.SalesOrders;
using LpgErp.Application.Features.Payments;
using LpgErp.Application.Features.Routes;
using LpgErp.Application.Features.VehicleLoadings;
using LpgErp.Application.Features.DriverSettlements;
using LpgErp.Application.Features.SalesmanSettlements;
using LpgErp.Application.Features.CylinderDeposits;
using LpgErp.Application.Features.CylinderExchanges;
using LpgErp.Application.Features.CustomerNotifications;
using LpgErp.Application.Features.StockTransfer;
using LpgErp.Application.Features.Reports;
using LpgErp.Application.Features.DailySalesSummaries;
using LpgErp.Application.Features.AdvanceRefills;
using LpgErp.Application.Features.CustomerCylinderLedger;
using LpgErp.Application.Features.CustomerGasLedger;
using LpgErp.Application.Features.CustomerCredit;
using LpgErp.Application.Features.TransportCompanies;
using LpgErp.Application.Features.VehicleClosings;
using Microsoft.Extensions.DependencyInjection;

namespace LpgErp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ICylinderService, CylinderService>();
        services.AddScoped<ICylinderSizeService, CylinderSizeService>();
        services.AddScoped<ITruckService, TruckService>();
        services.AddScoped<IDriverService, DriverService>();
        services.AddScoped<ISalesmanService, SalesmanService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IRouteService, RouteService>();
        services.AddScoped<IVehicleLoadingService, VehicleLoadingService>();
        services.AddScoped<IDriverSettlementService, DriverSettlementService>();
        services.AddScoped<ISalesmanSettlementService, SalesmanSettlementService>();
        services.AddScoped<ICylinderDepositService, CylinderDepositService>();
        services.AddScoped<ICylinderExchangeService, CylinderExchangeService>();
        services.AddScoped<ICustomerNotificationService, CustomerNotificationService>();
        services.AddScoped<IStockTransferService, StockTransferService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDailySalesSummaryService, DailySalesSummaryService>();
        services.AddScoped<IAdvanceRefillService, AdvanceRefillService>();
        services.AddScoped<ICustomerCylinderLedgerService, CustomerCylinderLedgerService>();
        services.AddScoped<ICustomerGasLedgerService, CustomerGasLedgerService>();
        services.AddScoped<ICustomerCreditService, CustomerCreditService>();
        services.AddScoped<ITransportCompanyService, TransportCompanyService>();
        services.AddScoped<IVehicleClosingService, VehicleClosingService>();

        return services;
    }
}
