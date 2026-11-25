using StajFlow.Api.DTOs.Analysis;
using StajFlow.Api.Entities;

namespace StajFlow.Api.Services;

public sealed class CompatibilityScorer
{
    public const string RuleVersion = "1";

    public CompatibilityScoringResult Evaluate(ResumeAnalysisData resume, PostingAnalysisData posting)
    {
        var criteria = posting.Criteria.Select(item => EvaluateCriterion(resume, item)).ToList();
        var missing = criteria.Where(item => item.Type == RequirementType.Required && item.MatchStatus != CriterionMatchStatus.Matched)
            .Select(item => item.Description).Distinct(StringComparer.Ordinal).ToArray();
        var data = new ApplicationEvaluationData
        {
            ResumeAnalysisStatus = AnalysisStatus.Completed,
            PostingAnalysisStatus = AnalysisStatus.Completed,
            Criteria = criteria,
            MandatoryWarnings = missing.Length == 0 ? [] :
                [new() { Message = $"Zorunlu kriterlerin tamamı CV'de doğrulanamadı: {string.Join(", ", missing)}." }],
            SocialIndicators = posting.SocialExpectations.Select(item => EvaluateSocial(resume, item)).ToList()
        };
        return criteria.Count == 0
            ? new(null, EvaluationUnavailableReason.NoMeasurableCriteria, data)
            : new(Score(criteria), null, data);
    }

    private static decimal Score(List<CriterionEvaluationData> criteria)
    {
        var preferred = criteria.Where(item => item.Type == RequirementType.Preferred).ToList();
        var requiredGroups = criteria.Where(item => item.Type == RequirementType.Required).GroupBy(item => item.Category).ToList();
        var preferredPool = preferred.Count == 0 ? 0m : requiredGroups.Count == 0 ? 100m : 10m;
        var score = preferred.Count == 0 ? 0m : preferredPool * preferred.Count(Matched) / preferred.Count;
        if (requiredGroups.Count > 0)
        {
            var requiredPool = 100m - preferredPool;
            var totalWeight = requiredGroups.Sum(group => BaseWeight(group.Key, preferred.Count > 0));
            var matchedWeight = requiredGroups.Sum(group =>
                BaseWeight(group.Key, preferred.Count > 0) * group.Count(Matched) / group.Count());
            score += requiredPool * matchedWeight / totalWeight;
        }
        return Math.Round(Math.Clamp(score, 0m, 100m), 2, MidpointRounding.AwayFromZero);
    }

    private static bool Matched(CriterionEvaluationData item) => item.MatchStatus == CriterionMatchStatus.Matched;

    private static decimal BaseWeight(CriterionCategory category, bool hasPreferred) => category switch
    {
        CriterionCategory.TechnicalSkill => hasPreferred ? 40m : 45m,
        CriterionCategory.Project => 25m,
        CriterionCategory.Experience => 15m,
        CriterionCategory.Education => hasPreferred ? 10m : 15m,
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };

    private static CriterionEvaluationData EvaluateCriterion(ResumeAnalysisData resume, PostingCriterionData criterion)
    {
        var evidence = criterion.Category switch
        {
            CriterionCategory.TechnicalSkill => MatchTechnicalSkill(resume, criterion),
            CriterionCategory.Project => resume.Projects.Where(item =>
                    MatchesTechnologies(criterion, item.Technologies, Quoted(item.Evidence)) && MatchesTopic(criterion.ProjectTopic, item))
                .Select(item => Quoted(item.Evidence)).FirstOrDefault(items => items.Count > 0) ?? [],
            CriterionCategory.Experience => resume.Experiences.Where(item => MatchesExperience(criterion, item))
                .Select(item => Quoted(item.Evidence)).FirstOrDefault(items => items.Count > 0) ?? [],
            CriterionCategory.Education => resume.Education.Where(item => MatchesEducation(criterion, item))
                .Select(item => Quoted(item.Evidence)).FirstOrDefault(items => items.Count > 0) ?? [],
            _ => []
        };
        return new()
        {
            CriterionId = criterion.Id,
            Category = criterion.Category,
            Type = criterion.Type,
            Description = criterion.Description,
            MatchStatus = evidence.Count > 0 ? CriterionMatchStatus.Matched : CriterionMatchStatus.NoEvidence,
            PostingEvidence = criterion.Evidence,
            Evidence = evidence
        };
    }

