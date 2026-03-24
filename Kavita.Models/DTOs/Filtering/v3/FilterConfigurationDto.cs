using System.Collections.Generic;
using Kavita.Models.DTOs.Filtering.v2;

namespace Kavita.Models.DTOs.Filtering.v3;

public sealed record FilterConfigurationDto
{

    public List<FilterEntityConfigurationDto> Series { get; set; }
    public List<FilterEntityConfigurationDto> Chapters { get; set; }

}

public sealed record FilterEntityConfigurationDto
{
    public FilterEntity Entity { get; set; }
    public List<FilterFieldConfigurationDto> Fields { get; set; } = [];
}

public sealed record FilterFieldConfigurationDto
{
    public FilterFieldV3 Field { get; set; }
    public List<FilterComparison> Comparisons { get; set; } = [];
}
