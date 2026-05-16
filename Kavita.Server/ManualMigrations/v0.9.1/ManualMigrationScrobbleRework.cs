using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Kavita.API.Repositories;
using Kavita.API.Services.Plus;
using Kavita.Database;
using Kavita.Database.Extensions;
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
                AuthenticationToken = user.AniListAccessToken,
            };
            user.ScrobbleProviders[ScrobbleProvider.Mal] = new AppUserScrobbleProvider
            {
                AuthenticationToken = user.MalAccessToken,
                UserName = user.MalAccessToken,
            };

            user.UserPreferences.ScrobbleSettings[ScrobbleProvider.AniList] = new AppUserScrobbleSettings()
            {
                ProgressScrobbling = user.UserPreferences.AniListScrobblingEnabled,
                WantToReadSync = user.UserPreferences.WantToReadSync,
            };
            user.UserPreferences.ScrobbleSettings[ScrobbleProvider.Mal] = new AppUserScrobbleSettings()
            {
                WantToReadSync = user.UserPreferences.WantToReadSync,
            };

            context.AppUser.Update(user);
            context.AppUserPreferences.Update(user.UserPreferences);
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }

        if (hasValidScrobbleProviders)
        {
            logger.LogDebug("Valid scrobble providers found, enqueueing a provider sync task");
            BackgroundJob.Enqueue<IScrobblingService>(s => s.SyncProviderInfo(CancellationToken.None));
        }
    }
}
