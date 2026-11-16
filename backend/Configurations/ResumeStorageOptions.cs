namespace StajFlow.Api.Configurations;

public class ResumeStorageOptions
{
    public const string SectionName = "ResumeStorage";
    public string Directory { get; set; } = "../ozel-veriler/ozgecmisler";
    public int MaxFileSizeMb { get; set; }
    public long MaxFileSizeBytes => MaxFileSizeMb * 1024L * 1024L;
}
