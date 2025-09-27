using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.A2A;
using POProcessingAgent.Models;

namespace POProcessingAgent.Services;

/// <summary>
/// POProcessingAgent - A Semantic Kernel A2A agent for processing Purchase Orders
/// This agent demonstrates how to work with PurchaseOrder objects using AI capabilities
/// </summary>
public class POProcessingAgentService
{
    private readonly A2AAgent _agent;
    private readonly JsonSerializerOptions _jsonOptions;

    public POProcessingAgentService(A2AAgent agent)
    {
        _agent = agent ?? throw new ArgumentNullException(nameof(agent));
        _jsonOptions = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Process a purchase order using AI capabilities
    /// </summary>
    /// <param name="purchaseOrder">The purchase order to process</param>
    /// <param name="query">The processing query or instruction</param>
    /// <returns>AI-generated response about the purchase order</returns>
    public async Task<string> ProcessPurchaseOrderAsync(PurchaseOrder purchaseOrder, string query)
    {
        if (purchaseOrder == null)
            throw new ArgumentNullException(nameof(purchaseOrder));

        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty", nameof(query));

        // Serialize the purchase order to JSON for the AI agent
        var purchaseOrderJson = JsonSerializer.Serialize(purchaseOrder, _jsonOptions);
        
        // Create a comprehensive prompt that includes both the PO data and the query
        var prompt = $@"
I have a Purchase Order with the following details:

{purchaseOrderJson}

Please process this Purchase Order and respond to the following query:
{query}

Please provide a detailed analysis based on the purchase order data.";

        var responses = new List<string>();

        // Invoke the A2A agent with the prompt
        await foreach (var response in _agent.InvokeAsync(prompt))
        {
            // Based on the sample code pattern from Step01_A2AAgent.cs
            // The response should be an AgentResponseItem<ChatMessageContent>
            // We need to extract the content from the response
            var content = response.ToString(); // Simple approach to get the content
            if (!string.IsNullOrEmpty(content))
            {
                responses.Add(content);
            }
        }

        return string.Join("\n", responses);
    }

    /// <summary>
    /// Validate a purchase order using AI capabilities
    /// </summary>
    /// <param name="purchaseOrder">The purchase order to validate</param>
    /// <returns>AI-generated validation response</returns>
    public async Task<string> ValidatePurchaseOrderAsync(PurchaseOrder purchaseOrder)
    {
        return await ProcessPurchaseOrderAsync(purchaseOrder, 
            "Please validate this purchase order. Check for any missing required fields, unusual values, or potential issues. Provide recommendations for improvement if needed.");
    }

    /// <summary>
    /// Get purchase order summary using AI capabilities
    /// </summary>
    /// <param name="purchaseOrder">The purchase order to summarize</param>
    /// <returns>AI-generated summary</returns>
    public async Task<string> SummarizePurchaseOrderAsync(PurchaseOrder purchaseOrder)
    {
        return await ProcessPurchaseOrderAsync(purchaseOrder,
            "Please provide a concise summary of this purchase order, including key details like supplier, total amount, number of items, and any notable aspects.");
    }
}