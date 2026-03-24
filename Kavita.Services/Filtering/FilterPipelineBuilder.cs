using System;
using System.Collections.Generic;
using System.Linq;
using Kavita.Models.DTOs.Filtering.v2;
using Kavita.Models.DTOs.Filtering.v3;

namespace Kavita.Services.Filtering;

public interface IFilterPipeline<TEntity>
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, FilterV3Dto filter, int userId);

    IReadOnlyDictionary<FilterEntity, IReadOnlyDictionary<FilterFieldV3, IReadOnlySet<FilterComparison>>> SupportedEntities { get; }

    List<FilterEntityConfigurationDto> Configuration();
}

public class FilterContext<TValue>
{
    public required int UserId { get; init; }
    public required TValue Value { get; init; }
}

public class FilterPipelineBuilder<TEntity>
{

    private readonly Dictionary<FilterEntity, IFilterEntity<TEntity>> _entities = [];

    public FilterPipelineBuilder<TEntity> WithEntity(FilterEntity entity, IFilterEntity<TEntity> filterEntity)
    {
        if (!_entities.TryAdd(entity, filterEntity))
            throw new ArgumentException("Cannot register the same entity twice", nameof(entity));

        return this;
    }

    public IFilterPipeline<TEntity> Build()
    {
        return new FilterPipeline(_entities);
    }

    private sealed class FilterPipeline(Dictionary<FilterEntity, IFilterEntity<TEntity>> entities): IFilterPipeline<TEntity>
    {
        public IQueryable<TEntity> Apply(IQueryable<TEntity> query, FilterV3Dto filter, int userId)
        {
            var groups = filter.Groups
                .Select(group => ApplyGroup(query, group, userId))
                .ToList();

            if (groups.Count == 0) return query; // Return everything if no filters

            return filter.Combination == FilterCombination.And
                ? groups.Aggregate((current, next) => current.Intersect(next))
                : groups.Aggregate((current, next) => current.Union(next));
        }

        public IReadOnlyDictionary<FilterEntity, IReadOnlyDictionary<FilterFieldV3, IReadOnlySet<FilterComparison>>> SupportedEntities { get; } =
            entities.ToDictionary(kv => kv.Key, kv => kv.Value.SupportedComparisons).AsReadOnly();

        public List<FilterEntityConfigurationDto> Configuration()
        {
            return SupportedEntities.Select(kv => new FilterEntityConfigurationDto()
            {
                Entity = kv.Key,
                Fields = kv.Value.Select(kv => new FilterFieldConfigurationDto()
                {
                    Field = kv.Key,
                    Comparisons = kv.Value.ToList()
                }).ToList()
            }).ToList();
        }

        private IQueryable<TEntity> ApplyGroup(IQueryable<TEntity> query, FilterV3GroupDto group, int userId)
        {
            var statements = group.Statements
                .Select(statement => ApplyEntity(query, statement, new FilterContext<string> { UserId = userId, Value = statement.Value }))
                .ToList();

            if (statements.Count == 0) return query;

            return group.Combination == FilterCombination.And
                ? statements.Aggregate((current, next) => current.Intersect(next))
                : statements.Aggregate((current, next) => current.Union(next));
        }


        private IQueryable<TEntity> ApplyEntity(IQueryable<TEntity> query, FilterV3StatementDto statement, FilterContext<string> context)
        {
            if (entities.TryGetValue(statement.Entity, out var entity))
            {
                return entity.Apply(query, statement, context);
            }

            return query;
        }

    }
}
