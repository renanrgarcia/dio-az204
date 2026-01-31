using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace fnPostDatabase;

public class Function1(ILogger<Function1> logger)
{
    private readonly ILogger<Function1> _logger = logger;

    [Function("movie")]
    [CosmosDBOutput(
        databaseName: "%DatabaseName%",
        containerName: "movies",
        Connection = "CosmosDBConnection",
        CreateIfNotExists = true)]
    public async Task<object?> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        var content = await new StreamReader(req.Body).ReadToEndAsync();

        MovieRequest movieRequest;
        try
        {
            movieRequest = JsonConvert.DeserializeObject<MovieRequest>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deserializing request body: {Message}", ex.Message);
            return new BadRequestObjectResult("Invalid request body");
        }

        return JsonConvert.SerializeObject(movieRequest);
    }
}