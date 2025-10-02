using A2A;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

public class A2APOProcessingAgent
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ChatCompletionAgent _agent;
    private ITaskManager? _taskManager;

    public A2APOProcessingAgent(ILogger logger, HttpClient httpClient, IConfiguration configuration)
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
            Description = "An agent that processes Purchase Order data and processes the rules.",
            Instructions =
            """
            You are a specialized document processor, and your task is to read the JSON data of a purchase order (PO) and determine if it can be approved.
            To do this, analyze this purchase order data and check the following:
            1. The Grand Total must be less than $1000.
            2. The Supplier Name must not be empty.
            3. The Buyer Department must be one of the following: "Travel", "Marketing", "IT", "HR".
            Return the data strictly as JSON matching this schema:
            {
                "poNumber": "string",
                "isApproved": "boolean",
                "approvalReason": "string"
            }
            Evaluate the rules, and  provide a clear reason in the approvalReason field.
            Do not include any additional text or explanations.
            """
        };

        _logger.LogInformation("Purchase Order Intake Agent initialized successfully.");
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
            Description = "A skill that processes Purchase Order details, and uses rules to determine if the PO is valid or not.",
            Tags = ["purchase-order", "data-processing", "rules-engine"],
            Examples =
            [
                "Review this Purchase Order, and verify the details.",
                "Does this purchase order meet all the requirements?",
                "Can we approve this purchase order?"
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

        var textPart = messageSendParams.Message.Parts.OfType<TextPart>().First();

        // Okay, lets process the message.
        try
        {
            var chatMessage = new ChatMessageContent(AuthorRole.User, "Please analyze this purchase order image and extract the key details.");

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
}