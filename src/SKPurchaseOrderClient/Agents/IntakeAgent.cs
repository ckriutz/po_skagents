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
    
    public async Task<string> InvokeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Purchase order file not found: {filePath}");
        }

        // Read the image file
        byte[] imageData = await File.ReadAllBytesAsync(filePath, cancellationToken);

        // Determine the image format based on file extension
        string mimeType = Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "image/png" // default to PNG
        };

        // Create a message with the image
        var messageWithImage = new ChatMessageContent(
            AuthorRole.User,
            [
                new TextContent("Please extract the purchase order information from this image according to your instructions."),
                new ImageContent(imageData, mimeType)
            ]);

        // Process the image with the agent
        await foreach (var response in _agent.InvokeAsync(messageWithImage, cancellationToken: cancellationToken))
        {
            return response.Message.Content ?? string.Empty;
        }

        return string.Empty;
    }
}