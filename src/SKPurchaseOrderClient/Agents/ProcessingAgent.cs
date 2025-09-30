// The goal of this agent is to take in the JSON that was produced from
// the IntakeAgent, and determine if it can be approved or not.

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class ProcessingAgent
{
    private readonly ChatCompletionAgent _agent;
    private readonly ChatHistory _chatHistory;

    public ProcessingAgent(Kernel kernel)
    {
        _agent = Create(kernel);
        _chatHistory = new ChatHistory();
    }

    private ChatCompletionAgent Create(Kernel kernel)
    {
        return new ChatCompletionAgent
        {
            Kernel = kernel,
            //Name = "PurchaseOrderProcessingAgent", // Look, I love naming agents, but some models don't like it.
            Description = "An agent that processes purchase order data and determines if it can be approved.",
            Instructions =
            """
            You are a specialized document processor, and your task is to read the JSON data of a purchase order (PO) and determine if it can be approved.
            To do this, analyze this purchase order data and check the following:
            1. The Grand Total must be less than $10,000.
            2. The Supplier Name must not be empty.
            3. The Buyer Department must be one of the following: "Sales", "Marketing", "Engineering", "HR".
            Return the data strictly as JSON matching this schema:
            {
                "poNumber": "string",
                "isApproved": "boolean",
                "approvalReason": "string"
            }
            If the PO is not approved, provide a clear reason in the approvalReason field.
            Do not include any additional text or explanations.
            """,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }
            )
        };
    }

    public async Task<string> InvokeAsync(Models.PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
    {
        // Serialize the purchase order to JSON
        string poJson = System.Text.Json.JsonSerializer.Serialize(purchaseOrder);
        
        // Add the PO data as a user message
        _chatHistory.AddUserMessage($"Please analyze this purchase order and determine if it can be approved:\n\n{poJson}");
        
        // Get the response from the agent
        string result = string.Empty;
        await foreach (var response in _agent.InvokeAsync(_chatHistory, cancellationToken: cancellationToken))
        {
            result = response.Message.Content ?? string.Empty;
        }
        
        return result;
    }
}