using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using StajFlow.Api.Configurations;
using StajFlow.Api.Services.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig.Exceptions;

namespace StajFlow.Api.Services;

public class PdfTextExtractionService(IOptions<ResumeStorageOptions> options) : IPdfTextExtractionService
{
    public async Task<PdfTextExtractionResult> ExtractAsync(Stream content, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        cancellationToken.ThrowIfCancellationRequested();
        if (!content.CanRead) return new(PdfTextExtractionStatus.ReadFailed);

        using var buffer = new MemoryStream();
        try
        {
            var chunk = new byte[81920];
            int count;
            while ((count = await content.ReadAsync(chunk, cancellationToken)) > 0)
            {
                if (buffer.Length + count > options.Value.MaxFileSizeBytes)
                    return new(PdfTextExtractionStatus.FileTooLarge);
                await buffer.WriteAsync(chunk.AsMemory(0, count), cancellationToken);
            }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ObjectDisposedException or NotSupportedException)
        {
            return new(PdfTextExtractionStatus.ReadFailed);
        }

        if (!buffer.GetBuffer().AsSpan(0, (int)buffer.Length).StartsWith("%PDF-"u8))
            return new(PdfTextExtractionStatus.InvalidPdf);

        cancellationToken.ThrowIfCancellationRequested();
        buffer.Position = 0;
        try
        {
            using var document = PdfDocument.Open(buffer, new ParsingOptions
            {
                UseLenientParsing = false,
                SkipMissingFonts = false
            });
            if (document.IsEncrypted) return new(PdfTextExtractionStatus.EncryptedPdf);
            if (document.NumberOfPages < 1) return new(PdfTextExtractionStatus.InvalidPdf);

            var extracted = new StringBuilder();
            for (var number = 1; number <= document.NumberOfPages; number++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var pageText = ContentOrderTextExtractor.GetText(document.GetPage(number));
                cancellationToken.ThrowIfCancellationRequested();
                extracted.AppendLine(pageText);
                extracted.AppendLine();
            }

            var text = NormalizeText(extracted.ToString());
            if (string.IsNullOrWhiteSpace(text)) return new(PdfTextExtractionStatus.NoText);
            if (!HasReadableText(text)) return new(PdfTextExtractionStatus.UnreadableText);
            return new(PdfTextExtractionStatus.Succeeded, text);
        }
        catch (PdfDocumentEncryptedException)
        {
            return new(PdfTextExtractionStatus.EncryptedPdf);
        }
        catch (Exception error) when (error is not OperationCanceledException and not OutOfMemoryException)
        {
            return new(PdfTextExtractionStatus.InvalidPdf);
        }
    }

    private static string NormalizeText(string text)
    {
        text = text.Replace("\r\n", "\n").Replace('\r', '\n');
        text = Regex.Replace(text, @"[^\S\n]+", " ");
        text = Regex.Replace(text, @" *\n *", "\n");
        text = Regex.Replace(text, @"\n{3,}", "\n\n");
        return text.Trim().Normalize(NormalizationForm.FormC);
    }

    private static bool HasReadableText(string text)
    {
        var letters = 0;
        var unreadable = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            if (Rune.IsLetter(rune)) letters++;
            else if (rune.Value == 0xfffd || Rune.GetUnicodeCategory(rune) is
                UnicodeCategory.PrivateUse or UnicodeCategory.OtherNotAssigned or UnicodeCategory.Surrogate ||
                (Rune.IsControl(rune) && !Rune.IsWhiteSpace(rune)))
                unreadable++;
        }
        return letters > unreadable && Regex.IsMatch(text, @"\p{L}[\p{L}\p{M}]+", RegexOptions.CultureInvariant);
    }
}
