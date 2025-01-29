using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using Azure.Storage.Blobs;
using System.Text.RegularExpressions;

namespace EventFunction;

public class EventFunction
{
    private readonly ILogger<EventFunction> _logger;
    private readonly string _connectionString;

    public EventFunction(ILogger<EventFunction> logger)
    {
        _logger = logger;
        _connectionString = Environment.GetEnvironmentVariable("StorageConnectionString", EnvironmentVariableTarget.Process) ?? string.Empty;
    }

    [Function("EventFileTrigger")]
    public async Task EventFileTrigger(
       [BlobTrigger("events/{name}.pdf",
       Connection = "StorageConnectionString")] Stream pdfStream,
       string name)
    {
        try
        {
            _logger.LogInformation("Start EventFileTrigger");
            
            // we are uploading a .txt file which will trigger this again, so we will ignore it
            if (name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation($"Skipping {name}");
                return;
            }

            var pdfText = ExtractTextFromPdf(pdfStream);

            // *** TMEventId ***
            var tmEventId = Regex.Match(name, @"(?<=_)(\d+)(?=\.pdf$)").Value;

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

    private string ExtractTextFromPdf(Stream pdfStream)
    {
        var text = new StringBuilder();

        using (var pdfReader = new PdfReader(pdfStream))
        using (var pdfDoc = new PdfDocument(pdfReader))
        {
            var numberOfPages = pdfDoc.GetNumberOfPages();
            for (int pageNumber = 1; pageNumber <= numberOfPages; pageNumber++)
            {
                var page = pdfDoc.GetPage(pageNumber);
                var strategy = new SimpleTextExtractionStrategy();
                var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                text.Append(pageText);
            }
        }

        return text.ToString();
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