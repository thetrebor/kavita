using System.Collections.Generic;
using System.Threading.Tasks;
using API.Constants;
using API.Data;
using API.DTOs.Progress;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

// TODO: Move all this to a SessionController or something
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
}
