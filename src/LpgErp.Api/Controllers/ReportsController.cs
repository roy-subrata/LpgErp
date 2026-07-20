using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Reports;
using LpgErp.Application.Features.Reports.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryReport([FromQuery] Guid? warehouseId, CancellationToken cancellationToken)
    {
        var result = await _service.GetInventoryReportAsync(warehouseId, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<InventoryReportDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<InventoryReportDto>>.Ok(result.Data!));
    }

    [HttpGet("inventory/brands")]
    public async Task<IActionResult> GetBrandInventory(CancellationToken cancellationToken)
    {
        var result = await _service.GetBrandInventoryAsync(cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<BrandInventoryDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<BrandInventoryDto>>.Ok(result.Data!));
    }

    [HttpGet("inventory/low-stock")]
    public async Task<IActionResult> GetLowStockAlerts(CancellationToken cancellationToken)
    {
        var result = await _service.GetLowStockAlertsAsync(cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<LowStockAlertDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<LowStockAlertDto>>.Ok(result.Data!));
    }

    [HttpGet("inventory/movements")]
    public async Task<IActionResult> GetCylinderMovements([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetCylinderMovementHistoryAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<CylinderMovementDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<CylinderMovementDto>>.Ok(result.Data!));
    }

    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetSalesReportAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<SalesReportDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<SalesReportDto>>.Ok(result.Data!));
    }

    [HttpGet("sales/by-customer")]
    public async Task<IActionResult> GetSalesByCustomer([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetSalesByCustomerAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<CustomerSalesSummaryDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<CustomerSalesSummaryDto>>.Ok(result.Data!));
    }

    [HttpGet("sales/by-product-type")]
    public async Task<IActionResult> GetProductTypeSales([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetProductTypeSalesAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<ProductTypeSalesDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<ProductTypeSalesDto>>.Ok(result.Data!));
    }

    [HttpGet("purchases")]
    public async Task<IActionResult> GetPurchaseReport([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetPurchaseReportAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<PurchaseReportDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<PurchaseReportDto>>.Ok(result.Data!));
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomerReport(CancellationToken cancellationToken)
    {
        var result = await _service.GetCustomerReportAsync(cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<CustomerReportDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<CustomerReportDto>>.Ok(result.Data!));
    }

    [HttpGet("vehicles")]
    public async Task<IActionResult> GetVehicleLoadingReport([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetVehicleLoadingReportAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<VehicleLoadingReportDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<VehicleLoadingReportDto>>.Ok(result.Data!));
    }

    [HttpGet("financial")]
    public async Task<IActionResult> GetFinancialReport([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetFinancialReportAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<FinancialReportDto>.Fail(result.Error!));
        return Ok(ApiResponse<FinancialReportDto>.Ok(result.Data!));
    }

    [HttpGet("sales/by-route")]
    public async Task<IActionResult> GetSalesByRoute([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetSalesByRouteAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<RouteSalesDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<RouteSalesDto>>.Ok(result.Data!));
    }

    [HttpGet("sales/by-brand")]
    public async Task<IActionResult> GetBrandWiseSales([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetBrandWiseSalesAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<BrandSalesDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<BrandSalesDto>>.Ok(result.Data!));
    }

    [HttpGet("vehicles/reconciliation")]
    public async Task<IActionResult> GetVehicleReconciliation([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetVehicleReconciliationAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<VehicleReconciliationDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<VehicleReconciliationDto>>.Ok(result.Data!));
    }

    [HttpGet("drivers/productivity")]
    public async Task<IActionResult> GetDriverProductivity([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetDriverProductivityAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<DriverProductivityDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<DriverProductivityDto>>.Ok(result.Data!));
    }

    [HttpGet("salesmen/productivity")]
    public async Task<IActionResult> GetSalesmanProductivity([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetSalesmanProductivityAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<SalesmanProductivityDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<SalesmanProductivityDto>>.Ok(result.Data!));
    }

    [HttpGet("sales/by-cylinder-size")]
    public async Task<IActionResult> GetCylinderSizeSales([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetCylinderSizeSalesAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<CylinderSizeSalesDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<CylinderSizeSalesDto>>.Ok(result.Data!));
    }

    [HttpGet("advance-refills")]
    public async Task<IActionResult> GetAdvanceRefillReport(CancellationToken cancellationToken)
    {
        var result = await _service.GetAdvanceRefillReportAsync(cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<AdvanceRefillReportDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<AdvanceRefillReportDto>>.Ok(result.Data!));
    }

    [HttpGet("customers/{customerId}/refill-history")]
    public async Task<IActionResult> GetRefillHistory(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.GetRefillHistoryAsync(customerId, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<RefillHistoryDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<RefillHistoryDto>>.Ok(result.Data!));
    }

    [HttpGet("vehicles/sales")]
    public async Task<IActionResult> GetSalesByVehicle([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetSalesByVehicleAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<VehicleSalesDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<VehicleSalesDto>>.Ok(result.Data!));
    }

    [HttpGet("purchases/credit")]
    public async Task<IActionResult> GetCreditPurchases(CancellationToken cancellationToken)
    {
        var result = await _service.GetCreditPurchasesAsync(cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<CreditPurchaseDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<CreditPurchaseDto>>.Ok(result.Data!));
    }

    [HttpGet("financial/cashflow")]
    public async Task<IActionResult> GetCashFlow([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetCashFlowAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<CashFlowEntryDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<CashFlowEntryDto>>.Ok(result.Data!));
    }

    [HttpGet("financial/pnl")]
    public async Task<IActionResult> GetPnLBreakdown([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetPnLBreakdownAsync(from, to, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<PnLCategoryDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<PnLCategoryDto>>.Ok(result.Data!));
    }
}
