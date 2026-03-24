using System.Threading.Tasks;
using AutoMapper;
using Kavita.API.Database;
using Kavita.API.Services;
using Kavita.Models.DTOs.Filtering.v3;
using Microsoft.AspNetCore.Mvc;

namespace Kavita.Server.Controllers;

public class FilterV3Controller(IFilterV3Service filterV3Service, IDataContext dataContext, IMapper mapper): BaseApiController
{

    [HttpPost]
    public async Task<ActionResult<FilterResponse>> Filter([FromBody] FilterV3Dto filter)
    {
        return Ok(await filterV3Service.Filter(UserId, filter));
    }

    [HttpGet]
    public ActionResult<FilterConfigurationDto> GetFilterConfiguration()
    {
        return Ok(filterV3Service.GetConfiguration());
    }

}
