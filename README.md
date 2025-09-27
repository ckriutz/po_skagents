# po_skagents

A POC that shows how to use Semantic Kernel Agents on a Purchase Order

## Project Structure

This repository demonstrates the use of Microsoft Semantic Kernel A2A (Agent-to-Agent) framework for processing Purchase Orders with AI capabilities.

```
po_skagents/
├── src/
│   └── Agents/
│       └── POProcessingAgent/          # C# console application
│           ├── Models/
│           │   └── PurchaseOrder.cs    # Purchase Order data models
│           ├── Services/
│           │   └── POProcessingAgentService.cs # A2A Agent service
│           ├── Program.cs              # Main application entry point
│           ├── README.md               # Agent-specific documentation
│           └── POProcessingAgent.csproj # Project file
├── POSkAgents.sln                      # Solution file
├── README.md                           # This file
├── LICENSE                             # MIT License
└── .gitignore                          # Git ignore rules
```

## Features

- **POProcessingAgent**: A Semantic Kernel A2A agent for processing Purchase Orders
- **AI-Powered Validation**: Intelligent validation of purchase order data
- **Smart Summarization**: Generate concise summaries of purchase orders
- **Custom Query Processing**: Process specific questions about purchase orders
- **Extensible Architecture**: Ready for additional agents in the Agents folder

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- An A2A agent endpoint (configuration required)

### Building the Solution

```bash
# Clone the repository
git clone https://github.com/ckriutz/po_skagents.git
cd po_skagents

# Build the solution
dotnet build

# Run the POProcessingAgent
cd src/Agents/POProcessingAgent
dotnet run
```

### Configuration

The POProcessingAgent requires configuration of:

1. **A2A Agent URL**: Endpoint for your A2A agent service
2. **HTTP Client**: Properly configured HttpClient for API communication  
3. **Agent Card**: Resolution of agent capabilities

See the [POProcessingAgent README](src/Agents/POProcessingAgent/README.md) for detailed configuration instructions.

## Purchase Order Model

The solution includes a comprehensive Purchase Order model with:

- **Supplier Information**: Name, address, contact details
- **Line Items**: Item code, description, quantity, unit price, line total
- **Metadata**: PO number, created by, department, notes
- **Tax Calculations**: Automatic subtotal, tax amount, and grand total calculations
- **Approval Workflow**: Approval status and rejection reasons

## Example Usage

```csharp
// Create a purchase order
var purchaseOrder = new PurchaseOrder
{
    PoNumber = "PO-2025-001",
    SupplierName = "Tech Supply Co.",
    Items = new List<PurchaseOrderItem>
    {
        new PurchaseOrderItem
        {
            ItemCode = "LAP001",
            Description = "Business Laptop",
            Quantity = 5,
            UnitPrice = 1299.99m,
            LineTotal = 6499.95m
        }
    },
    TaxRate = 0.0875m
};

// Process with the agent
var agentService = new POProcessingAgentService(agent);
var validation = await agentService.ValidatePurchaseOrderAsync(purchaseOrder);
var summary = await agentService.SummarizePurchaseOrderAsync(purchaseOrder);
```

## Adding More Agents

This architecture supports multiple agents. To add a new agent:

1. Create a new folder in `src/Agents/`
2. Create a new C# console project: `dotnet new console -n YourAgentName`
3. Add the project to the solution: `dotnet sln add src/Agents/YourAgentName/YourAgentName.csproj`
4. Follow the same patterns as POProcessingAgent

## Dependencies

- **Microsoft.SemanticKernel.Agents.A2A**: Core A2A agent functionality
- **System.Text.Json**: JSON serialization for data exchange

## References

- [Building AI Agents A2A .NET SDK](https://devblogs.microsoft.com/foundry/building-ai-agents-a2a-dotnet-sdk/)
- [Semantic Kernel A2A Samples](https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/GettingStartedWithAgents/A2A)
- [Purchase Order Framework](https://github.com/ckriutz/po_skprocessframework)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
