using Kavita.API.Database;
using Kavita.Models.Entities;
using Kavita.Models.Entities.Enums;
using Kavita.Models.Entities.Scrobble;
using Microsoft.Extensions.Logging;

namespace Kavita.Services.Plus.ScrobbleService;

public class MangabakaScrobbleProviderService(ILogger<MangabakaScrobbleProviderService> logger, IUnitOfWork unitOfWork) : SeriesScrobbleService<MangabakaScrobbleProviderService>(logger, unitOfWork)
{
    protected override ScrobbleProvider Provider => ScrobbleProvider.Mangabaka;
    protected override void SetScrobbleIds(ScrobbleEvent evt, Series series, Chapter chapter)
    {
        evt.MangabakaId = series.MangaBakaId;
    }
}
