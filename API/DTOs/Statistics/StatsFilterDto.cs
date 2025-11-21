using System;
using System.Collections.Generic;

namespace API.DTOs.Statistics;

public sealed record StatsFilterDto
{
    public DateTime? StartDate { get; init; }

    private DateTime? _endDate;
    public DateTime? EndDate
    {
        get => _endDate;
        init => _endDate = value == null || value == DateTime.MinValue ? DateTime.MaxValue : value;
    }


    private IList<int>? _libraries;
    public IList<int> Libraries
    {
        get => _libraries ?? [];
        set => _libraries = value;
    }

}
