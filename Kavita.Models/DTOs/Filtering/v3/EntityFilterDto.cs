using System.Collections.Generic;
using Kavita.Models.DTOs.Filtering.v2;

namespace Kavita.Models.DTOs.Filtering.v3;

public sealed record EntityFilterDto
{

    public List<EntityFilterGroupDto> Groups { get; set; }
    public FilterCombination Combination { get; set; }
    public List<FilterEntity> RequestedEntities { get; set; }
}

public sealed record EntityFilterGroupDto
{
    public FilterCombination Combination { get; set; }
    public List<EntityFilterStatementDto> Statements { get; set; }
}

public sealed record EntityFilterStatementDto
{
    public FilterEntity Entity { get; set; }
    public FilterComparison Comparison { get; set; }
    public EntityFilterField Field { get; set; }
    public string Value { get; set; }
}
