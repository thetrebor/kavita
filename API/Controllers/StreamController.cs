using System.Collections.Generic;
using System.Threading.Tasks;
using API.Constants;
using API.Data;
using API.DTOs.Dashboard;
using API.DTOs.SideNav;
using API.Extensions;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

#nullable enable

/// <summary>
/// Responsible for anything that deals with Streams (SmartFilters, ExternalSource, DashboardStream, SideNavStream)
/// </summary>
public class StreamController : BaseApiController
{
    private readonly IStreamService _streamService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;

    public StreamController(IStreamService streamService, IUnitOfWork unitOfWork, ILocalizationService localizationService)
    {
        _streamService = streamService;
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
    }

    /// <summary>
    /// Returns the layout of the user's dashboard
    /// </summary>
    /// <returns></returns>
    [HttpGet("dashboard")]
    public async Task<ActionResult<IEnumerable<DashboardStreamDto>>> GetDashboardLayout(bool visibleOnly = true)
    {
        return Ok(await _streamService.GetDashboardStreams(UserId, visibleOnly));
    }

    /// <summary>
    /// Return's the user's side nav
    /// </summary>
    [HttpGet("sidenav")]
    public async Task<ActionResult<IEnumerable<SideNavStreamDto>>> GetSideNav(bool visibleOnly = true)
    {
        return Ok(await _streamService.GetSidenavStreams(UserId, visibleOnly));
    }

    /// <summary>
    /// Return's the user's external sources
    /// </summary>
    [HttpGet("external-sources")]
    public async Task<ActionResult<IEnumerable<ExternalSourceDto>>> GetExternalSources()
    {
        return Ok(await _streamService.GetExternalSources(UserId));
    }

