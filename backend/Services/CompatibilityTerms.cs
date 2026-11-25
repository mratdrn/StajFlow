using System.Text;
using System.Text.RegularExpressions;

namespace StajFlow.Api.Services;

internal static class CompatibilityTerms
{
    private static readonly Dictionary<string, string> TechnologyAliases = new(StringComparer.Ordinal)
    {
        ["c sharp"] = "c#",
        ["sql server"] = "sql",
        ["asp.net core webapi"] = "asp.net core web api"
    };

    private static readonly Dictionary<string, string> TechnologyParents = new(StringComparer.Ordinal)
    {
        ["asp.net core web api"] = "asp.net core",
        ["asp.net core mvc"] = "asp.net core"
    };

    private static readonly Dictionary<string, string> EducationAliases = new(StringComparer.Ordinal)
    {
        ["öğrenci"] = "student",
        ["lisans"] = "bachelor",
        ["bachelor's degree"] = "bachelor",
        ["bachelor degree"] = "bachelor",
        ["bachelor's"] = "bachelor",
        ["undergraduate"] = "bachelor",
        ["ön lisans"] = "associate",
        ["önlisans"] = "associate",
        ["associate degree"] = "associate",
        ["yüksek lisans"] = "master",
        ["master's degree"] = "master",
        ["master's"] = "master",
        ["doktora"] = "phd",
        ["ph.d."] = "phd",
        ["bilgisayar mühendisliği"] = "computer engineering",
        ["yazılım mühendisliği"] = "software engineering",
        ["bilgisayar bilimleri"] = "computer science"
    };

    private static readonly Dictionary<string, string> SocialAliases = new(StringComparer.Ordinal)
    {
        ["takim çalişmasi"] = "teamwork",
        ["ekip çalişmasi"] = "teamwork",
        ["team player"] = "teamwork",
        ["team work"] = "teamwork",
        ["iletişim"] = "communication",
        ["iletişim becerileri"] = "communication",
        ["communication skills"] = "communication",
        ["problem çözme"] = "problem solving",
        ["problem-solving"] = "problem solving"
    };

    internal static string Normalize(string value) =>
        Regex.Replace(value.Normalize(NormalizationForm.FormKC).Replace('İ', 'I').Replace('ı', 'i').Replace('’', '\''), @"\s+", " ")
            .Trim().ToLowerInvariant();

    internal static string Technology(string value) => Canonical(value, TechnologyAliases);
    internal static string Education(string value) => Canonical(value, EducationAliases);
    internal static string Social(string value) => Canonical(value, SocialAliases);

    internal static bool SocialMatches(string candidate, string expected)
    {
        var required = Social(expected);
        if (Social(candidate) == required) return true;
        if (!SocialAliases.ContainsValue(required)) return false;
        return Regex.Split(Normalize(candidate), @"\s+(?:ve|and|&)\s+")
            .Any(part => Social(part) == required);
    }

    internal static bool TechnologyMatches(string candidate, string required)
    {
        var actual = Technology(candidate);
        var expected = Technology(required);
        return actual == expected || TechnologyParents.GetValueOrDefault(actual) == expected;
    }

    internal static bool HasTerm(string source, string term)
    {
        var text = Normalize(source);
        var token = Normalize(term);
        if (token.Length == 0) return false;
        for (var start = text.IndexOf(token, StringComparison.Ordinal); start >= 0;
             start = text.IndexOf(token, start + 1, StringComparison.Ordinal))
        {
            var end = start + token.Length;
            if ((start == 0 || !IsTokenCharacter(text[start - 1])) &&
                (end == text.Length || !IsTokenCharacter(text[end]))) return true;
        }
        return false;
    }

    internal static bool HasTechnology(string source, string value)
    {
        var expected = Technology(value);
        return HasTerm(source, value) || HasTerm(source, expected) ||
            TechnologyAliases.Any(alias => alias.Value == expected && HasTerm(source, alias.Key));
    }

    private static bool IsTokenCharacter(char character) => char.IsLetterOrDigit(character) || character is '#' or '+' or '_';

    private static string Canonical(string value, Dictionary<string, string> aliases)
    {
        var normalized = Normalize(value);
        return aliases.GetValueOrDefault(normalized, normalized);
    }
}
