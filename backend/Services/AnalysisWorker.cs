using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StajFlow.Api.Configurations;
using StajFlow.Api.Data;
using StajFlow.Api.Entities;

namespace StajFlow.Api.Services;

public sealed class AnalysisWorker(IServiceScopeFactory scopeFactory, IOptions<GeminiOptions> options,
    TimeProvider clock, ILogger<AnalysisWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (options.Value.Enabled) await RunWithLockAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception error) when (error is not OutOfMemoryException)
            {
                logger.LogWarning("Analiz işleyicisi şu anda çalışamıyor; yeniden denenecek.");
            }
            try { await Task.Delay(TimeSpan.FromSeconds(5), clock, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }

    private async Task RunWithLockAsync(CancellationToken cancellationToken)
    {
        await using var lockScope = scopeFactory.CreateAsyncScope();
        var database = lockScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if ((await database.Database.GetPendingMigrationsAsync(cancellationToken)).Any()) return;
        var connection = database.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DECLARE @result int; EXEC @result = sp_getapplock @Resource = N'StajFlow.ResumeAnalysisWorker', @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 0; SELECT @result;";
        var result = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        if (result < 0) return;
        try
        {
            var now = clock.GetUtcNow();
            await database.ResumeAnalyses.Where(record => record.Status == AnalysisStatus.Processing)
                .ExecuteUpdateAsync(setters => setters.SetProperty(record => record.Status, AnalysisStatus.Failed)
                    .SetProperty(record => record.DataJson, (string?)null)
                    .SetProperty(record => record.ErrorCode, (AnalysisErrorCode?)AnalysisErrorCode.Cancelled)
                    .SetProperty(record => record.CompletedAt, record => record.StartedAt > now ? record.StartedAt : now), cancellationToken);
            await database.PostingAnalyses.Where(record => record.Status == AnalysisStatus.Processing)
                .ExecuteUpdateAsync(setters => setters.SetProperty(record => record.Status, AnalysisStatus.Failed)
                    .SetProperty(record => record.DataJson, (string?)null)
                    .SetProperty(record => record.ErrorCode, (AnalysisErrorCode?)AnalysisErrorCode.Cancelled)
                    .SetProperty(record => record.CompletedAt, record => record.StartedAt > now ? record.StartedAt : now), cancellationToken);
            var preferPosting = false;
            while (!cancellationToken.IsCancellationRequested && connection.State == ConnectionState.Open)
            {
                command.CommandText = "SELECT APPLOCK_MODE('public', 'StajFlow.ResumeAnalysisWorker', 'Session');";
                if (!string.Equals(Convert.ToString(await command.ExecuteScalarAsync(cancellationToken)), "Exclusive", StringComparison.Ordinal)) return;
                await using var scope = scopeFactory.CreateAsyncScope();
                var resumes = scope.ServiceProvider.GetRequiredService<ResumeAnalysisService>();
                var postings = scope.ServiceProvider.GetRequiredService<PostingAnalysisService>();
                var processed = preferPosting
                    ? await postings.ProcessNextAsync(cancellationToken) || await resumes.ProcessNextAsync(cancellationToken)
                    : await resumes.ProcessNextAsync(cancellationToken) || await postings.ProcessNextAsync(cancellationToken);
                preferPosting = !preferPosting;
                if (!processed) await Task.Delay(TimeSpan.FromSeconds(3), clock, cancellationToken);
            }
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
            {
                command.CommandText = "EXEC sp_releaseapplock @Resource = N'StajFlow.ResumeAnalysisWorker', @LockOwner = 'Session';";
                try { await command.ExecuteNonQueryAsync(CancellationToken.None); }
                catch (Exception error) when (error is System.Data.Common.DbException or InvalidOperationException) { }
            }
        }
    }
}
