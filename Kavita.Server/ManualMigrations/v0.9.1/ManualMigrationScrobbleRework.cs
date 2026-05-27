using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Kavita.API.Repositories;
using Kavita.API.Services.Plus;
using Kavita.Database;
using Kavita.Database.Extensions;
using Kavita.Models.DTOs.KavitaPlus.Scrobble;
using Kavita.Models.Entities.Enums;
using Kavita.Models.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kavita.Server.ManualMigrations.v0._9._1;

public class ManualMigrationScrobbleRework: ManualMigration
{
    protected override string MigrationName => nameof(ManualMigrationScrobbleRework);

    protected override async Task ExecuteAsync(DataContext context, ILogger<Program> logger)
    {
        var users = await context.AppUser
            .Includes(AppUserIncludes.UserPreferences)
            .ToListAsync();

        var hasValidScrobbleProviders = false;

        foreach (var user in users)
        {
            logger.LogTrace("Migrating scrobble info for user {UserName}", user.UserName);

            hasValidScrobbleProviders |= !string.IsNullOrEmpty(user.AniListAccessToken) && string.IsNullOrEmpty(user.MalAccessToken);

            user.ScrobbleProviders[ScrobbleProvider.AniList] = new AppUserScrobbleProvider
            {
                Provider = ScrobbleProvider.AniList,
                AuthenticationToken = user.AniListAccessToken,
                Settings = new ScrobbleProviderSettingsDto()
                {
                    ProgressScrobbling = user.UserPreferences.AniListScrobblingEnabled,
                    WantToReadSync = user.UserPreferences.WantToReadSync,
                },
                ScrobbleEventGenerationRan = user.ScrobbleEventGenerationRan,
            };
            user.ScrobbleProviders[ScrobbleProvider.Mal] = new AppUserScrobbleProvider
            {
                Provider = ScrobbleProvider.Mal,
                AuthenticationToken = user.MalAccessToken,
                UserName = user.MalAccessToken,
                Settings = new ScrobbleProviderSettingsDto() {
                    WantToReadSync = user.UserPreferences.WantToReadSync,
                }
            };

            context.AppUser.Update(user);
            context.AppUserPreferences.Update(user.UserPreferences);
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}
