using System.Collections.Generic;
using System.Threading.Tasks;
using API.Constants;
using API.Data;
using API.DTOs.Progress;
using API.Extensions;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ActivityController(IUnitOfWork unitOfWork, IClientDeviceService clientDeviceService) : BaseApiController
{
    /// <summary>
    /// Returns active reading sessions on the Server
    /// </summary>
    /// <returns></returns>
    [Authorize(PolicyGroups.AdminPolicy)]
    [HttpGet("current")]
    public async Task<ActionResult<List<ReadingSessionDto>>> GetActiveReadingSessions()
    {
        return Ok(await unitOfWork.ReadingSessionRepository.GetAllReadingSessionAsync());
    }

    [HttpGet("devices")]
    public async Task<ActionResult<List<ClientDeviceDto>>> GetClientDevices(bool includeInactive = false)
    {
        return Ok(await clientDeviceService.GetUserDeviceDtosAsync(User.GetUserId(),  includeInactive));
    }
}
