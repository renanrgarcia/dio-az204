using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace fnPostDataStorage;

public class Function1(ILogger<Function1> logger)
{
    private readonly ILogger<Function1> _logger = logger;

    [Function("dataStorage")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("Processing Image on Storage");

        if (!req.Headers.TryGetValue("file-type", out var fileTypeHeader))
        {
            return new BadRequestObjectResult("Missing required 'file-type' header.");
        }

        var fileType = fileTypeHeader.ToString().ToLower();
        var form = await req.ReadFormAsync();
        var file = form.Files["file"];

        if (file == null || file.Length == 0)
        {
            return new BadRequestObjectResult("No file uploaded.");
        }

        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var containerName = fileType;
        BlobClient blobClient = new BlobClient(connectionString, containerName, file.FileName);
        BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);

        await containerClient.CreateIfNotExistsAsync();
        await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

        string blobName = file.FileName;
        var blob = containerClient.GetBlobClient(blobName);

        using (var stream = file.OpenReadStream())
        {
            await blob.UploadAsync(stream, true);
        }

        return new OkObjectResult(new
        {
            message = $"File '{file.FileName}' uploaded successfully to container '{containerName}'.",
            blobUrl = blob.Uri.ToString()
        });
    }
}