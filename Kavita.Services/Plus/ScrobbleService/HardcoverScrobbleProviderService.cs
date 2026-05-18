using Kavita.API.Database;
using Kavita.Models.Entities;
using Kavita.Models.Entities.Enums;
using Kavita.Models.Entities.Scrobble;
using Microsoft.Extensions.Logging;

namespace Kavita.Services.Plus.ScrobbleService;

public class HardcoverScrobbleProviderService(ILogger<HardcoverScrobbleProviderService> logger, IUnitOfWork unitOfWork) : ChapterScrobbleService<HardcoverScrobbleProviderService>(logger, unitOfWork)
{
    protected override ScrobbleProvider Provider => ScrobbleProvider.Hardcover;
    protected override void SetScrobbleIds(ScrobbleEvent evt, Series series, Chapter chapter)
    {
        evt.HardcoverId = chapter.HardcoverId;
    }
}
