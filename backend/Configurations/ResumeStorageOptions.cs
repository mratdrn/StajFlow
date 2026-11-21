namespace StajFlow.Api.Configurations;

public class ResumeStorageOptions
{
    public const string SectionName = "ResumeStorage";
    public string Directory { get; set; } = "../ozel-veriler/ozgecmisler";
    public int MaxFileSizeMb { get; set; }
    public long MaxFileSizeBytes => MaxFileSizeMb * 1024L * 1024L;
    public string GetStoragePath(string contentRoot, string key)
    {
        if (key.Length != 36 || !key.EndsWith(".pdf", StringComparison.Ordinal) ||
            !Guid.TryParseExact(key[..32], "N", out _))
            throw new InvalidOperationException("Özgeçmiş depolama anahtarı geçersiz.");
        return Path.Combine(Path.GetFullPath(Directory, contentRoot), key);
    }
}
