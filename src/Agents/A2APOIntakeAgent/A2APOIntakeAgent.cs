using A2A;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

public class A2APOIntakeAgent
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ChatCompletionAgent _agent;
    private ITaskManager? _taskManager;

    public A2APOIntakeAgent(ILogger logger, HttpClient httpClient, IConfiguration configuration)
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

        var poIntakeAgent = new ChatCompletionAgent()
        {
            Kernel = kernel,
            Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
            Name = "POIntakeAgent", // Good to keep in mind here that there CAN NOT be a space in the name.
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

        _logger.LogInformation("Purchase Order Intake Agent initialized successfully.");
        return poIntakeAgent;
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
            Id = "purchase-order-intake-agent",
            Name = "PurchaseOrderIntakeAgent",
            Description = "A skill that processes Purchase Order Images and extracts key details.",
            Tags = ["purchase-order", "image-processing", "data-extraction"],
            Examples =
            [
                "Extract key details from this purchase order image.",
                "Analyze the attached PO image and return the data as JSON.",
                "Process this purchase order image and provide the PO number, total amount, supplier name, and buyer department.",
                "Verify this purchase order image is a valid PO and extract the relevant information.",
                "Scan the provided PO image and return the extracted details in JSON format."
            ],
        };

        return Task.FromResult(new AgentCard()
        {
            Name = "The Purchase Order Intake Agent",
            Description = "An agent that manages purchase order images.",
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
        if (cancellationToken.IsCancellationRequested)
        {
            return new AgentMessage
            {
                Role = MessageRole.Agent,
                MessageId = Guid.NewGuid().ToString(),
                ContextId = messageSendParams.Message.ContextId,
                Parts = [new TextPart { Text = "Request was cancelled." }]
            };
        }

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