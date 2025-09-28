# POProcessingAgent

A Semantic Kernel A2A (Agent-to-Agent) agent for processing Purchase Order images using AI capabilities. This agent extracts key information from purchase order images and returns structured JSON data.

## Overview

This agent demonstrates how to use Microsoft's Semantic Kernel A2A framework to build intelligent agents that can analyze purchase order images and extract structured data using Azure OpenAI's vision capabilities.

## Features

- **Image Processing**: Analyzes purchase order images (PNG format supported)
- **Data Extraction**: Extracts key details including PO Number, totals, supplier information, and department
- **JSON Output**: Returns structured JSON data following a predefined schema
- **A2A Integration**: Fully integrated with Microsoft's Agent-to-Agent framework
- **RESTful API**: Exposes HTTP endpoints for agent communication

## Project Structure

```
POProcessingAgent/
├── POProcessingAgent.cs       # Main agent implementation
├── PurchaseOrder.cs          # Purchase order data models
├── Program.cs                # ASP.NET Core web application entry point
├── README.md                 # This file
└── POProcessingAgent.csproj  # Project file (.NET 9.0 Web SDK)
```

## Architecture

### POProcessingAgent Class
The main agent class that:
- Wraps a `ChatCompletionAgent` from Semantic Kernel
- Handles A2A message processing and image analysis
- Integrates with Azure OpenAI for vision capabilities
- Manages agent lifecycle and task manager attachment

### PurchaseOrder Models
Data models representing purchase order structure:

**PurchaseOrder**: Main model with supplier info, line items, metadata, and calculated totals
- Supplier information (name, address, contact details)
- Line items collection
- Purchase order metadata (PO number, created by, department)
- Tax calculations with automatic subtotal, tax amount, and grand total
- Approval workflow fields

**PurchaseOrderItem**: Individual line items with item code, description, quantity, unit price, and line total

### Program.cs
ASP.NET Core web application that:
- Bootstraps the Semantic Kernel A2A TaskManager
- Configures dependency injection and HTTP client
- Exposes A2A endpoints (`/`, `/.well-known/agent-card.json`)
- Provides health check endpoint (`/health`)

## Configuration Required

The agent requires these environment variables at startup:
- `DEPLOYMENT_NAME` - Azure OpenAI deployment name
- `ENDPOINT` - Azure OpenAI endpoint URL
- `API_KEY` - Azure OpenAI API key

If missing, run the command: `source .env` to set the required environment variables from the `.env` file.

## Dependencies

- `A2A` (0.3.1-preview) - Core A2A framework
- `A2A.AspNetCore` (0.3.1-preview) - ASP.NET Core integration
- `Microsoft.SemanticKernel.Agents.A2A` (1.65.0-alpha) - A2A agent functionality
- `Microsoft.SemanticKernel.Agents.Core` (1.65.0) - Core agent framework
- `Microsoft.SemanticKernel.Connectors.AzureOpenAI` (1.65.0) - Azure OpenAI connector
- `Microsoft.SemanticKernel.Connectors.OpenAI` (1.65.0) - OpenAI connector
- `System.Text.Json` (9.0.9) - JSON serialization

## Agent Output Schema

The agent extracts purchase order data and returns it as JSON:

```json
{
  "poNumber": "string",
  "subTotal": "number",
  "tax": "number", 
  "grandTotal": "number",
  "supplierName": "string",
  "buyerDepartment": "string",
  "notes": "string"
}
```

## API Endpoints

- `GET /health` - Health check endpoint
- `POST /` - A2A message processing endpoint
- `GET /.well-known/agent-card.json` - Agent capabilities discovery

## Building and Running

### Prerequisites
- .NET 9.0 SDK
- Azure OpenAI access with vision-capable model

### Build
```bash
dotnet restore  # Only needed after package changes
dotnet build    # For CI-style validation
```

### Run
```bash
dotnet run      # Launch the web host on default Kestrel ports
```

### Health Check
Once the app is running, verify it's responding:

```bash
curl http://localhost:5000/health
```

Expected response: `{"status":"healthy"}`

### Agent Card
The agent capabilities can be discovered at:
```bash
curl http://localhost:5000/.well-known/agent-card.json
```

## Troubleshooting

- **Environment Variables**: Ensure `DEPLOYMENT_NAME`, `ENDPOINT`, and `API_KEY` are set
- **Build Errors**: Check that all preview packages are compatible
- **Agent Card**: Located at `http://localhost:5000/.well-known/agent-card.json`

## Next Steps

1. Configure Azure OpenAI environment variables
2. Test with purchase order images
3. Extend the agent for additional document types
4. Add validation logic for extracted data