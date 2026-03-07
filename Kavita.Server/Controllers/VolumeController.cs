using System.Threading.Tasks;
using Kavita.API.Database;
using Kavita.API.Repositories;
using Kavita.API.Services;
using Kavita.API.Services.SignalR;
using Kavita.Models.Constants;
using Kavita.Models.DTOs;
using Kavita.Models.DTOs.SignalR;
using Kavita.Server.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kavita.Server.Controllers;

public class VolumeController(IUnitOfWork unitOfWork, ILocalizationService localizationService,
    IEntityNamingService namingService, IEventHub eventHub)
    : BaseApiController
{
    /// <summary>
    /// Returns the appropriate Volume
    /// </summary>
    /// <param name="volumeId"></param>
    /// <returns></returns>
    [VolumeAccess]
    [HttpGet]
    public async Task<ActionResult<VolumeDto?>> GetVolume(int volumeId)
    {
        var volume = await unitOfWork.VolumeRepository.GetVolumeDtoAsync(volumeId, UserId);
        if (volume != null)
        {
            var libraryType = await unitOfWork.LibraryRepository.GetLibraryTypeBySeriesIdAsync(volume.SeriesId);
            var namingContext = await LocalizedNamingContext.CreateAsync(namingService, localizationService, UserId, libraryType);
            namingContext.ApplyNaming([volume]);
        }

        return Ok(volume);
    }

    [HttpDelete]
    [Authorize(Policy = PolicyGroups.AdminPolicy)]
    public async Task<ActionResult<bool>> DeleteVolume(int volumeId)
    {
        var volume = await unitOfWork.VolumeRepository.GetVolumeByIdAsync(volumeId,
            VolumeIncludes.Chapters | VolumeIncludes.People | VolumeIncludes.Tags);
        if (volume == null)
            return BadRequest(localizationService.Translate(UserId, "volume-doesnt-exist"));

        unitOfWork.VolumeRepository.Remove(volume);

        if (await unitOfWork.CommitAsync())
        {
            await eventHub.SendMessageAsync(MessageFactory.VolumeRemoved, MessageFactory.VolumeRemovedEvent(volume.Id, volume.SeriesId), false);
            return Ok(true);
        }

        return Ok(false);
    }

    [HttpPost("multiple")]
    [Authorize(Policy = PolicyGroups.AdminPolicy)]
    public async Task<ActionResult<bool>> DeleteMultipleVolumes(int[] volumesIds)
    {
        var volumes = await unitOfWork.VolumeRepository.GetVolumesById(volumesIds);
        if (volumes.Count != volumesIds.Length)
        {
            return BadRequest(localizationService.Translate(UserId, "volume-doesnt-exist"));
        }

        unitOfWork.VolumeRepository.Remove(volumes);

        if (!await unitOfWork.CommitAsync())
        {
            return Ok(false);
        }

        foreach (var volume in volumes)
        {
            await eventHub.SendMessageAsync(MessageFactory.VolumeRemoved, MessageFactory.VolumeRemovedEvent(volume.Id, volume.SeriesId), false);
        }

        return Ok(true);
    }
}
