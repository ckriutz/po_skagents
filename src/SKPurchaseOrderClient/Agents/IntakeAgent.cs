using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class IntakeAgent
{
    private readonly ChatCompletionAgent _agent;
    private readonly ChatHistory _chatHistory;

    public IntakeAgent(Kernel kernel)
    {
        _agent = Create(kernel);
        _chatHistory = new ChatHistory();
    }

    // Put all the logic into creating the Kernel here.
    private ChatCompletionAgent Create(Kernel kernel)
    {
        return new ChatCompletionAgent
        {
            Kernel = kernel,
            //Name = "PurchaseOrderIntakeAgent", // Look, I love naming agents, but some models don't like it.
            Description = "An agent that processes purchase order images and extracts relevant information.",
            Instructions =
            """
            You are a specialized document processor, and your task is to read the image of a purchase order (PO) and extract the relevant information in a structured format.
            To do this, analyze this purchase order image and extract the key details: PO Number, Grand Total, Supplier Name, Notes, and Buyer Department.
            Return the data strictly as JSON matching this schema:
            {
                "poNumber": "string",
                "subTotal": "number",
                "tax": "number",
                "grandTotal": "number",
                "supplierName": "string",
                "buyerDepartment": "string",
                "notes": "string"
            }
            Do not include any additional text or explanations.
            """,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }
            )
        };
    }
    
    public async Task<string> InvokeAsync(string message, CancellationToken cancellationToken = default)
    {
        var userMessage = new ChatMessageContent(AuthorRole.User, [new TextContent(message)]);
        await foreach (var response in _agent.InvokeAsync(userMessage, cancellationToken: cancellationToken))
        {
            return response.Message.Content ?? string.Empty;
        }
        
        return string.Empty;
    }
    // public async Task<string> InvokeAsync(string filePath, CancellationToken cancellationToken = default)
    // {
    //     if (!File.Exists(filePath))
    //     {
    //         throw new FileNotFoundException($"Purchase order file not found: {filePath}");
    //     }

    //     // Read the image file
    //     byte[] imageData = await File.ReadAllBytesAsync(filePath, cancellationToken);

    //     // Determine the image format based on file extension
    //     string mimeType = Path.GetExtension(filePath).ToLowerInvariant() switch
    //     {
    //         ".png" => "image/png",
    //         ".jpg" or ".jpeg" => "image/jpeg",
    //         ".gif" => "image/gif",
    //         ".bmp" => "image/bmp",
    //         _ => "image/png" // default to PNG
    //     };

    //     // Create a message with the image
    //     var messageWithImage = new ChatMessageContent(
    //         AuthorRole.User,
    //         [
    //             new TextContent("Please extract the purchase order information from this image according to your instructions."),
    //             new ImageContent(imageData, mimeType)
    //         ]);

    //     // Process the image with the agent
    //     await foreach (var response in _agent.InvokeAsync(messageWithImage, cancellationToken: cancellationToken))
    //     {
    //         return response.Message.Content ?? string.Empty;
    //     }

    //     return string.Empty;
    // }

    // Factory method for creating a purchase order intake agent WITH validation plugins
    // public static ChatCompletionAgent CreateWithValidation(Kernel kernel)
    // {
    //     // Clone the kernel to add agent-specific plugins
    //     var agentKernel = kernel.Clone();

    //     // Add the purchase order validation plugin
    //     agentKernel.Plugins.AddFromType<SKPurchaseOrderClient.Plugins.PurchaseOrderPlugin>("POValidation");

    //     return new ChatCompletionAgent
    //     {
    //         Kernel = agentKernel,
    //         Name = "PurchaseOrderIntakeAgent",
    //         Description = "An agent that processes purchase order images, extracts information, and validates the data.",
    //         Instructions =
    //         """
    //         You are a specialized document processor that reads purchase order (PO) images and extracts relevant information.

    //         STEP 1: Analyze the purchase order image and extract the key details: PO Number, Subtotal, Tax, Grand Total, Supplier Name, Buyer Department, and Notes.

    //         STEP 2: After extracting the data, use the available validation functions to:
    //         - Validate the PO number format
    //         - Verify tax calculations based on the department
    //         - Look up supplier information
    //         - Validate that grand total = subtotal + tax

    //         STEP 3: Return the extracted data as JSON with validation results:
    //         {
    //             "poNumber": "string",
    //             "subTotal": "number",
    //             "tax": "number",
    //             "grandTotal": "number",
    //             "supplierName": "string",
    //             "buyerDepartment": "string",
    //             "notes": "string",
    //             "validationResults": {
    //                 "poNumberValid": "validation message",
    //                 "taxValid": "validation message",
    //                 "supplierValid": "validation message",
    //                 "grandTotalValid": "validation message"
    //             }
    //         }

    //         Do not include any additional text or explanations outside the JSON.
    //         """,
    //         Arguments = new KernelArguments(
    //             new OpenAIPromptExecutionSettings()
    //             {
    //                 // Enable automatic function calling so the agent can call validation functions
    //                 FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    //             })
    //     };
    //}

    // Process a purchase order file and return extracted data
    // public static async IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> ProcessPurchaseOrderAsync(
    //     this ChatCompletionAgent agent,
    //     string filePath,
    //     [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    // {
    //     if (!File.Exists(filePath))
    //     {
    //         throw new FileNotFoundException($"Purchase order file not found: {filePath}");
    //     }

    //     // Read the image file
    //     byte[] imageData = await File.ReadAllBytesAsync(filePath, cancellationToken);

    //     // Determine the image format based on file extension
    //     string mimeType = Path.GetExtension(filePath).ToLowerInvariant() switch
    //     {
    //         ".png" => "image/png",
    //         ".jpg" or ".jpeg" => "image/jpeg",
    //         ".gif" => "image/gif",
    //         ".bmp" => "image/bmp",
    //         _ => "image/png" // default to PNG
    //     };

    //     // Create a message with the image
    //     var messageWithImage = new ChatMessageContent(
    //         AuthorRole.User,
    //         [
    //             new TextContent("Please extract the purchase order information from this image according to your instructions."),
    //             new ImageContent(imageData, mimeType)
    //         ]);

    //     // Process the image with the agent
    //     await foreach (var response in agent.InvokeAsync(messageWithImage, cancellationToken: cancellationToken))
    //     {
    //         yield return response;
    //     }
    // }

    // Convenience method to process a single purchase order and get the first response
    // public static async Task<string> ProcessSinglePurchaseOrderAsync(
    //     this ChatCompletionAgent agent,
    //     string filePath,
    //     CancellationToken cancellationToken = default)
    // {
    //     await foreach (var response in agent.ProcessPurchaseOrderAsync(filePath, cancellationToken))
    //     {
    //         return response.Message.Content ?? string.Empty;
    //     }
    //     return string.Empty;
    // }
}