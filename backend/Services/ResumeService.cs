using StajFlow.Api.Configurations;
using StajFlow.Api.Data;
using StajFlow.Api.DTOs;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
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
            .Select(resume => new ResumeDto(resume.Id, resume.OriginalFileName, resume.FileSizeBytes, resume.UploadedAt, resume.IsFavorite))
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
        resume.Analyses.Add(new ResumeAnalysis { SchemaVersion = ResumeAnalysisContract.SchemaVersion });
        var fileCreated = false;
        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            if (await LockStudentAsync(userId, cancellationToken) == 0) return new(null, ProfileNotFound: true);
            resume.IsFavorite = !await context.Resumes.AnyAsync(item => item.StudentProfileId == studentId.Value && item.IsFavorite, cancellationToken);
            await using (var target = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                fileCreated = true;
                await target.WriteAsync(content, cancellationToken);
            }
            context.Resumes.Add(resume);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(CancellationToken.None);
        }
        catch
        {
            try { if (fileCreated) File.Delete(path); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            { logger.LogError(error, "Kaydedilemeyen özgeçmiş dosyası temizlenemedi."); }
            throw;
        }
        return new(new(resume.Id, name, resume.FileSizeBytes, resume.UploadedAt, resume.IsFavorite));
    }

    public async Task<bool> SetFavoriteAsync(string userId, int id, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var studentId = await LockStudentAsync(userId, cancellationToken);
        if (studentId == 0 || !await context.Resumes.AnyAsync(item => item.Id == id && item.StudentProfileId == studentId, cancellationToken))
            return false;
        await context.Resumes.Where(item => item.StudentProfileId == studentId && item.IsFavorite)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.IsFavorite, false), cancellationToken);
        await context.Resumes.Where(item => item.Id == id && item.StudentProfileId == studentId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.IsFavorite, true), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<ResumeDeletionResult> DeleteAsync(string userId, int id, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var studentId = await LockStudentAsync(userId, cancellationToken);
        if (studentId == 0) return ResumeDeletionResult.NotFound;
        var resume = await context.Resumes.SingleOrDefaultAsync(
            resume => resume.Id == id && resume.StudentProfileId == studentId, cancellationToken);
        if (resume is null) return ResumeDeletionResult.NotFound;
        var path = GetStoragePath(resume.StorageKey);
        byte[]? content = null;
        var fileDeleted = false;
        var committed = false;
        try
        {
            if (await context.Applications.AnyAsync(application => application.ResumeId == id, cancellationToken))
                return ResumeDeletionResult.InUse;
            context.Resumes.Remove(resume);
            await context.SaveChangesAsync(cancellationToken);
            if (resume.IsFavorite)
            {
                var next = await context.Resumes.Where(item => item.StudentProfileId == studentId)
                    .OrderBy(item => item.UploadedAt).ThenBy(item => item.Id).FirstOrDefaultAsync(cancellationToken);
                if (next is not null)
                {
                    next.IsFavorite = true;
                    await context.SaveChangesAsync(cancellationToken);
                }
            }
            try { content = await File.ReadAllBytesAsync(path, cancellationToken); }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
            if (content is not null)
            {
                File.Delete(path);
                fileDeleted = true;
            }
            await transaction.CommitAsync(CancellationToken.None);
            committed = true;
            return ResumeDeletionResult.Succeeded;
        }
        catch (DbUpdateConcurrencyException) { return ResumeDeletionResult.NotFound; }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 547 })
        {
            return ResumeDeletionResult.InUse;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or SqlException or DbUpdateException)
        {
            logger.LogError(error, "Özgeçmiş kaldırılamadı.");
            return ResumeDeletionResult.Failed;
        }
        finally
        {
            if (!committed && fileDeleted && content is not null)
            {
                try
                {
                    await using var restored = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                    await restored.WriteAsync(content, CancellationToken.None);
                }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                {
                    logger.LogError(error, "İptal edilen kaldırma işleminde özgeçmiş dosyası geri yüklenemedi.");
                    throw;
                }
            }
        }
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

    private string GetStoragePath(string key) => settings.GetStoragePath(environment.ContentRootPath, key);

    private Task<int> LockStudentAsync(string userId, CancellationToken cancellationToken) => context.Database.SqlQuery<int>(
        $"SELECT [Id] AS [Value] FROM [StudentProfiles] WITH (UPDLOCK, HOLDLOCK) WHERE [IdentityUserId] = {userId}")
        .SingleOrDefaultAsync(cancellationToken);
}
