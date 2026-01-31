using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

//builder.Services
//    .AddApplicationInsightsTelemetryWorkerService()
//    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton(s =>
{
    var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnection");
    var cosmosClientOptions = new CosmosClientOptions
    {
        ConnectionMode = ConnectionMode.Gateway,
        RequestTimeout = TimeSpan.FromSeconds(30)
    };
    return new CosmosClient(connectionString, cosmosClientOptions);
});

builder.Build().Run();
