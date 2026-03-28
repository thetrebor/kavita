using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Kavita.Models.DTOs.Filtering.v2;

namespace Kavita.API.Services.Filtering;

public interface IFilterField<TEntity>
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, FilterComparison comparison, FilterContext<string> context);
    IReadOnlySet<FilterComparison> SupportedComparisons { get; }
}

public delegate Expression<Func<TEntity, bool>> FilterExpression<TEntity, TValue>(FilterContext<TValue> ctx);
public delegate IQueryable<TEntity> FilterFunc<TEntity, TValue>(FilterContext<TValue> ctx, IQueryable<TEntity> query);
