using System;
using System.Collections.Generic;

namespace API.DTOs.Statistics;

public sealed record StatsFilterDto
{
    public TimeFilterDto TimeFilter { get; init; }
    public IList<int> Libraries { get; init; }
}

public sealed record TimeFilterDto
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
