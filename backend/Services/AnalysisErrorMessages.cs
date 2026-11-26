using StajFlow.Api.Entities;

namespace StajFlow.Api.Services;

public static class AnalysisErrorMessages
{
    public static string? Get(AnalysisErrorCode? code) => code switch
    {
        null => null,
        AnalysisErrorCode.FileNotFound => "Özgeçmiş dosyası bulunamadı.",
        AnalysisErrorCode.InvalidPdf => "PDF geçersiz veya bozuk.",
        AnalysisErrorCode.EncryptedPdf => "Şifreli PDF analiz edilemez. Parola koruması olmayan bir PDF yükleyin.",
        AnalysisErrorCode.NoText => "PDF'de seçilebilir metin bulunamadı. Görüntü tabanlı belgeler analiz edilemiyor.",
        AnalysisErrorCode.UnreadableText => "PDF'den okunabilir metin çıkarılamadı.",
        AnalysisErrorCode.FileTooLarge => "PDF, özgeçmiş dosya boyutu sınırını aşıyor.",
        AnalysisErrorCode.ReadFailed => "Özgeçmiş dosyası okunamadı.",
        AnalysisErrorCode.ConfigurationError => "Analiz hizmeti yapılandırılmamış veya yapılandırması geçersiz.",
        AnalysisErrorCode.AuthenticationFailed => "Analiz hizmetinin API anahtarı veya erişim yetkisi geçersiz.",
        AnalysisErrorCode.ModelUnavailable => "Seçilen analiz modeline erişilemiyor. Başka modele geçilmedi.",
        AnalysisErrorCode.QuotaExceeded => "Analiz kullanım sınırına ulaşıldı. Kota yenilendiğinde yeniden deneyebilirsiniz.",
        AnalysisErrorCode.InputTooLong => "Analiz metni veya gerekli yanıt bütçesi, yapılandırılmış metin/token sınırını aşıyor. Metin kesilmeden analiz durduruldu.",
        AnalysisErrorCode.InvalidResponse => "Model yanıtı beklenen ortak analiz şemasına uygun değil.",
        AnalysisErrorCode.ContentBlocked => "Analiz hizmeti bu içerik için yanıt üretmedi.",
        AnalysisErrorCode.Cancelled => "Analiz uygulama kapanırken kesildi. Yeniden analiz isteyebilirsiniz.",
        AnalysisErrorCode.ProviderUnavailable => "Analiz hizmetine şu anda erişilemiyor. Daha sonra yeniden deneyin.",
        _ => "Analiz tamamlanamadı. Daha sonra yeniden deneyin."
    };
}
