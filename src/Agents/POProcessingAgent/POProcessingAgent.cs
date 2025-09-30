using A2A;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Agents.A2A;
using Microsoft.SemanticKernel.ChatCompletion;
using System;

/// <summary>
/// POProcessingAgent - A Semantic Kernel A2A agent for processing Purchase Orders
/// This agent demonstrates how to work with PurchaseOrder objects using AI capabilities
/// </summary>
public class POProcessingAgent
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ChatCompletionAgent _agent;
    private ITaskManager? _taskManager;

    public POProcessingAgent(ILogger logger, HttpClient httpClient, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _configuration = configuration;

        // Initialize the agent
        _agent = InitializeAgent();
    }

    private ChatCompletionAgent InitializeAgent()
    {
        var deploymentName = Environment.GetEnvironmentVariable("DEPLOYMENT_NAME") ?? throw new ArgumentException("DEPLOYMENT_NAME must be provided");
        var endpoint = Environment.GetEnvironmentVariable("ENDPOINT") ?? throw new ArgumentException("ENDPOINT must be provided");
        var apiKey = Environment.GetEnvironmentVariable("API_KEY") ?? throw new ArgumentException("API_KEY must be provided");

        _logger.LogInformation("Initializing Semantic Kernel agent with model {deploymentName}", deploymentName);

        var builder = Kernel.CreateBuilder();
        builder.Services.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

        var kernel = builder.Build();

        var poProcessingAgent = new ChatCompletionAgent()
        {
            Kernel = kernel,
            Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
            Name = "POProcessingAgent", // Good to keep in mind here that there CAN NOT be a space in the name.
            Description = "An agent that processes Purchase Order Images and extracts key details.",
            Instructions =
            """
            Analyze this purchase order image and extract the key details: PO Number, Grand Total, Supplier Name, Notes, and Buyer Department.
            Return the data strictly as JSON matching this schema:
            {
                "poNumber": "string",
                "subTotal": "number",
                "tax": "number",
                "grandTotal": "number",
                "supplierName": "string",
                "buyerDepartment": "string"
                "notes": "string"
            }
            Do not include any additional text or explanations.
            """
        };

        _logger.LogInformation("Purchase Order Agent created successfully");
        return poProcessingAgent;
    }

    

    public void Attach(ITaskManager taskManager)
    {
        _taskManager = taskManager;
        _taskManager.OnAgentCardQuery = GetAgentCardAsync;
        _taskManager.OnMessageReceived = ProcessMessageAsync;
    }

    private Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<AgentCard>(cancellationToken);
        }

        var capabilities = new AgentCapabilities()
        {
            Streaming = true,
            PushNotifications = false,
        };

        var orderAgentSkill = new AgentSkill()
        {
            Id = "purchase-order-processing-agent",
            Name = "PurchaseOrderProcessingAgent",
            Description = "A skill that processes Purchase Order Images and extracts key details.",
            Tags = ["purchase-order", "image-processing", "data-extraction"],
            Examples =
            [
                "Return a list of all orders.",
                "Get order details by Customer ID.",
                "Get order details by Order ID.",
                "Update order status.",
                "Create a new order.",
            ],
        };

        return Task.FromResult(new AgentCard()
        {
            Name = "The Order Agent",
            Description = "An agent that manages customer orders.",
            Url = agentUrl,
            Version = "1.0.0",
            DefaultInputModes = ["image/png"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [orderAgentSkill],

        });
    }

    private async Task<A2AResponse> ProcessMessageAsync(MessageSendParams messageSendParams, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing message.");
        //if (cancellationToken.IsCancellationRequested)
        //{
            //return new AgentMessage { Message = null, Error = "Operation cancelled" };
        //}

        var filePart = messageSendParams.Message.Parts.OfType<FilePart>().First();

        // Okay, lets process the message.
        try
        {
            var chatMessage = new ChatMessageContent(AuthorRole.User, "Please analyze this purchase order image and extract the key details.");
            
            // Process the image using the dedicated function
            var imageContent = ProcessImageFromFilePart(filePart);
            if (imageContent != null)
            {
                chatMessage.Items.Add(imageContent);
            }
            else
            {
                chatMessage.Items.Add(new TextContent("Error: Could not process the image file"));
            }

            var artifact = new Artifact();
            await foreach (AgentResponseItem<ChatMessageContent> response in _agent.InvokeAsync([chatMessage], cancellationToken: cancellationToken))
            {
                var content = response.Message.Content;
                artifact.Parts.Add(new TextPart { Text = content! });
            }

            var agentMessage = new AgentMessage
            {
                Role = MessageRole.Agent,
                MessageId = Guid.NewGuid().ToString(),
                ContextId = messageSendParams.Message.ContextId,
                Parts = artifact.Parts
            };

            return agentMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            return new AgentMessage
            {
                Role = MessageRole.Agent,
                MessageId = Guid.NewGuid().ToString(),
                ContextId = messageSendParams.Message.ContextId,
                Parts = [new TextPart { Text = "Sorry, I encountered an error processing your request." }]
            };
        }
    }

    /// <summary>
    /// Processes a FilePart containing image data and returns an ImageContent object
    /// </summary>
    /// <param name="filePart">The FilePart containing the image file</param>
    /// <returns>ImageContent object ready for AI processing, or null if processing failed</returns>
    private ImageContent? ProcessImageFromFilePart(FilePart filePart)
    {
        try
        {
            // Access the File property which contains FileContent
            var fileContent = filePart.File;
            if (fileContent == null)
            {
                _logger.LogWarning("FilePart does not contain file content");
                return null;
            }

            // Check if we can get the MIME type from metadata
            string mimeType = "image/png"; // Default to PNG
            if (filePart.Metadata?.ContainsKey("contentType") == true)
            {
                mimeType = filePart.Metadata["contentType"].ToString() ?? mimeType;
            }
            else if (filePart.Metadata?.ContainsKey("Content-Type") == true)
            {
                mimeType = filePart.Metadata["Content-Type"].ToString() ?? mimeType;
            }

            // Extract the Base64 string and convert to binary data
            if (fileContent is FileWithBytes fileWithBytes && !string.IsNullOrEmpty(fileWithBytes.Bytes))
            {
                // Convert Base64 string back to binary data
                var imageBytes = Convert.FromBase64String(fileWithBytes.Bytes);
                var binaryData = BinaryData.FromBytes(imageBytes);

                _logger.LogInformation("Successfully processed image file: {Size} bytes, MIME: {MimeType}", 
                    imageBytes.Length, mimeType);

                return new ImageContent(binaryData, mimeType);
            }
            else
            {
                _logger.LogWarning("FileContent is not FileWithBytes or Bytes is empty");
                return null;
            }
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Failed to decode Base64 image data");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process image from FilePart");
            return null;
        }
    }
}