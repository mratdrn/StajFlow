using StajFlow.Api.DTOs.Analysis;
using StajFlow.Api.Entities;

namespace StajFlow.Api.Services;

public sealed class CompatibilityScorer
{
    public const string RuleVersion = "3";
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.Ordinal)
    {
        ["csharp"] = "c#", ["js"] = "javascript", ["mssql"] = "sqlserver", ["mssqlserver"] = "sqlserver",
        ["excel"] = "microsoftexcel", ["msexcel"] = "microsoftexcel", ["postgres"] = "postgresql",
        ["dotnet"] = "net", ["netcore"] = "net", ["reactjs"] = "react"
    };

    private static string Normalize(string value)
    {
        var key = new string(value.ToLowerInvariant().Where(character => char.IsLetterOrDigit(character) || character is '#' or '+').ToArray())
            .Replace('ı', 'i');
        return Aliases.GetValueOrDefault(key, key);
    }

    private static HashSet<string> Set(IEnumerable<string> values)
    {
        var set = values.Select(Normalize).ToHashSet(StringComparer.Ordinal);
        if (set.Contains("microsoftoffice") || set.Contains("msoffice")) set.Add("microsoftexcel");
        if (set.Contains("sqlserver") || set.Contains("postgresql") || set.Contains("mysql")) set.Add("sql");
        return set;
    }

    public static int Months(IEnumerable<MatchingPeriod> periods, DateOnly today)
    {
        var now = today.Year * 12 + today.Month;
        var months = new HashSet<int>();
        foreach (var period in periods)
        {
            if (!MatchingProfileContract.TryMonth(period.Start, out var start)) continue;
            var first = start.Year * 12 + start.Month;
            var last = now;
            if (period.End != "Present")
            {
                if (!MatchingProfileContract.TryMonth(period.End, out var end)) continue;
                last = end.Year * 12 + end.Month;
            }
            for (var month = first; month <= Math.Min(last, now); month++) months.Add(month);
        }
        return months.Count;
    }

    public CompatibilityScoringResult Evaluate(MatchingProfile resume, MatchingProfile posting, DateOnly today)
    {
        var criteria = new List<CriterionEvaluationData>();
        var skills = Set(resume.Skills.SelectMany(skill => skill.Names));
        double weighted = 0, available = 0;
        void Add(double weight, double ratio) { weighted += weight * ratio; available += weight; }
        void Criterion(string id, CriterionCategory category, bool required, string description, bool matched, string? basis)
        {
            criteria.Add(new()
            {
                CriterionId = id, Category = category, Type = required ? RequirementType.Required : RequirementType.Preferred,
                Description = description, MatchStatus = matched ? CriterionMatchStatus.Matched : CriterionMatchStatus.NoEvidence,
                Evidence = basis is null ? [] : [new() { Text = basis }]
            });
        }
        void AddSkills(double weight, bool required)
        {
            var list = posting.Skills.Where(skill => skill.Required == required).ToList();
            if (list.Count == 0) return;
            var matchedCount = 0;
            for (var index = 0; index < list.Count; index++)
            {
                var skill = list[index];
                var match = skill.Names.FirstOrDefault(name => skills.Contains(Normalize(name)));
                if (match is not null) matchedCount++;
                Criterion($"skill-{required}-{index}", CriterionCategory.TechnicalSkill, required, string.Join(" veya ", skill.Names),
                    match is not null, match is null ? null : $"Yapılandırılmış CV becerileriyle eşleşti: {match}.");
            }
            Add(weight, (double)matchedCount / list.Count);
        }
        AddSkills(45, true);
        AddSkills(15, false);
        if (posting.Departments.Count > 0)
        {
            var matched = Set(posting.Departments).Overlaps(Set(resume.Departments));
            Add(20, matched ? 1 : 0);
            Criterion("department", CriterionCategory.Education, true, $"Bölüm: {string.Join(" veya ", posting.Departments)}", matched,
                matched ? $"CV bölümleri: {string.Join(", ", resume.Departments)}." : null);
        }
        if (posting.MinExperienceMonths is > 0)
        {
            var months = Months(resume.Experiences, today);
            Add(15, Math.Min(1.0, (double)months / posting.MinExperienceMonths.Value));
            Criterion("experience", CriterionCategory.Experience, true, $"En az {posting.MinExperienceMonths} ay deneyim",
                months >= posting.MinExperienceMonths.Value, $"CV tarih aralıkları: çakışan aylar bir kez sayılarak {months} ay. Alan ayrımı yapılmaz; süre eksikse oransal puan uygulanır.");
        }
        var actualSocial = Set(resume.SoftSkills);
        var wantedSocial = Set(posting.SoftSkills);
        if (wantedSocial.Count > 0) Add(5, (double)wantedSocial.Count(actualSocial.Contains) / wantedSocial.Count);
        var missing = criteria.Where(item => item.Type == RequirementType.Required && item.MatchStatus != CriterionMatchStatus.Matched)
            .Select(item => item.Description).ToArray();
        var data = new ApplicationEvaluationData
        {
            ResumeAnalysisStatus = AnalysisStatus.Completed, PostingAnalysisStatus = AnalysisStatus.Completed, Criteria = criteria,
            MandatoryWarnings = missing.Length == 0 ? [] : [new() { Message = $"Zorunlu kriterlerin tamamı CV'de doğrulanamadı: {string.Join(", ", missing)}." }],
            SocialIndicators = posting.SoftSkills.Distinct(StringComparer.Ordinal).Select(name => new SocialIndicatorData
            {
                Name = name, EvidenceStatus = actualSocial.Contains(Normalize(name)) ? EvidenceStatus.DeclarationOnly : EvidenceStatus.NoEvidence,
                Evidence = actualSocial.Contains(Normalize(name)) ? [new() { Text = "Yapılandırılmış CV sosyal beceri listesinde yer alıyor; bu şema somut faaliyet kanıtı içermez." }] : []
            }).ToList()
        };
        return available == 0 ? new(null, EvaluationUnavailableReason.NoMeasurableCriteria, data)
            : new((decimal)Math.Round(100 * weighted / available, MidpointRounding.AwayFromZero), null, data);
    }
}

public sealed record CompatibilityScoringResult(decimal? Score, EvaluationUnavailableReason? UnavailableReason,
    ApplicationEvaluationData Data);
