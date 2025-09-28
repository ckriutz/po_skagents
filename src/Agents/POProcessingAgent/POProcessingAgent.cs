using A2A;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Agents.A2A;


/// <summary>
/// POProcessingAgent - A Semantic Kernel A2A agent for processing Purchase Orders
/// This agent demonstrates how to work with PurchaseOrder objects using AI capabilities
/// </summary>
public class POProcessingAgent
{
    private readonly ILogger<POProcessingAgent> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ChatCompletionAgent _agent;
    private ITaskManager? _taskManager;

    public POProcessingAgent(ILogger<POProcessingAgent> logger, HttpClient httpClient, IConfiguration configuration)
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
        builder.Services.AddOpenAIChatCompletion(deploymentName, endpoint, apiKey);

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
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [orderAgentSkill],

        });
    }

    private Task<A2AResponse> ProcessMessageAsync(MessageSendParams messageSendParams, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<A2AResponse>(cancellationToken);
        }

        var message = messageSendParams.Message.Parts.OfType<FilePart>().First();
        _logger.LogInformation("Processing message: {Message}", message);

        var agentMessage = new AgentMessage
        {
            Role = MessageRole.Agent,
            MessageId = Guid.NewGuid().ToString(),
            ContextId = messageSendParams.Message.ContextId,
            Parts = [new TextPart { Text = $"Processing {message}" }]
        };

        return Task.FromResult<A2AResponse>(agentMessage);

    }

}