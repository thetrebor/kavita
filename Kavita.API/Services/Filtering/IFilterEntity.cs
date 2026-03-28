using System.Collections.Generic;
using System.Linq;
using Kavita.Models.DTOs.Filtering.v2;
using Kavita.Models.DTOs.Filtering.v3;

namespace Kavita.API.Services.Filtering;

public interface IFilterEntity<TEntity>
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, EntityFilterStatementDto statement, FilterContext<string> context);

    IReadOnlyDictionary<EntityFilterField, IReadOnlySet<FilterComparison>> SupportedComparisons { get; }
}
