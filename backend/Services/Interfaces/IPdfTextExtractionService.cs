namespace StajFlow.Api.Services.Interfaces;

public interface IPdfTextExtractionService
{
    Task<PdfTextExtractionResult> ExtractAsync(Stream content, CancellationToken cancellationToken);
}
