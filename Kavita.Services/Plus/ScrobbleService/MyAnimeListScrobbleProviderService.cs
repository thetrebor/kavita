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

    public override async Task UpdateUserScrobbleProvider(int userId, ScrobbleProviderDto dto, CancellationToken ct = default)
    {
        var user = await unitOfWork.UserRepository.GetUserByIdAsync(userId, ct: ct);
        if (user == null) throw new KavitaNotFoundException();

        var scrobbleProvider = user.ScrobbleProviders.GetValueOrDefault(dto.Provider) ?? new AppUserScrobbleProvider()
        {
            Provider = dto.Provider
        };

        scrobbleProvider.AuthenticationToken = dto.AuthenticationToken;
        scrobbleProvider.UserName = dto.UserName;

        unitOfWork.UserRepository.Update(user);
        await unitOfWork.CommitAsync(ct);
    }
}
