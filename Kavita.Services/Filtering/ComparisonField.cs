using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Kavita.Common.Extensions;
using Kavita.Database.Extensions;
using Kavita.Database.Extensions.Filters;
using Kavita.Models.DTOs.Filtering.v2;
using Kavita.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kavita.Services.Filtering;

public class SeriesComparisonField : IFilterField<Series>
{
    public IQueryable<Series> Apply(IQueryable<Series> query, FilterComparison comparison, FilterContext<string> context)
    {
        if (string.IsNullOrEmpty(context.Value)) return query;

        var value = context.Value.AsFloat();

        return query.HasReadingProgress(true, comparison, value, context.UserId);
    }

    public IReadOnlySet<FilterComparison> SupportedComparisons { get; } = new ReadOnlySet<FilterComparison>(new HashSet<FilterComparison>([
        FilterComparison.Equal, FilterComparison.NotEqual,
        FilterComparison.GreaterThan, FilterComparison.GreaterThanEqual,
        FilterComparison.LessThan, FilterComparison.LessThanEqual
    ]));
}

public class ChapterComparisonField : IFilterField<Chapter>
{
    public IQueryable<Chapter> Apply(IQueryable<Chapter> query, FilterComparison comparison, FilterContext<string> context)
    {
        if (string.IsNullOrEmpty(context.Value)) return query;

        var readProgress = context.Value.AsFloat();

        var subQuery = query
            .Select(s => new
            {
                ChapterId = s.Id,
                Percentage = s.UserProgress
                    .Where(p => p != null && p.AppUserId == context.UserId)
                    .Sum(p => p != null ? (p.PagesRead * 1.0f / s.Pages) : 0f) * 100f
            })
            .AsSplitQuery();

        switch (comparison)
        {
            case FilterComparison.Equal:
                subQuery = subQuery.WhereEqual(s => s.Percentage, readProgress);
                break;
            case FilterComparison.GreaterThan:
                subQuery = subQuery.WhereGreaterThan(s => s.Percentage, readProgress);
                break;
            case FilterComparison.GreaterThanEqual:
                subQuery = subQuery.WhereGreaterThanOrEqual(s => s.Percentage, readProgress);
                break;
            case FilterComparison.LessThan:
                subQuery = subQuery.WhereLessThan(s => s.Percentage, readProgress);
                break;
            case FilterComparison.LessThanEqual:
                subQuery = subQuery.WhereLessThanOrEqual(s => s.Percentage, readProgress);
                break;
            case FilterComparison.NotEqual:
                subQuery = subQuery.WhereNotEqual(s => s.Percentage, readProgress);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
        }

        var ids = subQuery.Select(s => s.ChapterId);
        return query.Where(c => ids.Contains(c.Id));
    }

    public IReadOnlySet<FilterComparison> SupportedComparisons { get; } = new ReadOnlySet<FilterComparison>(new HashSet<FilterComparison>([
        FilterComparison.Equal, FilterComparison.NotEqual,
        FilterComparison.GreaterThan, FilterComparison.GreaterThanEqual,
        FilterComparison.LessThan, FilterComparison.LessThanEqual
    ]));
}
