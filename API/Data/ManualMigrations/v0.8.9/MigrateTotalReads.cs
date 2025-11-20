using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities.History;
using API.Entities.Progress;
using Kavita.Common.EnvironmentInfo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Data.ManualMigrations;

public static class MigrateTotalReads
{
    private const int BatchSize = 1000;

    public static async Task Migrate(DataContext dataContext, ILogger<Program> logger)
    {
        try
        {
            if (await dataContext.ManualMigrationHistory.AnyAsync(m => m.Name == "MigrateTotalReads"))
            {
                return;
            }

            logger.LogCritical(
                "Running MigrateTotalReads migration - Please be patient, this may take some time. This is not an error");

            var totalProgressRecords = await dataContext.AppUserProgresses.CountAsync();
            if (totalProgressRecords > 0)
            {
                logger.LogInformation("Found {Count} progress records to migrate", totalProgressRecords);

                var totalBatches = (int)Math.Ceiling(totalProgressRecords / (double)BatchSize);
                var migratedCount = 0;

                for (var batchNumber = 0; batchNumber < totalBatches; batchNumber++)
                {
                    // Join with Chapter to get TotalPages and WordCount
                    var progressBatch = await dataContext.AppUserProgresses
                        .AsNoTracking()
                        .Where(p => p.PagesRead > 0)
                        .OrderBy(p => p.Id)
                        .Skip(batchNumber * BatchSize)
                        .Take(BatchSize)
                        .Join(dataContext.Chapter,
                            p => p.ChapterId,
                            c => c.Id,
                            (progress, chapter) => new { Progress = progress, Chapter = chapter })
                        .Join(dataContext.Series,
                            p => p.Progress.SeriesId,
                            s => s.Id,
                            (combo, series) => new {combo.Progress, combo.Chapter, series.Format })
                        .Where(d => d.Progress.PagesRead >= d.Chapter.Pages)
                        .ToListAsync();

                    foreach (var progress in progressBatch)
                    {
                        progress.Progress.TotalReads = 1;
                        migratedCount += 1;
                    }

                    if (dataContext.ChangeTracker.HasChanges())
                    {
                        await dataContext.SaveChangesAsync();
                    }

                    logger.LogInformation("Migrated batch {Current}/{Total} ({Count} with TotalReads)",
                        batchNumber + 1, totalBatches, migratedCount);
                }

                logger.LogInformation("Migration complete: {Count}/{Total} progress records updated with total reads",
                    migratedCount, totalProgressRecords);
            }
            else
            {
                logger.LogInformation("No progress records found to migrate");
            }

            logger.LogCritical(
                "Running MigrateTotalReads migration - Completed. This is not an error");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during MigrateTotalReads migration");
            throw;
        }

        dataContext.ManualMigrationHistory.Add(new ManualMigrationHistory()
        {
            Name = "MigrateTotalReads",
            ProductVersion = BuildInfo.Version.ToString(),
            RanAt = DateTime.UtcNow
        });
        await dataContext.SaveChangesAsync();
    }
}
