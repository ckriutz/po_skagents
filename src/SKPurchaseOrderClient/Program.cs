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

var builder = Kernel.CreateBuilder();
builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// OPTION 1: Create the intake agent WITHOUT plugins (simple extraction only)
var intakeAgent = new IntakeAgent(kernel);

// OPTION 2: Create the intake agent WITH validation plugins (AI will automatically call validation functions)
//var intakeAgent = IntakeAgent.CreateWithValidation(kernel);

//Console.WriteLine($"\nUsing agent: {intakeAgent.Name}");
//Console.WriteLine($"Agent has access to {intakeAgent.Kernel.Plugins.Count} plugin(s)");

// Path to the purchase orders folder
string purchaseOrdersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PurchaseOrders/AdventureWorksPO_HROrder.png");

//string[] imageFiles = Directory.GetFiles(purchaseOrdersPath, "*.png");

//if (imageFiles.Length == 0)
//{
//Console.WriteLine("No PNG files found in PurchaseOrders folder.");
//return;
//}

Console.WriteLine($"\nProcessing: {purchaseOrdersPath}");
var response = await intakeAgent.InvokeAsync("Can you extract the purchase order information from this image?", new CancellationToken());
//await foreach (var response in 
//{
    Console.WriteLine($"Extracted Data: {response}");
//}
Console.WriteLine("Done Processing.");
// Process each purchase order image
//foreach (string imagePath in imageFiles)
//{
    //Console.WriteLine($"\n=== Processing: {Path.GetFileName(imagePath)} ===");

    //try
    //{
        // Option 1: Stream all responses (current approach)
        //await foreach (var response in intakeAgent.ProcessPurchaseOrderAsync(imagePath))
        //{
            //Console.WriteLine($"Extracted Data: {response.Message.Content}");
        //}

        // Option 2: Get just the first response (simpler)
        // string extractedData = await intakeAgent.ProcessSinglePurchaseOrderAsync(imagePath);
        // Console.WriteLine($"Extracted Data: {extractedData}");
    //}
 //catch (Exception ex)
   //{
   //    Console.WriteLine($"Error processing {Path.GetFileName(imagePath)}: {ex.Message}");
   //}

   //Console.WriteLine(new string('-', 50));
//}
