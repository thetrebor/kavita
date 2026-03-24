using System;
using System.Collections.Generic;
using System.Linq;
using Kavita.Models.DTOs.Filtering.v2;
using Kavita.Models.DTOs.Filtering.v3;

namespace Kavita.Services.Filtering;

public interface IFilterEntity<TEntity>
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, FilterV3StatementDto statement, FilterContext<string> context);

    IReadOnlyDictionary<FilterFieldV3, IReadOnlySet<FilterComparison>> SupportedComparisons { get; }
}

public class FilterEntityBuilder<TEntity>
{

    private readonly Dictionary<FilterFieldV3, IFilterField<TEntity>> _fields = [];

    public FilterEntityBuilder<TEntity> WithField(FilterFieldV3 field, IFilterField<TEntity> filterField)
    {
        if (!_fields.TryAdd(field, filterField))
            throw new ArgumentException("Cannot register for the same field twice", nameof(field));

        return this;
    }

    public IFilterEntity<TEntity> Build()
    {
        return new FilterEntity(_fields);
    }

    private sealed class FilterEntity(Dictionary<FilterFieldV3, IFilterField<TEntity>> fields): IFilterEntity<TEntity>
    {
        public IQueryable<TEntity> Apply(IQueryable<TEntity> query, FilterV3StatementDto statement, FilterContext<string> context)
        {
            if (fields.TryGetValue(statement.Field, out var field))
            {
                return field.Apply(query, statement.Comparison, context);
            }

            return query;
        }

        public IReadOnlyDictionary<FilterFieldV3, IReadOnlySet<FilterComparison>> SupportedComparisons { get; } =
            fields.ToDictionary(kv => kv.Key, kv => kv.Value.SupportedComparisons).AsReadOnly();
    }

}
