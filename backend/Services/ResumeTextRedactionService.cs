using System.Text.RegularExpressions;

namespace StajFlow.Api.Services;

public sealed class ResumeTextRedactionService
{
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(1);

    public string Redact(string text, string fullName, string? email, string? phone)
    {
        text = RemoveIdentifiers(text, fullName, email, phone);
        var lines = text.Replace("\r\n", "\n").Split('\n');
        var firstContent = true;
        var references = false;
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].Trim();
            if (line.Length == 0) continue;
            if (IsMatch(line, @"^(?:references|referanslar|referans)\s*:?$")) references = true;
            else if (references && IsMatch(line, @"^(?:projects|projeler|education(?:\s*&\s*training)?|eğitim|work\s*experience|experience|deneyim|technical\s*skills|skills|beceriler|soft\s*skills|languages|diller|certifications|sertifikalar|summary|özet)\s*:?$"))
                references = false;
            if (references) { lines[index] = string.Empty; continue; }
            var personal = IsMatch(line, @"^(?:ad\s*soyad|ad[ıi]?\s*(?:ve\s*)?soyad[ıi]?|full\s*name|name|isim|doğum\s*(?:tarihi|yeri)|date\s*of\s*birth|birth\s*date|nationality|uyruk|medeni\s*durum|marital\s*status|t\.?c\.?\s*(?:kimlik|no)|passport)\s*[:：-]");
            var address = IsMatch(line, @"^(?:açık\s*adres|adres|address|home\s*address)\s*[:：-]") ||
                IsMatch(line, @"\b(?:mahallesi|sokağı|caddesi|street|avenue)\b|\b(?:mah|sok|cad|apt)\.");
            var nameHeader = firstContent && IsMatch(line, @"^\p{L}[\p{L}'’\-]*(?:\s+\p{L}[\p{L}'’\-]*){1,3}$") &&
                !IsMatch(line, @"resume|curriculum|vitae|özgeçmiş|engineer|developer|student|intern|software|data|computer|education|summary|profile|skill|project|experience|technical|yazılım|mühendis|öğrenci|staj|eğitim|profil|proje|deneyim|teknik|bilgisayar|veri");
            if (personal || address || nameHeader) lines[index] = string.Empty;
            firstContent = false;
        }
        return string.Join('\n', lines).Trim();
    }

    public string RemoveIdentifiers(string text, string fullName, string? email, string? phone)
    {
        foreach (var value in new[] { fullName, email, phone }.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            var pattern = @"(?<![\p{L}\p{N}])" + Regex.Escape(value!.Trim()).Replace(@"\ ", @"\s+") + @"(?![\p{L}\p{N}])";
            text = Replace(text, pattern, "[KİMLİK]");
        }
        text = Replace(text, @"[\p{L}\p{N}._%+\-]+@[\p{L}\p{N}.\-]+\.[\p{L}]{2,}", "[EPOSTA]");
        text = Replace(text, @"(?<![\p{L}\d])(?:\+\d(?:[ ().\-]*\d){9,14}|0(?:[ ().\-]*\d){9,10}|\(?\d{3}\)?[ .\-]+\d{3}[ .\-]+(?:\d{4}|\d{2}[ .\-]+\d{2})|\d{10})(?!\d)", "[TELEFON]");
        text = Replace(text, @"(?<!\d)\d{11}(?!\d)", "[KİMLİK]");
        text = Replace(text, @"(?:https?://)?(?:www\.)?(?:linkedin\.com|github\.com|gitlab\.com)/[^\s<>""\[\]]+", "[PROFİL]");
        return text;
    }

    private static bool IsMatch(string text, string pattern) => Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase, MatchTimeout);
    private static string Replace(string text, string pattern, string replacement) => Regex.Replace(text, pattern, replacement, RegexOptions.IgnoreCase, MatchTimeout);
}
