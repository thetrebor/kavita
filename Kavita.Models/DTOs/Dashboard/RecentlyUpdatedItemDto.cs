using System;
using Kavita.Models.DTOs.ReadingLists;
using Kavita.Models.Entities.Enums;

namespace Kavita.Models.DTOs.Dashboard;
#nullable enable

public sealed record RecentlyUpdatedItemDto
{
    public EntityKind Kind { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public GroupedSeriesDto? Series { get; set; }
    public ReadingListDto? ReadingList { get; set; }
}
