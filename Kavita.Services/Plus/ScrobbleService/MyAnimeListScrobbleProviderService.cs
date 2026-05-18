using System.Collections.Generic;
using Kavita.API.Database;
using Kavita.Models.DTOs.Scrobbling;
using Kavita.Models.Entities;
using Kavita.Models.Entities.Enums;
using Kavita.Models.Entities.Scrobble;
using Microsoft.Extensions.Logging;

namespace Kavita.Services.Plus.ScrobbleService;

public class MyAnimeListScrobbleProviderService(ILogger<MyAnimeListScrobbleProviderService> logger, IUnitOfWork unitOfWork) : SeriesScrobbleService<MyAnimeListScrobbleProviderService>(logger, unitOfWork)
{
    protected override ScrobbleProvider Provider => ScrobbleProvider.Mal;
    protected override IReadOnlyList<ScrobbleEventType> SupportedEvents =>
    [
        // I don't actually know?
    ];

    protected override void SetScrobbleIds(ScrobbleEvent evt, Series series)
    {
        evt.MalId = series.MalId;
    }
}
