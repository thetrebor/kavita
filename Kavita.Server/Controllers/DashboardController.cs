using System.Collections.Generic;
using System.Threading.Tasks;
using Kavita.API.Database;
using Kavita.Common.Helpers;
using Kavita.Models.DTOs.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Kavita.Server.Controllers;

public class DashboardController(IUnitOfWork unitOfWork) : BaseApiController
{
    /// <summary>
    /// Returns a mixed stream of recently updated items for the dashboard, interleaving Series
    /// (newly added chapters) and Reading Lists (last CBL sync) ordered by most recent.
    /// </summary>
    /// <param name="userParams">Page size and offset</param>
    [HttpPost("recently-updated-items")]
    public async Task<ActionResult<IList<RecentlyUpdatedItemDto>>> GetRecentlyUpdatedItems([FromQuery] UserParams? userParams)
    {
        userParams ??= UserParams.Default;
        return Ok(await unitOfWork.SeriesRepository.GetRecentlyUpdatedItems(UserId, userParams));
    }
}
