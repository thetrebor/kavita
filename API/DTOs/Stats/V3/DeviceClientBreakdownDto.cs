using System.Collections.Generic;
using API.DTOs.Statistics;
using API.Entities.Enums;

namespace API.DTOs.Stats.V3;

public sealed record DeviceClientBreakdownDto
{
    public IList<StatCount<ClientDeviceType>> Records { get; set; }
    public int TotalCount { get; set; }
}
