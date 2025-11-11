using System;
using System.Linq;
using System.Threading.Tasks;
using API.Entities.History;
using Kavita.Common.EnvironmentInfo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Data.ManualMigrations;

/// <summary>
/// v0.8.9 - Some AppUser rows are missing CreatedUtc date
/// </summary>
public static class MigrateMissingCreatedUtcDate
{
    //0001-01-01 00:00:00
    public static async Task Migrate(DataContext dataContext, ILogger<Program> logger)
    {
        try
        {
            if (await dataContext.ManualMigrationHistory.AnyAsync(m => m.Name == "MigrateMissingCreatedUtcDate"))
            {
                return;
            }

            logger.LogCritical(
                "Running MigrateMissingCreatedUtcDate migration - Please be patient, this may take some time. This is not an error");

            var usersWithoutCorrectCreatedUtc = await dataContext.AppUser
                .Where(u => u.CreatedUtc == DateTime.MinValue)
                .ToListAsync();

            foreach (var user in usersWithoutCorrectCreatedUtc)
            {
                user.CreatedUtc = user.Created.ToUniversalTime();
            }

            if (dataContext.ChangeTracker.HasChanges())
            {
                await dataContext.SaveChangesAsync();
            }

            logger.LogCritical(
                "Running MigrateMissingCreatedUtcDate migration - Completed. This is not an error");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during MigrateMissingCreatedUtcDate migration");
            throw;
        }

        var entity = new ManualMigrationHistory()
        {
            Name = "MigrateMissingCreatedUtcDate",
            RanAt = DateTime.UtcNow
        };
        entity.ProductVersion = BuildInfo.Version.ToString();
        dataContext.ManualMigrationHistory.Add(entity);
        await dataContext.SaveChangesAsync();
    }

}
