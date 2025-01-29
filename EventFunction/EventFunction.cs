using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using Azure.Storage.Blobs;
using System.Text.RegularExpressions;
using Azure.AI.DocumentIntelligence;
using Azure;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Specialized;

namespace EventFunction;

public class EventFunction
{
    private readonly ILogger<EventFunction> _logger;
    private readonly string _connectionString;
    private readonly string _docIntelKey;
    private readonly string _docIntelEndpoint;

    public EventFunction(ILogger<EventFunction> logger)
    {
        _logger = logger;
        _connectionString = Environment.GetEnvironmentVariable("StorageConnectionString", EnvironmentVariableTarget.Process);
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new ArgumentException("StorageConnectionString environment variable is null or empty.");
        }

        _docIntelKey = Environment.GetEnvironmentVariable("DocumentIntelligenceKey", EnvironmentVariableTarget.Process);
        if (string.IsNullOrEmpty(_docIntelKey))
        {
            throw new ArgumentException("DocumentIntelligenceKey environment variable is null or empty.");
        }

        _docIntelEndpoint = Environment.GetEnvironmentVariable("DocumentIntelligenceEndpoint", EnvironmentVariableTarget.Process);
        if (string.IsNullOrEmpty(_docIntelEndpoint))
        {
            throw new ArgumentException("DocumentIntelligenceEndpoint environment variable is null or empty.");
        }
    }

    [Function("EventFileTrigger")]
    public async Task EventFileTrigger(
       [BlobTrigger("events/{name}.pdf",
       Connection = "StorageConnectionString")] Stream pdfStream,
       string name)
    {
        try
        {
            // *** TMEventId ***
            var tmEventId = Regex.Match(name, @"_(\d+)$").Groups[1].Value;

            _logger.LogInformation("Start EventFileTrigger");

            var pdfText = await ExtractTextFromPdfAsync(pdfStream);

            _logger.LogInformation($"Extracted PDF text and creating {tmEventId}.txt");

            await UploadPdfTextAsync(tmEventId, pdfText);

            _logger.LogInformation($"Blob trigger function Processed blob\n Name: {name} \n Size: {pdfStream.Length} Bytes");
            _logger.LogInformation("End EventFileTrigger");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

    private async Task<string> ExtractTextFromPdfAsync(Stream pdfStream)
    {
        var text = new StringBuilder();
        var credential = new AzureKeyCredential(_docIntelKey);
        var client = new DocumentIntelligenceClient(new Uri(_docIntelEndpoint), credential);

        try
        {
            BinaryData binaryData = BinaryData.FromStream(pdfStream);
            Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", binaryData);
            AnalyzeResult result = operation.Value;

            foreach (var page in result.Pages)
            {
                text.AppendLine($"Page {page.PageNumber}:");

                foreach (var line in page.Lines)
                {
                    text.AppendLine(line.Content);
                }
            }
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex.Message);
        }

        return text.ToString();
    }

    private async Task<Uri> GetBlobUrlAsync(string pdfName)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient($"events");
        await containerClient.CreateIfNotExistsAsync();
        var blobClient = containerClient.GetBlobClient(pdfName);
        return blobClient.Uri;
    }

    private async Task UploadPdfTextAsync(string tmEventId, string pdfText)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient($"events");
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient($"{tmEventId}/{tmEventId}.txt");

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(pdfText));
        await blobClient.UploadAsync(stream, true);
    }
}