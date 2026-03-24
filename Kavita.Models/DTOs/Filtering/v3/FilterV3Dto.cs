using System.Collections.Generic;
using Kavita.Models.DTOs.Filtering.v2;

namespace Kavita.Models.DTOs.Filtering.v3;

public sealed record FilterV3Dto
{

    public List<FilterV3GroupDto> Groups { get; set; }
    public FilterCombination Combination { get; set; }
    public List<FilterEntity> RequestedEntities { get; set; }
}

public sealed record FilterV3GroupDto
{
    public FilterCombination Combination { get; set; }
    public List<FilterV3StatementDto> Statements { get; set; }
}

public sealed record FilterV3StatementDto
{
    public FilterEntity Entity { get; set; }
    public FilterComparison Comparison { get; set; }
    public FilterFieldV3 Field { get; set; }
    public string Value { get; set; }
}
