using System;
using System.Collections.Generic;
using API.DTOs.Misc;
using API.DTOs.Progress;
using Microsoft.EntityFrameworkCore;

namespace API.Entities.Progress;

[Index(nameof(DateUtc), IsUnique = true)]
public class AppUserReadingHistory
{
    public int Id { get; set; }
    public DateTime DateUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DailyReadingDataDto Data { get; set; }
    public IList<ClientInfoDto> ClientInfoUsed { get; set; }


    public int AppUserId { get; set; }
    public virtual AppUser AppUser { get; set; }
}
