using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace fnGetMovieDetail;

public class Function1(ILogger<Function1> logger, CosmosClient cosmosClient)
{
    private readonly ILogger<Function1> _logger = logger;
    private readonly CosmosClient _cosmosClient = cosmosClient;

    [Function("detail")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var container = _cosmosClient.GetContainer("FlixDB", "movies");

        var id = req.Query["id"];
        var query = $"SELECT * FROM c WHERE c.id = @id";
        var queryDefinition = new QueryDefinition(query).WithParameter("@id", id);
        var result = container.GetItemQueryIterator<MovieResponse>(queryDefinition);
        var results = new List<MovieResponse>();
        while (result.HasMoreResults)
        {
            foreach (var item in await result.ReadNextAsync())
            {
                results.Add(item);
            }
        }

        var responseMessage = req.CreateResponse(HttpStatusCode.OK);
        await responseMessage.WriteAsJsonAsync(results.FirstOrDefault());

        return responseMessage;
    }
}