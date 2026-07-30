namespace LpgErp.Application.Features.SupplierAccount.DTOs;

/// <summary>Everything owed, paid and earned for one supplier, from one calculation.</summary>
public class SupplierAccountSummaryDto
{
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>Value of goods committed from the supplier — confirmed and beyond, net of commission and leakage credit.</summary>
    public decimal TotalPurchased { get; set; }

    /// <summary>Money paid out against those goods.</summary>
    public decimal TotalPaid { get; set; }

    public decimal OutstandingDue { get; set; }

    /// <summary>Commission earned but not yet drawn down against an order.</summary>
    public decimal CommissionBalance { get; set; }

    /// <summary>Commission earned in total, lifetime, whether or not it has since been applied.</summary>
    public decimal CommissionEarnedLifetime { get; set; }
}

public enum SupplierStatementLineKind
{
    Purchase = 0,
    Payment = 1,
}

public class SupplierStatementLineDto
{
    public DateTime Date { get; set; }
    public SupplierStatementLineKind Kind { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>Increases what is owed to the supplier.</summary>
    public decimal Debit { get; set; }

    /// <summary>Reduces what is owed to the supplier.</summary>
    public decimal Credit { get; set; }

    public decimal RunningBalance { get; set; }
    public string? Reference { get; set; }
}

public class SupplierStatementDto
{
    public SupplierAccountSummaryDto Summary { get; set; } = new();
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }

    /// <summary>Newest first for display; running balances were computed oldest first.</summary>
    public List<SupplierStatementLineDto> Lines { get; set; } = [];
}

/// <summary>One of the supplier's purchase orders with how much of it is still unpaid.</summary>
public class SupplierOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime? OrderDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal CommissionApplied { get; set; }
    public decimal NetPayable { get; set; }
    public decimal Paid { get; set; }
    public decimal Outstanding { get; set; }
    public bool IsOverdue { get; set; }
}
