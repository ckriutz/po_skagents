using A2A;
using A2A.AspNetCore;
using Microsoft.VisualBasic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();
builder.Services.AddHttpClient();

var app = builder.Build();

var taskManager = new TaskManager();

var agent = new POProcessingAgent(new LoggerFactory().CreateLogger<POProcessingAgent>(), new HttpClient(), builder.Configuration);
agent.Attach(taskManager);
app.MapA2A(taskManager, "/");
app.MapWellKnownAgentCard(taskManager, "/");
app.MapHttpA2A(taskManager, "/");

app.Run();