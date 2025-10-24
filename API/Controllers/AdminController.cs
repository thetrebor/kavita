using System.Collections.Generic;
using System.Threading.Tasks;
using API.Constants;
using API.Data;
using API.Data.ManualMigrations;
using API.DTOs;
using API.DTOs.Progress;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

#nullable enable

public class AdminController(UserManager<AppUser> userManager) : BaseApiController
{
    /// <summary>
    /// Checks if an admin exists on the system. This is essentially a check to validate if the system has been set up.
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("exists")]
    public async Task<ActionResult<bool>> AdminExists()
    {
        var users = await userManager.GetUsersInRoleAsync(PolicyConstants.AdminRole);
        return users.Count > 0;
    }


}
