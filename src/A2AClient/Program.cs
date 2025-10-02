using Azure.Identity;
using A2A;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.SemanticKernel;

Console.WriteLine("Starting PO SK Agents Client...");

A2ACardResolver intakeAgentCardResolver = new A2ACardResolver(new Uri("http://localhost:5000"));
AgentCard intakeAgentCard = await intakeAgentCardResolver.GetAgentCardAsync();
Console.WriteLine($"Processing Agent Card: {intakeAgentCard.Name}");
A2AClient intakeAgentClient = new A2AClient(new Uri(intakeAgentCard.Url));

A2ACardResolver processingAgentCardResolver = new A2ACardResolver(new Uri("http://localhost:5207"));
AgentCard processingAgentCard = await processingAgentCardResolver.GetAgentCardAsync();
Console.WriteLine($"Processing Agent Card: {processingAgentCard.Name}");
A2AClient processingAgentClient = new A2AClient(new Uri(processingAgentCard.Url));

await MessageIntakeAgent(intakeAgentClient);
await MessageProcessingAgent(processingAgentClient);

async Task<PurchaseOrder> MessageIntakeAgent(A2AClient agentClient)
{
    Console.WriteLine("Invoking Message Processing Agent...");
    var fileBytes = File.ReadAllBytes("PurchaseOrders/AdventureWorksPO_HROrder.png");

    AgentMessage message = new AgentMessage
    {
        Parts = [new FilePart { File = new FileWithBytes { Bytes = Convert.ToBase64String(fileBytes) } }]
    };

    A2AResponse response = await agentClient.SendMessageAsync(new MessageSendParams { Message = message });

    Console.WriteLine($"Response: {response}");

    AgentMessage responseMessage = (AgentMessage)response;
    var partCount = responseMessage.Parts.ToList().Count();
    Console.WriteLine($"Response Part Count: {partCount}");

    TextPart? textPart = null;
    foreach (var part in responseMessage.Parts)
    {
        textPart = part as TextPart ?? part.AsTextPart();
        if (textPart != null) break;
    }
    Console.WriteLine($"Response Text Part: {textPart?.Text}");

    PurchaseOrder po = new PurchaseOrder();
    //po.Amount
    //return textPart?.Text;
    return po;
}

async Task<PurchaseOrder> MessageProcessingAgent(A2AClient agentClient)
{

}