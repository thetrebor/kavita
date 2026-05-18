using Kavita.API.Database;
using Kavita.Models.Entities;
using Kavita.Models.Entities.Enums;
using Kavita.Models.Entities.Scrobble;
using Microsoft.Extensions.Logging;

namespace Kavita.Services.Plus.ScrobbleService;

public class MyAnimeListScrobbleProviderService(ILogger<MyAnimeListScrobbleProviderService> logger, IUnitOfWork unitOfWork) : SeriesScrobbleService<MyAnimeListScrobbleProviderService>(logger, unitOfWork)
{
    protected override ScrobbleProvider Provider => ScrobbleProvider.Mal;
    protected override void SetScrobbleIds(ScrobbleEvent evt, Series series, Chapter chapter)
    {
        evt.MalId = series.MalId;
    }
}
