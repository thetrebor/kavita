using System;
using System.Collections.Generic;

namespace API.DTOs.Statistics;

public sealed record StatsFilterDto
{
    public DateTime? StartDate { get; init; }

    public DateTime? EndDate
    {
        get;
        init => field = value == null || value == DateTime.MinValue ? DateTime.MaxValue : value;
    }

    public IList<int> Libraries { get; init; }
}
