using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Kavita.Common.Extensions;
using Kavita.Models.DTOs.Filtering.v2;

namespace Kavita.Services.Filtering;

public interface IFilterField<TEntity>
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, FilterComparison comparison, FilterContext<string> context);
    IReadOnlySet<FilterComparison> SupportedComparisons { get; }
}

public delegate Expression<Func<TEntity, bool>> FilterExpression<TEntity, TValue>(FilterContext<TValue> ctx);
public delegate IQueryable<TEntity> FilterFunc<TEntity, TValue>(FilterContext<TValue> ctx, IQueryable<TEntity> query);

public class FilterFieldBuilder<TEntity, TValue>(Func<string, TValue> convertor)
where TEntity: class
{
    private readonly Dictionary<FilterComparison, FilterExpression<TEntity, TValue>> _comparisons = [];
    private readonly Dictionary<FilterComparison, FilterFunc<TEntity, TValue>> _funcComparisons = [];
    private Func<FilterContext<TValue>, bool>? _guard;

    public FilterFieldBuilder<TEntity, TValue> WithGuard(Func<FilterContext<TValue>, bool> guard)
    {
        _guard = guard;
        return this;
    }

    public FilterFieldBuilder<TEntity, TValue> WithComparison(FilterComparison comparison, FilterExpression<TEntity, TValue> expression)
    {
        if (_funcComparisons.ContainsKey(comparison))
            throw new ArgumentException("Cannot register the same comparison twice", nameof(comparison));

        if (!_comparisons.TryAdd(comparison, expression))
            throw new ArgumentException("Cannot register the same comparison twice", nameof(comparison));

        return this;
    }

    public FilterFieldBuilder<TEntity, TValue> WithComparison(FilterComparison combination, FilterFunc<TEntity, TValue> func)
    {
        if (_comparisons.ContainsKey(combination))
            throw new ArgumentException("Cannot register the same combination twice", nameof(combination));

        if (!_funcComparisons.TryAdd(combination, func))
            throw new ArgumentException("Cannot register the same combination twice", nameof(combination));

        return this;
    }

    public IFilterField<TEntity> Build()
    {
        return new FilterField(_comparisons, _funcComparisons, convertor, _guard);
    }

    private sealed class FilterField(
        Dictionary<FilterComparison, FilterExpression<TEntity, TValue>> comparisons,
        Dictionary<FilterComparison, FilterFunc<TEntity, TValue>> funcComparisons,
        Func<string, TValue> converter,
        Func<FilterContext<TValue>, bool>? guard
        ) : IFilterField<TEntity>
    {
        public IQueryable<TEntity> Apply(IQueryable<TEntity> query, FilterComparison comparison, FilterContext<string> context)
        {
            if (comparisons.TryGetValue(comparison, out var expression))
            {
                var value = converter(context.Value);
                var newContext = new FilterContext<TValue> { Value = value, UserId = context.UserId };
                if (guard != null && !guard(newContext)) return query;

                return query.Where(expression(newContext));
            }

            if (funcComparisons.TryGetValue(comparison, out var func))
            {
                var value = converter(context.Value);
                var newContext = new FilterContext<TValue> { Value = value, UserId = context.UserId };
                if (guard != null && !guard(newContext)) return query;

                return func(newContext, query);
            }

            return query;
        }

        public IReadOnlySet<FilterComparison> SupportedComparisons { get; } = comparisons.Keys
            .Union(funcComparisons.Keys).ToHashSet();
    }

}

public class StringFilterFieldBuilder<TEntity>() : FilterFieldBuilder<TEntity, string>(s => s)
    where TEntity : class;

public class IntArrayFilterFieldBuilder<TEntity>() : FilterFieldBuilder<TEntity, IList<int>>(s => s.ParseIntArray())
    where TEntity : class;

public class IntFilterFieldBuilder<TEntity>() : FilterFieldBuilder<TEntity, int>(int.Parse)
    where TEntity : class;

public class EnumArrayFilterFieldBuilder<TEntity, TEnum>() : FilterFieldBuilder<TEntity, IList<TEnum>>(s => s
    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
    .Select(Enum.Parse<TEnum>)
    .ToList()
)
    where TEntity : class
    where TEnum : struct;
