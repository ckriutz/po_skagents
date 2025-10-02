using System;
using System.Collections.Generic;
using System.Linq;

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
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }

    // Approval flow
    public bool IsApproved { get; set; }
    public string? ApprovalReason { get; set; }
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