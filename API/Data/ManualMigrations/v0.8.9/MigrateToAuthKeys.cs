using System;
using System.Linq;
using System.Threading.Tasks;
using API.Entities.Enums.User;
using API.Entities.History;
using API.Entities.User;
using API.Helpers;
using Kavita.Common.EnvironmentInfo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Data.ManualMigrations;

/// <summary>
/// v0.8.9 - Migrating from fixed api key to user-defined with configurable length
/// </summary>
public static class MigrateToAuthKeys
{
    public static async Task Migrate(DataContext dataContext, ILogger<Program> logger)
    {
        try
        {
            if (await dataContext.ManualMigrationHistory.AnyAsync(m => m.Name == nameof(MigrateToAuthKeys)))
            {
                return;
            }

            logger.LogCritical(
                "Running MigrateToAuthKeys migration - Please be patient, this may take some time. This is not an error");

            // First: Migrate all existing ApiKeys
            var allUsers = await dataContext.AppUser
                .Include(u => u.AuthKeys)
                .ToListAsync();

            foreach (var user in allUsers)
            {
                var key = new AppUserAuthKey()
                {
                    Name = "ApiKey",
                    Key = user.ApiKey,
                    CreatedAtUtc = DateTime.UtcNow,
                    ExpiresAtUtc = null,
                    Permissions = AuthKeyPermission.All,
                    Provider = AuthKeyProvider.System,
                };

                user.AuthKeys.Add(key);

                var imageKey = new AppUserAuthKey()
                {
                    Name = "image-only",
                    Key = AuthKeyHelper.GenerateKey(16),
                    CreatedAtUtc = DateTime.UtcNow,
                    ExpiresAtUtc = null,
                    Permissions = AuthKeyPermission.Image,
                    Provider = AuthKeyProvider.System,
                };

                user.AuthKeys.Add(imageKey);
            }

            if (dataContext.ChangeTracker.HasChanges())
            {
                await dataContext.SaveChangesAsync();
            }

            logger.LogCritical(
                "Running MigrateToAuthKeys migration - Completed. This is not an error");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during MigrateToAuthKeys migration");
            throw;
        }

        var entity = new ManualMigrationHistory
        {
            Name = nameof(MigrateToAuthKeys),
            RanAt = DateTime.UtcNow,
            ProductVersion = BuildInfo.Version.ToString()
        };
        dataContext.ManualMigrationHistory.Add(entity);
        await dataContext.SaveChangesAsync();
    }
}
