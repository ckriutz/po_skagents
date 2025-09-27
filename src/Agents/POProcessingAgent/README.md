# POProcessingAgent

A Semantic Kernel A2A (Agent-to-Agent) agent for processing Purchase Orders using AI capabilities.

## Overview

This agent demonstrates how to use Microsoft's Semantic Kernel A2A framework to build intelligent agents that can process, validate, and analyze Purchase Orders using AI.

## Features

- **Purchase Order Processing**: Intelligent analysis and processing of purchase order data
- **Validation**: AI-powered validation of purchase order fields and business rules
- **Summarization**: Generate concise summaries of purchase orders
- **Custom Queries**: Process custom questions and analysis requests about purchase orders

## Project Structure

```
POProcessingAgent/
├── Models/
│   └── PurchaseOrder.cs       # Purchase order data models
├── Services/
│   └── POProcessingAgentService.cs  # Main agent service
├── Program.cs                 # Console application entry point
├── README.md                  # This file
└── POProcessingAgent.csproj   # Project file
```

## Models

### PurchaseOrder
The main data model representing a purchase order with:
- Supplier information (name, address, etc.)
- Line items collection
- Purchase order metadata (PO number, created by, department)
- Tax calculations
- Approval workflow fields

### PurchaseOrderItem
Represents individual line items in a purchase order:
- Item code and description
- Quantity and unit price
- Line total calculations

## Services

### POProcessingAgentService
The main service class that provides:
- `ProcessPurchaseOrderAsync()` - General purpose PO processing with custom queries
- `ValidatePurchaseOrderAsync()` - AI-powered validation
- `SummarizePurchaseOrderAsync()` - Generate PO summaries

## Configuration Required

To use this agent, you'll need to configure:

1. **A2A Agent Endpoint**: URL to your A2A agent service
2. **HTTP Client**: Properly configured HttpClient for API communication
3. **Agent Card**: Resolution of the agent card for capabilities discovery

## Dependencies

- `Microsoft.SemanticKernel.Agents.A2A` - Core A2A agent functionality
- `System.Text.Json` - JSON serialization for data exchange

## Usage Example

```csharp
// Configure A2A client and agent (configuration details needed)
var client = new A2AClient(agentUrl, httpClient);
var cardResolver = new A2ACardResolver(agentUrl, httpClient);
var agentCard = await cardResolver.GetAgentCardAsync();
var agent = new A2AAgent(client, agentCard);

// Create the processing service
var agentService = new POProcessingAgentService(agent);

// Process a purchase order
var result = await agentService.ValidatePurchaseOrderAsync(purchaseOrder);
Console.WriteLine(result);
```

## Running the Application

```bash
dotnet run
```

This will demonstrate the purchase order data structure and show sample output.

## Next Steps

1. Configure the A2A agent endpoint and authentication
2. Implement specific business rules for your organization
3. Add additional processing capabilities as needed
4. Integrate with your existing systems