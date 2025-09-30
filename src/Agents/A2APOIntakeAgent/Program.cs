using A2A;
using A2A.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();
builder.Services.AddHttpClient();

var app = builder.Build();
var httpClient = app.Services.GetRequiredService<HttpClient>();
var taskManager = new TaskManager();
var logger = app.Logger;

var poIntakeAgent = new A2APOIntakeAgent(logger, httpClient, builder.Configuration);
poIntakeAgent.Attach(taskManager);
app.MapA2A(taskManager, "/");
app.MapWellKnownAgentCard(taskManager, "/");
app.MapHttpA2A(taskManager, "/");

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

await app.RunAsync();
