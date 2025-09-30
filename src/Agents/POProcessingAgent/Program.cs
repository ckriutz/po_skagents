using A2A;
using A2A.AspNetCore;
using Microsoft.VisualBasic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();
builder.Services.AddHttpClient();

var app = builder.Build();
var httpClient = app.Services.GetRequiredService<HttpClient>();
var taskManager = new TaskManager();
var logger = app.Logger;

var agent = new POProcessingAgent(logger, httpClient, builder.Configuration);
agent.Attach(taskManager);
app.MapA2A(taskManager, "/");
app.MapWellKnownAgentCard(taskManager, "/");
app.MapHttpA2A(taskManager, "/");

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

await app.RunAsync();