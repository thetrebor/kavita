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

public class MangabakaScrobbleProviderService(ILogger<MangabakaScrobbleProviderService> logger, IUnitOfWork unitOfWork, IKavitaPlusAuditService auditService)
    : SeriesScrobbleService<MangabakaScrobbleProviderService>(logger, unitOfWork, auditService)
{
    protected override ScrobbleProvider Provider => ScrobbleProvider.MangaBaka;
    protected override IReadOnlyList<ScrobbleEventType> SupportedEvents =>
    [
        ScrobbleEventType.ChapterRead, ScrobbleEventType.AddWantToRead, ScrobbleEventType.RemoveWantToRead,
        ScrobbleEventType.ScoreUpdated
    ];
    protected override void SetScrobbleIds(ScrobbleEvent evt, Series series)
    {
        evt.MangabakaId = series.MangaBakaId;
    }

    public override async Task UpdateUserScrobbleProvider(int userId, ScrobbleProviderDto dto, CancellationToken ct = default)
    {
        var user = await unitOfWork.UserRepository.GetUserByIdAsync(userId, ct: ct);
        if (user == null) throw new KavitaNotFoundException();

        var scrobbleProvider = user.ScrobbleProviders.GetValueOrDefault(dto.Provider) ?? new AppUserScrobbleProvider()
        {
            Provider = dto.Provider
        };

        scrobbleProvider.AuthenticationToken = dto.AuthenticationToken;

        unitOfWork.UserRepository.Update(user);
        await unitOfWork.CommitAsync(ct);
    }
}
