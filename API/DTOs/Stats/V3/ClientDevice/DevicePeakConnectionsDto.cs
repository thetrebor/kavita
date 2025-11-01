using System.Collections.Generic;
using API.DTOs.Statistics;

namespace API.DTOs.Stats.V3.ClientDevice;

public sealed record DevicePeakConnectionsDto
{
    public IList<StatCount<int>> Records { get; set; } = [];
    public int PeakHour { get; set; }
    public int TotalConnections { get; set; }
}
