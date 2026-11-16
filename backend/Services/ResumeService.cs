using StajFlow.Api.Configurations;
using StajFlow.Api.Data;
using StajFlow.Api.DTOs;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UglyToad.PdfPig;

namespace StajFlow.Api.Services;

public class ResumeService(
    ApplicationDbContext context, IOptions<ResumeStorageOptions> options,
    IWebHostEnvironment environment, ILogger<ResumeService> logger) : IResumeService
{
    private readonly ResumeStorageOptions settings = options.Value;

    public async Task<IReadOnlyList<ResumeDto>> ListAsync(string userId, CancellationToken cancellationToken) =>
        await context.Resumes.AsNoTracking()
            .Where(resume => resume.StudentProfile.IdentityUserId == userId)
            .OrderByDescending(resume => resume.UploadedAt).ThenByDescending(resume => resume.Id)
            .Select(resume => new ResumeDto(resume.Id, resume.OriginalFileName, resume.FileSizeBytes, resume.UploadedAt))
            .ToListAsync(cancellationToken);

    public async Task<ResumeUploadResult> UploadAsync(string userId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0) return new(null, "Boş dosya yüklenemez.");
        if (file.Length > settings.MaxFileSizeBytes)
            return new(null, $"PDF dosyası en fazla {settings.MaxFileSizeMb} MB olabilir.");

        var name = Path.GetFileName(file.FileName.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(name) || name.Length > 255 || name.Any(char.IsControl) ||
            !string.Equals(Path.GetExtension(name), ".pdf", StringComparison.OrdinalIgnoreCase))
            return new(null, "Geçerli bir .pdf dosyası seçin; dosya adı en fazla 255 karakter olabilir.");
        if (!string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            return new(null, "Yalnızca PDF dosyaları yüklenebilir.");

        var studentId = await context.StudentProfiles.AsNoTracking()
            .Where(student => student.IdentityUserId == userId)
            .Select(student => (int?)student.Id).SingleOrDefaultAsync(cancellationToken);
        if (studentId is null) return new(null, ProfileNotFound: true);

        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken);
        var content = buffer.ToArray();
        if (content.LongLength != file.Length || !content.AsSpan().StartsWith("%PDF-"u8))
            return new(null, "Dosya geçerli bir PDF değil.");
        try
        {
            using var document = PdfDocument.Open(content, new ParsingOptions { UseLenientParsing = false });
            if (document.NumberOfPages < 1) return new(null, "PDF en az bir sayfa içermelidir.");
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return new(null, "PDF okunamadı. Bozuk veya parola korumalı olmayan bir PDF yükleyin.");
        }

        var key = $"{Guid.NewGuid():N}.pdf";
        var path = GetStoragePath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var resume = new Resume
        {
            StudentProfileId = studentId.Value, OriginalFileName = name, StorageKey = key,
            ContentType = "application/pdf", FileSizeBytes = content.LongLength
        };
        var fileCreated = false;
        try
        {
            await using (var target = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                fileCreated = true;
                await target.WriteAsync(content, cancellationToken);
            }
            context.Resumes.Add(resume);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            try { if (fileCreated) File.Delete(path); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            { logger.LogError(error, "Kaydedilemeyen özgeçmiş dosyası temizlenemedi."); }
            throw;
        }
        return new(new(resume.Id, name, resume.FileSizeBytes, resume.UploadedAt));
    }

    public async Task<ResumeFile?> GetFileAsync(string userId, int id, CancellationToken cancellationToken)
    {
        var resume = await context.Resumes.AsNoTracking().SingleOrDefaultAsync(
            resume => resume.Id == id && resume.StudentProfile.IdentityUserId == userId, cancellationToken);
        return OpenFile(resume);
    }

    public async Task<ResumeFile?> GetCompanyApplicationFileAsync(string userId, int applicationId, CancellationToken cancellationToken)
    {
        var resume = await context.Applications.AsNoTracking()
            .Where(application => application.Id == applicationId && application.InternshipPosting.CompanyProfile.IdentityUserId == userId)
            .Select(application => application.Resume).SingleOrDefaultAsync(cancellationToken);
        return OpenFile(resume);
    }

    private ResumeFile? OpenFile(Resume? resume)
    {
        if (resume is null) return null;
        try
        {
            return new(new FileStream(GetStoragePath(resume.StorageKey), FileMode.Open, FileAccess.Read,
                FileShare.Read, 81920, useAsync: true), resume.OriginalFileName);
        }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
    }

    private string GetStoragePath(string key)
    {
        if (key.Length != 36 || !key.EndsWith(".pdf", StringComparison.Ordinal) ||
            !Guid.TryParseExact(key[..32], "N", out _))
            throw new InvalidOperationException("Özgeçmiş depolama anahtarı geçersiz.");
        return Path.Combine(Path.GetFullPath(settings.Directory, environment.ContentRootPath), key);
    }
}
