using System.Text.Json;
using Microsoft.SemanticKernel.Agents.A2A;
using POProcessingAgent.Models;
using POProcessingAgent.Services;

namespace POProcessingAgent;

/// <summary>
/// Purchase Order Processing Agent - Console Application
/// This demonstrates how to use Semantic Kernel A2A agents to process Purchase Orders
/// </summary>
class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new() 
    { 
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Purchase Order Processing Agent ===");
        Console.WriteLine("A Semantic Kernel A2A Agent for Purchase Order Processing");
        Console.WriteLine();

        try
        {
            // TODO: Configure and initialize the A2A agent
            // This requires proper configuration including API keys and agent endpoint
            Console.WriteLine("⚠️  Agent initialization requires configuration:");
            Console.WriteLine("   - A2A Agent URL");
            Console.WriteLine("   - HTTP Client configuration");
            Console.WriteLine("   - Agent card resolution");
            Console.WriteLine();

            // Demonstrate with sample data
            await DemonstratePurchaseOrderStructure();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Demonstrates the Purchase Order data structure and basic operations
    /// </summary>
    private static async Task DemonstratePurchaseOrderStructure()
    {
        Console.WriteLine("📋 Sample Purchase Order Structure:");
        Console.WriteLine();

        // Create a sample purchase order
        var samplePO = new PurchaseOrder
        {
            PoNumber = "PO-2025-001",
            CreatedBy = "John Smith",
            BuyerDepartment = "IT Department",
            SupplierName = "Tech Supply Co.",
            SupplierAddressLine1 = "123 Tech Street",
            SupplierCity = "Seattle",
            SupplierState = "WA",
            SupplierPostalCode = "98101",
            SupplierCountry = "USA",
            TaxRate = 0.0875m, // 8.75% tax rate
            Notes = "Urgent order for Q1 project setup",
            Items = new List<PurchaseOrderItem>
            {
                new PurchaseOrderItem
                {
                    ItemCode = "LAP001",
                    Description = "Business Laptop - Dell Latitude 7420",
                    Quantity = 5,
                    UnitPrice = 1299.99m,
                    LineTotal = 6499.95m
                },
                new PurchaseOrderItem
                {
                    ItemCode = "MON001", 
                    Description = "24\" Monitor - Dell UltraSharp U2422H",
                    Quantity = 5,
                    UnitPrice = 279.99m,
                    LineTotal = 1399.95m
                },
                new PurchaseOrderItem
                {
                    ItemCode = "KBM001",
                    Description = "Wireless Keyboard & Mouse Set",
                    Quantity = 5,
                    UnitPrice = 89.99m,
                    LineTotal = 449.95m
                }
            },
            IsApproved = false
        };

        // Display the purchase order
        var json = JsonSerializer.Serialize(samplePO, JsonOptions);
        Console.WriteLine(json);
        Console.WriteLine();

        // Display calculated totals
        Console.WriteLine("💰 Calculated Totals:");
        Console.WriteLine($"   Subtotal: ${samplePO.SubTotal:F2}");
        Console.WriteLine($"   Tax ({samplePO.TaxRate:P2}): ${samplePO.TaxAmount:F2}");
        Console.WriteLine($"   Grand Total: ${samplePO.GrandTotal:F2}");
        Console.WriteLine();

        Console.WriteLine("✅ Purchase Order structure is ready for AI processing!");
        Console.WriteLine("   Next steps:");
        Console.WriteLine("   1. Configure A2A agent endpoint and credentials");
        Console.WriteLine("   2. Initialize POProcessingAgentService with configured agent");
        Console.WriteLine("   3. Use ValidatePurchaseOrderAsync() for validation");
        Console.WriteLine("   4. Use SummarizePurchaseOrderAsync() for summaries");
        Console.WriteLine("   5. Use ProcessPurchaseOrderAsync() for custom queries");

        await Task.CompletedTask;
    }
}
