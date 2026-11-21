using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StajFlow.Api.Configurations;
using StajFlow.Api.Data;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;

namespace StajFlow.Api.Services;

public class StudentAccountService(
    ApplicationDbContext context, UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager, IOptions<ResumeStorageOptions> options,
    IWebHostEnvironment environment, ILogger<StudentAccountService> logger) : IStudentAccountService
{
    public async Task<StudentAccountDeletionResult> DeleteAsync(
        ClaimsPrincipal principal, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null || !await userManager.IsInRoleAsync(user, UserRoles.Student))
            return StudentAccountDeletionResult.NotFound;
        var check = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!check.Succeeded) return StudentAccountDeletionResult.InvalidPassword;

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var deletedFiles = new List<(string Path, byte[] Content)>();
        var committed = false;
        try
        {
            var student = await context.StudentProfiles.SingleOrDefaultAsync(
                profile => profile.IdentityUserId == user.Id, cancellationToken);
            if (student is null) return StudentAccountDeletionResult.NotFound;
            var keys = await context.Resumes.Where(resume => resume.StudentProfileId == student.Id)
                .Select(resume => resume.StorageKey).ToListAsync(cancellationToken);
            await context.Applications.Where(application => application.StudentProfileId == student.Id)
                .ExecuteDeleteAsync(cancellationToken);
            await context.Resumes.Where(resume => resume.StudentProfileId == student.Id)
                .ExecuteDeleteAsync(cancellationToken);
            context.StudentProfiles.Remove(student);
            await context.SaveChangesAsync(cancellationToken);
            var deletion = await userManager.DeleteAsync(user);
            if (!deletion.Succeeded) return StudentAccountDeletionResult.Conflict;

            foreach (var key in keys)
            {
                var path = options.Value.GetStoragePath(environment.ContentRootPath, key);
                byte[] content;
                try { content = await File.ReadAllBytesAsync(path, cancellationToken); }
                catch (FileNotFoundException) { continue; }
                catch (DirectoryNotFoundException) { continue; }
                File.Delete(path);
                deletedFiles.Add((path, content));
            }
            await transaction.CommitAsync(CancellationToken.None);
            committed = true;
        }
        catch (DbUpdateConcurrencyException) { return StudentAccountDeletionResult.Conflict; }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or SqlException or DbUpdateException)
        {
            logger.LogError(error, "Öğrenci hesabı silinemedi.");
            return StudentAccountDeletionResult.Failed;
        }
        finally
        {
            if (!committed)
            {
                foreach (var file in deletedFiles)
                {
                    await using var restored = new FileStream(file.Path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                    await restored.WriteAsync(file.Content, CancellationToken.None);
                }
            }
        }
        await signInManager.SignOutAsync();
        return StudentAccountDeletionResult.Succeeded;
    }
}
