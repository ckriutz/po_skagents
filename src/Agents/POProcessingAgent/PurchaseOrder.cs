using System;
using System.Collections.Generic;
using System.Linq;

namespace POProcessingAgent.Models;

public class PurchaseOrder
{
    // Supplier fields.
    public string? SupplierName { get; set; }
    public string? SupplierAddressLine1 { get; set; }
    public string? SupplierAddressLine2 { get; set; }
    public string? SupplierCity { get; set; }
    public string? SupplierState { get; set; }
    public string? SupplierPostalCode { get; set; }
    public string? SupplierCountry { get; set; }

    // Line items table
    public List<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();

    // Purchase order metadata
    public string? PoNumber { get; set; }
    public string? CreatedBy { get; set; }
    public string? BuyerDepartment { get; set; }

    // Notes block
    public string? Notes { get; set; }

    // Tax information
    public decimal TaxRate { get; set; }

    // Computed totals
    public decimal SubTotal => Items?.Sum(i => i.LineTotal) ?? 0m;
    public decimal TaxAmount => Math.Round(SubTotal * TaxRate, 2);
    public decimal GrandTotal => Math.Round(SubTotal + TaxAmount, 2);

    // Keep Amount for compatibility; return grand total
    public decimal Amount
    {
        get => GrandTotal;
        set { /* retained for binary compatibility; ignored */ }
    }

    // Approval flow
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
}

public class PurchaseOrderItem
{
    // Table columns: Item Code, Description, Quantity, Unit Price, Line Total
    public string? ItemCode { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}