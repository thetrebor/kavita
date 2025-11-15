using System.Collections.Generic;

namespace API.DTOs.Statistics;

public sealed record PageSpreadStatsDto
{
    public List<StatBucketDto> Buckets { get; set; }
    public int TotalCount { get; set; }
}
