using Azure.Identity;
using A2A;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.SemanticKernel;
using System.Text.Json;

Console.WriteLine("Starting PO SK Agents Client...");

A2ACardResolver intakeAgentCardResolver = new A2ACardResolver(new Uri("http://localhost:5036"));
AgentCard intakeAgentCard = await intakeAgentCardResolver.GetAgentCardAsync();
Console.WriteLine($"Processing Agent Card: {intakeAgentCard.Name}");
A2AClient intakeAgentClient = new A2AClient(new Uri(intakeAgentCard.Url));

A2ACardResolver processingAgentCardResolver = new A2ACardResolver(new Uri("http://localhost:5207"));
AgentCard processingAgentCard = await processingAgentCardResolver.GetAgentCardAsync();
Console.WriteLine($"Processing Agent Card: {processingAgentCard.Name}");
A2AClient processingAgentClient = new A2AClient(new Uri(processingAgentCard.Url));

PurchaseOrder purchaseOrder = await MessageIntakeAgent(intakeAgentClient);
purchaseOrder = await MessageProcessingAgent(processingAgentClient, purchaseOrder);

Console.WriteLine($"Final Purchase Order Approval Status: {(purchaseOrder.IsApproved ? "Approved" : "Rejected")}");
Console.WriteLine($"Approval Reason: {purchaseOrder.ApprovalReason}");

async Task<PurchaseOrder> MessageIntakeAgent(A2AClient agentClient)
{
    Console.WriteLine("Invoking Message Intake Agent...");
    var fileBytes = File.ReadAllBytes("PurchaseOrders/AdventureWorksPO_HROrder.png");

    AgentMessage message = new AgentMessage
    {
        Parts = [new FilePart { File = new FileWithBytes { Bytes = Convert.ToBase64String(fileBytes) } }]
    };

    A2AResponse response = await agentClient.SendMessageAsync(new MessageSendParams { Message = message });
    AgentMessage responseMessage = (AgentMessage)response;

    TextPart? textPart = null;
    foreach (var part in responseMessage.Parts)
    {
        textPart = part as TextPart ?? part.AsTextPart();
        if (textPart != null) break;
    }

    PurchaseOrder po;
    try
    {
        if (!string.IsNullOrEmpty(textPart?.Text))
        {
            // Configure JsonSerializer options for case-insensitive property matching
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            po = JsonSerializer.Deserialize<PurchaseOrder>(textPart.Text, options) ?? new PurchaseOrder();
            Console.WriteLine($"Successfully deserialized PurchaseOrder - PO Number: {po.PoNumber}, Total: ${po.GrandTotal:F2}");
        }
        else
        {
            Console.WriteLine("No text content found in response, creating empty PurchaseOrder");
            po = new PurchaseOrder();
        }
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Failed to deserialize JSON response: {ex.Message}");
        Console.WriteLine($"Raw response text: {textPart?.Text}");
        po = new PurchaseOrder();
    }

    Console.WriteLine($"Message Intake Agent Done.");

    return po;
}

async Task<PurchaseOrder> MessageProcessingAgent(A2AClient agent, PurchaseOrder po)
{
    Console.WriteLine("Invoking Message Processing Agent...");
    AgentMessage message = new AgentMessage
    {
        Parts = [new TextPart { Text = JsonSerializer.Serialize(po) }]
    };
    A2AResponse response = await agent.SendMessageAsync(new MessageSendParams { Message = message });
    AgentMessage responseMessage = (AgentMessage)response;
    TextPart? textPart = null;
    foreach (var part in responseMessage.Parts)
    {
        textPart = part as TextPart ?? part.AsTextPart();
        if (textPart != null) break;
    }

    if (!string.IsNullOrEmpty(textPart?.Text))
    {
        using JsonDocument doc = JsonDocument.Parse(textPart.Text);
        JsonElement root = doc.RootElement;

        if (root.TryGetProperty("isApproved", out JsonElement isApprovedElement))
        {
            po.IsApproved = isApprovedElement.GetBoolean();
        }
        if (root.TryGetProperty("approvalReason", out JsonElement approvalReasonElement))
        {
            po.ApprovalReason = approvalReasonElement.GetString();
        }
    }
    Console.WriteLine($"Message Processing Agent Done.");

    return po;
}