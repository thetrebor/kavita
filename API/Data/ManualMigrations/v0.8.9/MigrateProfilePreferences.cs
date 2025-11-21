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

/// <summary>
/// v0.8.9 - Patch in new SocialPreferences
/// </summary>
public static class MigrateProfilePreferences
{
    // TODO: Implement migration logic
    public static async Task Migrate(DataContext dataContext, ILogger<Program> logger)
    {
        try
        {
            if (await dataContext.ManualMigrationHistory.AnyAsync(m => m.Name == "MigrateProfilePreferences"))
            {
                return;
            }

            logger.LogCritical(
                "Running MigrateProfilePreferences migration - Please be patient, this may take some time. This is not an error");

            // var recordsNeedingUpdate = await dataContext.AppUserPreferences
            //     .Where(x => x.SocialPreferences != null &&
            //                 EF.Functions.JsonExtract(x.SocialPreferences, "$.ShareProfile") == null)
            //     .CountAsync();




            logger.LogCritical(
                "Running MigrateProfilePreferences migration - Completed. This is not an error");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during MigrateProfilePreferences migration");
            throw;
        }

        dataContext.ManualMigrationHistory.Add(new ManualMigrationHistory()
        {
            Name = "MigrateProfilePreferences",
            ProductVersion = BuildInfo.Version.ToString(),
            RanAt = DateTime.UtcNow
        });
        await dataContext.SaveChangesAsync();
    }
}
