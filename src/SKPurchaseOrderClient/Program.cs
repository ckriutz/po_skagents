using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.ChatCompletion;
using System.IO;


var deploymentName = Environment.GetEnvironmentVariable("DEPLOYMENT_NAME") ?? throw new ArgumentException("DEPLOYMENT_NAME must be provided");
var endpoint = Environment.GetEnvironmentVariable("ENDPOINT") ?? throw new ArgumentException("ENDPOINT must be provided");
var apiKey = Environment.GetEnvironmentVariable("API_KEY") ?? throw new ArgumentException("API_KEY must be provided");

// Create a cancellation token source for graceful shutdown
using var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var builder = Kernel.CreateBuilder();
builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// OPTION 1: Create the intake agent WITHOUT plugins (simple extraction only)
var intakeAgent = new IntakeAgent(kernel);

// Path to the purchase orders folder
string purchaseOrdersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PurchaseOrders/AdventureWorksPO_HROrder.png");

Console.WriteLine($"\nProcessing: {purchaseOrdersPath}");

var response = await intakeAgent.InvokeAsync(purchaseOrdersPath, cancellationToken);
Console.WriteLine($"Extracted Data: {response}");
Console.WriteLine("Done Processing.");

var jsonOptions = new System.Text.Json.JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};
Models.PurchaseOrder po = System.Text.Json.JsonSerializer.Deserialize<Models.PurchaseOrder>(response, jsonOptions)!;
Console.WriteLine($"PO Number: {po.PoNumber}");

//Now lets analyze the PO to see if it can be approved.
var processingAgent = new ProcessingAgent(kernel);
var processingResponse = await processingAgent.InvokeAsync(po, cancellationToken);
Console.WriteLine($"Processing Response: {processingResponse}");