    private static List<EvidenceData> MatchTechnicalSkill(ResumeAnalysisData resume, PostingCriterionData criterion)
    {
        var facts = resume.TechnicalSkills.Select(item => (Technologies: (IReadOnlyList<string>)[item.Name], Evidence: Quoted(item.Evidence)))
            .Concat(resume.Projects.Select(item => (Technologies: (IReadOnlyList<string>)item.Technologies, Evidence: Quoted(item.Evidence))))
            .Concat(resume.Experiences.Select(item => (Technologies: (IReadOnlyList<string>)item.Technologies, Evidence: Quoted(item.Evidence))))
            .ToList();
        var matches = criterion.Technologies.Select(required => facts
            .SelectMany(fact => TechnologyEvidence(required, fact.Technologies, fact.Evidence)).ToList()).ToList();
        if (!criterion.AnyTechnology && matches.Any(items => items.Count == 0)) return [];
        return matches.SelectMany(items => items).DistinctBy(item => (item.SourceQuote, item.SourcePage)).ToList();
    }

    private static bool MatchesTechnologies(PostingCriterionData criterion, IReadOnlyList<string> technologies, List<EvidenceData> evidence)
    {
        bool Matches(string required) => TechnologyEvidence(required, technologies, evidence).Any();
        return criterion.AnyTechnology ? criterion.Technologies.Any(Matches) : criterion.Technologies.All(Matches);
    }

    private static IEnumerable<EvidenceData> TechnologyEvidence(string required, IReadOnlyList<string> technologies, List<EvidenceData> evidence) =>
        evidence.Where(item => technologies.Any(value => CompatibilityTerms.TechnologyMatches(value, required) &&
            CompatibilityTerms.HasTechnology(item.SourceQuote!, value)));

    private static bool MatchesTopic(string? topic, ResumeProjectData project) => topic is null ||
        (CompatibilityTerms.HasTerm(project.Name, topic) ||
            (project.Description is not null && CompatibilityTerms.HasTerm(project.Description, topic))) &&
        Quoted(project.Evidence).Any(item => CompatibilityTerms.HasTerm(item.SourceQuote!, topic));

    private static bool MatchesExperience(PostingCriterionData criterion, ResumeExperienceData experience)
    {
        if (criterion.ExperienceType is { } type && type != ExperienceType.Unspecified && experience.Type != type) return false;
        if (criterion.ExperienceRole is { } role && (experience.Role is null ||
            CompatibilityTerms.Normalize(experience.Role) != CompatibilityTerms.Normalize(role))) return false;
        if (criterion.MinimumDurationMonths is { } months &&
            (experience.DurationMonths is null || experience.DurationMonths < months ||
                string.IsNullOrWhiteSpace(experience.DurationText))) return false;
        return MatchesTechnologies(criterion, experience.Technologies, Quoted(experience.Evidence));
    }

    private static bool MatchesEducation(PostingCriterionData criterion, ResumeEducationData education)
    {
        if (criterion.FieldOfStudy is { } field && (education.FieldOfStudy is null ||
            CompatibilityTerms.Education(education.FieldOfStudy) != CompatibilityTerms.Education(field))) return false;
        if (criterion.EducationLevel is { } level && !MatchesLevel(level, education)) return false;
        if (criterion.MinimumStudyYear is { } minimum && (education.StudyYear is null || education.StudyYear < minimum)) return false;
        return criterion.MaximumStudyYear is not { } maximum || (education.StudyYear is { } year && year <= maximum);
    }

    private static bool MatchesLevel(string expected, ResumeEducationData education)
    {
        var level = CompatibilityTerms.Education(expected);
        if (education.Level is not null && CompatibilityTerms.Education(education.Level) == level) return true;
        return level == "student" && Quoted(education.Evidence).Any(item =>
            new[] { "student", "öğrenci", "öğrencisi", "öğrenciyim" }.Any(term => CompatibilityTerms.HasTerm(item.SourceQuote!, term)));
    }

    private static SocialIndicatorData EvaluateSocial(ResumeAnalysisData resume, SocialExpectationData expectation)
    {
        var match = resume.SocialIndicators.Where(item => CompatibilityTerms.SocialMatches(item.Name, expectation.Name) &&
                item.EvidenceStatus != EvidenceStatus.NoEvidence && item.Evidence.Any(IsQuoted))
            .OrderBy(item => item.EvidenceStatus == EvidenceStatus.EvidenceFound ? 0 : 1).FirstOrDefault();
        return new()
        {
            Name = expectation.Name,
            EvidenceStatus = match?.EvidenceStatus ?? EvidenceStatus.NoEvidence,
            Evidence = match is null ? [] : Quoted(match.Evidence)
        };
    }

    private static bool IsQuoted(EvidenceData item) => !string.IsNullOrWhiteSpace(item.SourceQuote);
    private static List<EvidenceData> Quoted(List<EvidenceData> items) => items.Where(IsQuoted).ToList();
}

public sealed record CompatibilityScoringResult(decimal? Score, EvaluationUnavailableReason? UnavailableReason,
    ApplicationEvaluationData Data);