    /// <summary>
    /// Create an external Source
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("create-external-source")]
    public async Task<ActionResult<ExternalSourceDto>> CreateExternalSource(ExternalSourceDto dto)
    {
        // Check if a host and api key exists for the current user
        return Ok(await _streamService.CreateExternalSource(UserId, dto));
    }

    /// <summary>
    /// Updates an existing external source
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("update-external-source")]
    public async Task<ActionResult<ExternalSourceDto>> UpdateExternalSource(ExternalSourceDto dto)
    {
        if (User.IsInRole(PolicyConstants.ReadOnlyRole)) return BadRequest(await _localizationService.Translate(UserId, "permission-denied"));
        // Check if a host and api key exists for the current user
        return Ok(await _streamService.UpdateExternalSource(UserId, dto));
    }

    /// <summary>
    /// Validates the external source by host is unique (for this user)
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    [HttpGet("external-source-exists")]
    public async Task<ActionResult<bool>> ExternalSourceExists(string host, string name, string apiKey)
    {
        if (User.IsInRole(PolicyConstants.ReadOnlyRole)) return BadRequest(await _localizationService.Translate(UserId, "permission-denied"));
        return Ok(await _unitOfWork.AppUserExternalSourceRepository.ExternalSourceExists(UserId, name, host, apiKey));
    }

    /// <summary>
    /// Delete's the external source
    /// </summary>
    /// <param name="externalSourceId"></param>
    /// <returns></returns>
    [HttpDelete("delete-external-source")]
    public async Task<ActionResult> ExternalSourceExists(int externalSourceId)
    {
        if (User.IsInRole(PolicyConstants.ReadOnlyRole)) return BadRequest(await _localizationService.Translate(UserId, "permission-denied"));
        await _streamService.DeleteExternalSource(UserId, externalSourceId);
        return Ok();
    }


    /// <summary>
    /// Creates a Dashboard Stream from a SmartFilter and adds it to the user's dashboard as visible
    /// </summary>
    /// <param name="smartFilterId"></param>
    /// <returns></returns>
    [HttpPost("add-dashboard-stream")]
    public async Task<ActionResult<DashboardStreamDto>> AddDashboard([FromQuery] int smartFilterId)
    {
        if (User.IsInRole(PolicyConstants.ReadOnlyRole)) return BadRequest(await _localizationService.Translate(UserId, "permission-denied"));
        return Ok(await _streamService.CreateDashboardStreamFromSmartFilter(UserId, smartFilterId));
    }

    /// <summary>
    /// Updates the visibility of a dashboard stream
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("update-dashboard-stream")]
    public async Task<ActionResult> UpdateDashboardStream(DashboardStreamDto dto)
    {
        if (User.IsInRole(PolicyConstants.ReadOnlyRole)) return BadRequest(await _localizationService.Translate(UserId, "permission-denied"));
        await _streamService.UpdateDashboardStream(UserId, dto);
        return Ok();
    }

    /// <summary>
    /// Updates the position of a dashboard stream
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("update-dashboard-position")]
    public async Task<ActionResult> UpdateDashboardStreamPosition(UpdateStreamPositionDto dto)
    {
        if (User.IsInRole(PolicyConstants.ReadOnlyRole)) return BadRequest(await _localizationService.Translate(UserId, "permission-denied"));
        await _streamService.UpdateDashboardStreamPosition(UserId, dto);
        return Ok();
    }


    /// <summary>
    /// Creates a SideNav Stream from a SmartFilter and adds it to the user's sidenav as visible
    /// </summary>
    /// <param name="smartFilterId"></param>
    /// <returns></returns>
    [HttpPost("add-sidenav-stream")]
    public async Task<ActionResult<SideNavStreamDto>> AddSideNav([FromQuery] int smartFilterId)
    {
        if (User.IsInRole(PolicyConstants.ReadOnlyRole)) return BadRequest(await _localizationService.Translate(UserId, "permission-denied"));
        return Ok(await _streamService.CreateSideNavStreamFromSmartFilter(UserId, smartFilterId));
    }

    /// <summary>
    /// Creates a SideNav Stream from a SmartFilter and adds it to the user's sidenav as visible
    /// </summary>
    /// <param name="externalSourceId"></param>
    /// <returns></returns>
    [HttpPost("add-sidenav-stream-from-external-source")]
    public async Task<ActionResult<SideNavStreamDto>> AddSideNavFromExternalSource([FromQuery] int externalSourceId)
    {
        if (User.IsInRole(PolicyConstants.ReadOnlyRole)) return BadRequest(await _localizationService.Translate(UserId, "permission-denied"));
        return Ok(await _streamService.CreateSideNavStreamFromExternalSource(UserId, externalSourceId));
    }

    /// <summary>
    /// Updates the visibility of a dashboard stream
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("update-sidenav-stream")]
    public async Task<ActionResult> UpdateSideNavStream(SideNavStreamDto dto)
    {
        if (User.IsInRole(PolicyConstants.ReadOnlyRole)) return BadRequest(await _localizationService.Translate(UserId, "permission-denied"));
        await _streamService.UpdateSideNavStream(UserId, dto);
        return Ok();
    }

    /// <summary>
    /// Updates the position of a dashboard stream
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("update-sidenav-position")]
    public async Task<ActionResult> UpdateSideNavStreamPosition(UpdateStreamPositionDto dto)
    {
        if (User.IsInRole(PolicyConstants.ReadOnlyRole)) return BadRequest(await _localizationService.Translate(UserId, "permission-denied"));
        await _streamService.UpdateSideNavStreamPosition(UserId, dto);
        return Ok();
    }

    [HttpPost("bulk-sidenav-stream-visibility")]
    public async Task<ActionResult> BulkUpdateSideNavStream(BulkUpdateSideNavStreamVisibilityDto dto)
    {
        if (User.IsInRole(PolicyConstants.ReadOnlyRole)) return BadRequest(await _localizationService.Translate(UserId, "permission-denied"));
        await _streamService.UpdateSideNavStreamBulk(UserId, dto);
        return Ok();
    }

    /// <summary>
    /// Removes a Smart Filter from a user's SideNav Streams
    /// </summary>
    /// <param name="sideNavStreamId"></param>
    /// <returns></returns>
    [HttpDelete("smart-filter-side-nav-stream")]
    public async Task<ActionResult> DeleteSmartFilterSideNavStream([FromQuery] int sideNavStreamId)
    {
        if (User.IsInRole(PolicyConstants.ReadOnlyRole)) return BadRequest(await _localizationService.Translate(UserId, "permission-denied"));
        await _streamService.DeleteSideNavSmartFilterStream(UserId, sideNavStreamId);
        return Ok();
    }

    /// <summary>
    /// Removes a Smart Filter from a user's Dashboard Streams
    /// </summary>
    /// <param name="dashboardStreamId"></param>
    /// <returns></returns>
    [HttpDelete("smart-filter-dashboard-stream")]
    public async Task<ActionResult> DeleteSmartFilterDashboardStream([FromQuery] int dashboardStreamId)
    {
        if (User.IsInRole(PolicyConstants.ReadOnlyRole)) return BadRequest(await _localizationService.Translate(UserId, "permission-denied"));
        await _streamService.DeleteDashboardSmartFilterStream(UserId, dashboardStreamId);
        return Ok();
    }
}
