# PO SK Agents Client

This .NET 9.0 console application includes all necessary packages for A2A (Application-to-Application) authentication and Semantic Kernel integration.

## Packages Included

### A2A Authentication
- **Azure.Identity** (v1.16.0) - Modern Azure authentication library
- **Microsoft.Graph** (v5.93.0) - Microsoft Graph SDK for accessing Office 365 services
- **Microsoft.Identity.Client** (v4.77.1) - Microsoft Authentication Library (MSAL)

### Semantic Kernel
- **Microsoft.SemanticKernel** (v1.65.0) - Core Semantic Kernel library
- **Microsoft.SemanticKernel.Connectors.OpenAI** (v1.65.0) - OpenAI connector for SK

### Configuration & Hosting
- **Microsoft.Extensions.Configuration** (v9.0.9) - Configuration system
- **Microsoft.Extensions.Configuration.Json** (v9.0.9) - JSON configuration provider
- **Microsoft.Extensions.DependencyInjection** (v9.0.9) - Dependency injection container
- **Microsoft.Extensions.Hosting** (v9.0.9) - Generic host for console applications

## Configuration

### appsettings.json
Update the `appsettings.json` file with your actual values:

```json
{
  "AzureAD": {
    "ClientId": "your-azure-ad-client-id",
    "ClientSecret": "your-azure-ad-client-secret", 
    "TenantId": "your-azure-ad-tenant-id"
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "ModelId": "gpt-4"
  }
}
```

### Azure AD App Registration
For A2A authentication, you need to:

1. Register an application in Azure AD
2. Create a client secret
3. Grant necessary Microsoft Graph permissions (e.g., `User.Read.All`, `Mail.Send`)
4. Grant admin consent for the permissions

## Usage Examples

### Microsoft Graph A2A Authentication

```csharp
// This is already configured in Program.cs
var graphClient = serviceProvider.GetRequiredService<GraphServiceClient>();

// Example: Get users
var users = await graphClient.Users.GetAsync();

// Example: Send email
var message = new Message
{
    Subject = "Test from A2A",
    Body = new ItemBody { Content = "Hello from application!", ContentType = BodyType.Text },
    ToRecipients = new List<Recipient>
    {
        new Recipient { EmailAddress = new EmailAddress { Address = "user@example.com" } }
    }
};

await graphClient.Users["sender@example.com"].SendMail.PostAsync(new SendMailPostRequestBody
{
    Message = message
});
```

### Semantic Kernel Integration

```csharp
// Configure with OpenAI (add to Program.cs)
kernelBuilder.AddOpenAIChatCompletion("gpt-4", configuration["OpenAI:ApiKey"]);

// Use kernel
var kernel = serviceProvider.GetRequiredService<Kernel>();
var response = await kernel.InvokePromptAsync("What is semantic kernel?");
Console.WriteLine(response);
```

### Combining Both

```csharp
// Get user info from Graph
var user = await graphClient.Users["user@example.com"].GetAsync();

// Use SK to generate personalized content
var prompt = $"Generate a welcome message for {user.DisplayName} who works at {user.CompanyName}";
var welcomeMessage = await kernel.InvokePromptAsync(prompt);

// Send via Graph
var message = new Message
{
    Subject = "Welcome!",
    Body = new ItemBody { Content = welcomeMessage.ToString(), ContentType = BodyType.Text },
    ToRecipients = new List<Recipient>
    {
        new Recipient { EmailAddress = new EmailAddress { Address = user.Mail } }
    }
};

await graphClient.Users["sender@example.com"].SendMail.PostAsync(new SendMailPostRequestBody
{
    Message = message
});
```

## Running the Application

```bash
cd /path/to/Client
dotnet run
```

## Next Steps

1. Configure your Azure AD app registration details in `appsettings.json`
2. Add your OpenAI API key for Semantic Kernel functionality
3. Implement your specific business logic using the configured services
4. Consider adding logging, error handling, and other production concerns