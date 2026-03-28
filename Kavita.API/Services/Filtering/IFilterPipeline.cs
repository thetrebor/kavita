using System.Collections.Generic;
using System.Linq;
using Kavita.Models.DTOs.Filtering.v2;
using Kavita.Models.DTOs.Filtering.v3;

namespace Kavita.API.Services.Filtering;

public interface IFilterPipeline<TEntity>
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, EntityFilterDto entityFilter, int userId);

    IReadOnlyDictionary<FilterEntity, IReadOnlyDictionary<EntityFilterField, IReadOnlySet<FilterComparison>>> SupportedEntities { get; }

    List<FilterEntityConfigurationDto> Configuration();
}

public class FilterContext<TValue>
{
    public required int UserId { get; init; }
    public required TValue Value { get; init; }
}
