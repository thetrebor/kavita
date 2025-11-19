using System;
using System.Linq;
using System.Threading.Tasks;
using API.Entities.History;
using Kavita.Common.EnvironmentInfo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Data.ManualMigrations;

public static class ManualMigrateUpdateTotalReads
{
    public static async Task Migrate(DataContext dataContext, ILogger<Program> logger)
    {

        try
        {
            if (await dataContext.ManualMigrationHistory.AnyAsync(m => m.Name == "ManualMigrateUpdateTotalReads"))
            {
                return;
            }

            logger.LogCritical(
                "Running ManualMigrateUpdateTotalReads migration - Please be patient, this may take some time. This is not an error");

            var updated = await dataContext.AppUserProgresses
                .Join(dataContext.Chapter,
                    p => p.ChapterId,
                    c => c.Id,
                    (p, c) => new { Progress = p, Chapter = c })
                .Where(x => x.Progress.TotalReads == 0 && x.Progress.PagesRead >= x.Chapter.Pages)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Progress.TotalReads, 1));

            logger.LogCritical(
                "Running ManualMigrateUpdateTotalReads migration - Completed. Updated {Rows} rows. This is not an error", updated);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during ManualMigrateUpdateTotalReads migration");
            throw;
        }

        dataContext.ManualMigrationHistory.Add(new ManualMigrationHistory()
        {
            Name = "ManualMigrateUpdateTotalReads",
            ProductVersion = BuildInfo.Version.ToString(),
            RanAt = DateTime.UtcNow
        });
        await dataContext.SaveChangesAsync();

    }
}
