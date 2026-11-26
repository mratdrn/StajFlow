using StajFlow.Api.Entities;

namespace StajFlow.Api.Services;

public static class PostingDescription
{
    public static string Compose(string description, IEnumerable<(string Description, RequirementType Type)> requirements)
    {
        var text = description.Trim();
        var items = requirements.ToList();
        foreach (var type in new[] { RequirementType.Required, RequirementType.Preferred })
        {
            var lines = items.Where(item => item.Type == type).Select(item => item.Description.Trim())
                .Where(item => item.Length > 0).Distinct(StringComparer.Ordinal).ToList();
            if (lines.Count == 0) continue;
            var heading = type == RequirementType.Required ? "Zorunlu nitelikler:" : "Tercih edilen nitelikler:";
            var section = heading + "\n" + string.Join("\n", lines.Select(line => "- " + line));
            if (!text.Contains(section, StringComparison.Ordinal)) text += "\n\n" + section;
        }
        return text;
    }
}
