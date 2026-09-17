using BlazorDemo.ItsmAiAgent.Api.Plugins;
using Microsoft.Azure.Cosmos;
using Microsoft.SemanticKernel;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 1. CORS - pozwalamy Blazorowi uderzać do naszego API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllForDemo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var aoaiKey = builder.Configuration["AzureOpenAI:Key"]!;
var aoaiEndpoint = new Uri(builder.Configuration["AzureOpenAI:Endpoint"]!);
var chatDeployment = builder.Configuration["AzureOpenAI:ChatDeployment"]!;
var embeddingDeployment = builder.Configuration["AzureOpenAI:EmbeddingDeployment"]!;

builder.Services.AddKernel();

builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: chatDeployment,
    endpoint: aoaiEndpoint.ToString(),
    apiKey: aoaiKey
);

#pragma warning disable SKEXP0001
builder.Services.AddAzureOpenAITextEmbeddingGeneration(
    deploymentName: embeddingDeployment,
    endpoint: aoaiEndpoint.ToString(),
    apiKey: aoaiKey
);
#pragma warning restore SKEXP0001

// 2. Rate Limiting (5 zapytań na minutę na dane IP)
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("ChatRateLimit", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// 3. Rejestracja Cosmos DB
builder.Services.AddSingleton(sp =>
    new CosmosClient(builder.Configuration["CosmosDb:ConnectionString"], new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    }));

// 4. Rejestracja Pluginu ITSM
builder.Services.AddScoped<TicketPlugin>();

var app = builder.Build();

app.UseHttpsRedirection();

// WAŻNE: Kolejność tych trzech linijek ma znaczenie w ASP.NET Core!
app.UseCors("AllowAllForDemo");
app.UseRateLimiter(); // <-- TEGO BRAKOWAŁO!
app.UseAuthorization();

app.MapControllers();

app.Run();