namespace LpgErp.Application.Features.CustomerAccount.DTOs;

/// <summary>Everything owed, held and deposited for one customer, from one calculation.</summary>
public class CustomerAccountSummaryDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }

    /// <summary>Value of goods committed to the customer — confirmed and delivered orders, after discount.</summary>
    public decimal TotalBilled { get; set; }

    /// <summary>Money received against those goods. Deposits are not included; they are refundable.</summary>
    public decimal TotalPaid { get; set; }

    public decimal OutstandingDue { get; set; }

    /// <summary>Refundable cylinder security currently held, net of refunds. A liability, not revenue.</summary>
    public decimal DepositHeld { get; set; }

    /// <summary>Cylinders the customer physically holds and has not returned.</summary>
    public int CylindersHeld { get; set; }

    public decimal CreditUtilization { get; set; }
    public bool IsOverCredit { get; set; }
}

public enum StatementLineKind
{
    Sale = 0,
    Payment = 1,
    Deposit = 2,
    DepositRefund = 3,
    ExchangeCharge = 4,
}

public class StatementLineDto
{
    public DateTime Date { get; set; }
    public StatementLineKind Kind { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>Increases what the customer owes.</summary>
    public decimal Debit { get; set; }

    /// <summary>Reduces what the customer owes.</summary>
    public decimal Credit { get; set; }

    /// <summary>Cash that moved, shown for deposit lines that sit outside the goods balance.</summary>
    public decimal Amount { get; set; }

    public decimal RunningBalance { get; set; }
    public string? Reference { get; set; }
}

public class CylinderHoldingDto
{
    public string BrandName { get; set; } = string.Empty;
    public string CylinderSizeName { get; set; } = string.Empty;
    public int Held { get; set; }
}

public class CustomerStatementDto
{
    public CustomerAccountSummaryDto Summary { get; set; } = new();
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }

    /// <summary>Newest first for display; running balances were computed oldest first.</summary>
    public List<StatementLineDto> Lines { get; set; } = [];

    public List<CylinderHoldingDto> Cylinders { get; set; } = [];
}

/// <summary>One of the customer's orders with how much of it is still unpaid.</summary>
public class CustomerOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int Status { get; set; }
    public bool IsCreditSale { get; set; }
    public decimal NetAmount { get; set; }
    public decimal Paid { get; set; }
    public decimal Outstanding { get; set; }
    public bool IsOverdue { get; set; }
}

public class CustomerAgingEntryDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Current { get; set; }
    public decimal Days30 { get; set; }
    public decimal Days60 { get; set; }
    public decimal Days90 { get; set; }
    public decimal DaysOver90 { get; set; }
    public decimal Total => Current + Days30 + Days60 + Days90 + DaysOver90;
}

public class CustomerAgingDto
{
    public List<CustomerAgingEntryDto> Entries { get; set; } = [];
    public decimal TotalCurrent { get; set; }
    public decimal TotalDays30 { get; set; }
    public decimal TotalDays60 { get; set; }
    public decimal TotalDays90 { get; set; }
    public decimal TotalDaysOver90 { get; set; }
}
