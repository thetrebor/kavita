using Kavita.API.Database;
using Kavita.Models.Entities;
using Kavita.Models.Entities.Enums;
using Kavita.Models.Entities.Scrobble;
using Microsoft.Extensions.Logging;

namespace Kavita.Services.Plus.ScrobbleService;

public class AniListScrobbleProviderService(ILogger<AniListScrobbleProviderService> logger, IUnitOfWork unitOfWork) : SeriesScrobbleService<AniListScrobbleProviderService>(logger, unitOfWork)
{
    protected override ScrobbleProvider Provider => ScrobbleProvider.AniList;
    protected override void SetScrobbleIds(ScrobbleEvent evt, Series series, Chapter chapter)
    {
        evt.AniListId = series.AniListId;
    }
}
