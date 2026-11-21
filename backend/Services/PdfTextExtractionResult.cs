using System.Text.Json.Serialization;

namespace StajFlow.Api.Services;

public enum PdfTextExtractionStatus { Succeeded, InvalidPdf, EncryptedPdf, NoText, UnreadableText, FileTooLarge, ReadFailed }

public sealed class PdfTextExtractionResult
{
    internal PdfTextExtractionResult(PdfTextExtractionStatus status, string? text = null)
    {
        Status = status;
        Text = status == PdfTextExtractionStatus.Succeeded ? text : null;
    }

    public PdfTextExtractionStatus Status { get; }
    public bool Succeeded => Status == PdfTextExtractionStatus.Succeeded;

    [JsonIgnore]
    public string? Text { get; }

    public string? Error => Status switch
    {
        PdfTextExtractionStatus.Succeeded => null,
        PdfTextExtractionStatus.InvalidPdf => "Dosya geçerli bir PDF değil veya PDF içeriği bozuk.",
        PdfTextExtractionStatus.EncryptedPdf => "Şifreli PDF dosyasından metin çıkarılamaz. Parola koruması olmayan bir PDF kullanın.",
        PdfTextExtractionStatus.NoText => "PDF'de seçilebilir metin bulunamadı. Belge boş veya görüntü tabanlı olabilir; metin içeren bir PDF kullanın.",
        PdfTextExtractionStatus.UnreadableText => "PDF'den okunabilir ve anlamlı metin çıkarılamadı. Metni seçilebilen ve okunabilen bir PDF kullanın.",
        PdfTextExtractionStatus.FileTooLarge => "PDF dosyası izin verilen özgeçmiş dosya boyutunu aşıyor.",
        _ => "PDF dosyası okunamadı. Lütfen tekrar deneyin."
    };
}
