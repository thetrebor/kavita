using System.Threading.Tasks;
using AutoMapper;
using Kavita.API.Database;
using Kavita.API.Services;
using Kavita.API.Services.Filtering;
using Kavita.Models.DTOs.Filtering.v3;
using Microsoft.AspNetCore.Mvc;

namespace Kavita.Server.Controllers;

public class FilterV3Controller(IFilterService filterService, IDataContext dataContext, IMapper mapper): BaseApiController
{

    [HttpPost]
    public async Task<ActionResult<FilterResponse>> Filter([FromBody] EntityFilterDto entityFilter)
    {
        return Ok(await filterService.Filter(UserId, entityFilter));
    }

    [HttpGet]
    public ActionResult<FilterConfigurationDto> GetFilterConfiguration()
    {
        return Ok(filterService.GetConfiguration());
    }

}
