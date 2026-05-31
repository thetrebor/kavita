using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kavita.API.Database;
using Kavita.API.Services.Plus;
using Kavita.Common;
using Kavita.Models.DTOs.KavitaPlus.Scrobble;
using Kavita.Models.DTOs.Scrobbling;
using Kavita.Models.Entities;
using Kavita.Models.Entities.Enums;
using Kavita.Models.Entities.Scrobble;
using Kavita.Models.Entities.User;
using Microsoft.Extensions.Logging;

namespace Kavita.Services.Plus.ScrobbleService;

public class MyAnimeListScrobbleProviderService(ILogger<MyAnimeListScrobbleProviderService> logger, IUnitOfWork unitOfWork, IKavitaPlusAuditService auditService)
    : SeriesScrobbleService<MyAnimeListScrobbleProviderService>(logger, unitOfWork, auditService)
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

    public override bool IsTokenValid(string token)
    {
        // I don't actually know what Mal uses, but it's always valid whatever
        return true;
    }
}
