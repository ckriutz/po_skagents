using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SKPurchaseOrderClient.Plugins;

/// <summary>
/// Example plugin showing how to add function calling capabilities to the IntakeAgent.
/// These functions can be automatically called by the AI when processing purchase orders.
/// </summary>
public class PurchaseOrderPlugin
{
    [KernelFunction]
    [Description("Validates if a purchase order number follows the correct format")]
    public string ValidatePONumber(string poNumber)
    {
        // Example validation: PO numbers should start with a prefix and contain a number
        if (string.IsNullOrWhiteSpace(poNumber))
        {
            return "Invalid: PO number is empty";
        }
        
        if (poNumber.Contains("-PO-"))
        {
            return $"Valid: PO number {poNumber} follows the expected format";
        }
        
        return $"Warning: PO number {poNumber} does not follow standard format (expected: PREFIX-PO-NUMBER)";
    }

    [KernelFunction]
    [Description("Calculates the expected tax based on subtotal and validates against provided tax")]
    public string ValidateTax(double subTotal, double tax, string department)
    {
        // Example tax rates by department
        double taxRate = department.ToUpper() switch
        {
            "HR" => 0.065,
            "IT" => 0.070,
            "MKT" or "MARKETING" => 0.072,
            _ => 0.07
        };
        
        double expectedTax = Math.Round(subTotal * taxRate, 2);
        double difference = Math.Abs(tax - expectedTax);
        
        if (difference < 0.01)
        {
            return $"Tax validated: ${tax} is correct for {department} department (rate: {taxRate:P})";
        }
        
        return $"Tax discrepancy: Expected ${expectedTax} ({taxRate:P}), but found ${tax}. Difference: ${difference}";
    }

    [KernelFunction]
    [Description("Looks up supplier information and returns whether they are approved")]
    public string LookupSupplier(string supplierName)
    {
        // Example supplier database (in real world, this would query a database)
        var approvedSuppliers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Stuff We All Get Inc.",
            "Swag Depot",
            "Office Supplies Co",
            "Tech Hardware LLC"
        };
        
        if (approvedSuppliers.Contains(supplierName))
        {
            return $"✓ {supplierName} is an approved supplier";
        }
        
        return $"⚠ Warning: {supplierName} is not in the approved supplier list";
    }

    [KernelFunction]
    [Description("Validates that the grand total equals subtotal plus tax")]
    public string ValidateGrandTotal(double subTotal, double tax, double grandTotal)
    {
        double expectedTotal = Math.Round(subTotal + tax, 2);
        double difference = Math.Abs(grandTotal - expectedTotal);
        
        if (difference < 0.01)
        {
            return $"Grand total validated: ${grandTotal} = ${subTotal} + ${tax}";
        }
        
        return $"Grand total error: Expected ${expectedTotal}, but found ${grandTotal}. Difference: ${difference}";
    }
}
