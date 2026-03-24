using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Kavita.API.Database;
using Kavita.API.Services;
using Kavita.Common.Extensions;
using Kavita.Database;
using Kavita.Database.Extensions;
using Kavita.Models.DTOs;
using Kavita.Models.DTOs.Filtering.v2;
using Kavita.Models.DTOs.Filtering.v3;
using Kavita.Models.Entities;
using Kavita.Models.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kavita.Services.Filtering;

public class FilterV3Service(DataContext dataContext, IMapper mapper): IFilterV3Service
{

    private static readonly IFilterPipeline<Series> SeriesFilterPipeline = new FilterPipelineBuilder<Series>()
        .WithEntity(FilterEntity.Series, new FilterEntityBuilder<Series>()
            .WithField(FilterFieldV3.AgeRating, new EnumArrayFilterFieldBuilder<Series, AgeRating>()
                .WithGuard(ctx => ctx.Value.Count > 0)
                .WithComparison(FilterComparison.LessThan, ctx => s => s.Metadata.AgeRating < ctx.Value[0])
                .WithComparison(FilterComparison.LessThanEqual, ctx => s => s.Metadata.AgeRating <= ctx.Value[0])
                .WithComparison(FilterComparison.Equal, ctx => s => s.Metadata.AgeRating == ctx.Value[0])
                .WithComparison(FilterComparison.GreaterThanEqual, ctx => s => s.Metadata.AgeRating > ctx.Value[0])
                .WithComparison(FilterComparison.GreaterThan, ctx => s => s.Metadata.AgeRating <= ctx.Value[0])
                .WithComparison(FilterComparison.Contains, ctx => s => ctx.Value.Contains(s.Metadata.AgeRating))
                .Build())
            .WithField(FilterFieldV3.SeriesName, new StringFilterFieldBuilder<Series>()
                .WithGuard(ctx => !string.IsNullOrWhiteSpace(ctx.Value))
                .WithComparison(FilterComparison.Contains, ctx => s => EF.Functions.Like(s.Name, $"%{ctx.Value}%"))
                .Build())
            .WithField(FilterFieldV3.Library, new IntArrayFilterFieldBuilder<Series>()
                .WithGuard(ctx => ctx.Value.Count > 0)
                .WithComparison(FilterComparison.Contains, ctx => s => ctx.Value.Contains(s.LibraryId))
                .Build())
            .WithField(FilterFieldV3.Progress, new SeriesComparisonField())
            .Build())
        .WithEntity(FilterEntity.Chapters, new FilterEntityBuilder<Series>()
            .WithField(FilterFieldV3.FileSize, new FilterFieldBuilder<Series, long>(s => s.ParseHumanReadableBytes())
                .WithGuard(ctx => ctx.Value > 0)
                .WithComparison(FilterComparison.GreaterThan, ctx => s => s.Volumes
                    .Any(v => v.Chapters.Any(c => c.Files.Sum(f => f.Bytes) > ctx.Value)))
                .Build())
            .Build())
        .Build();

    private static readonly IFilterPipeline<Chapter> ChapterFilterPipeline = new FilterPipelineBuilder<Chapter>()
        .WithEntity(FilterEntity.Chapters, new FilterEntityBuilder<Chapter>()
            .WithField(FilterFieldV3.FileSize, new FilterFieldBuilder<Chapter, long>(s => s.ParseHumanReadableBytes())
                .WithComparison(FilterComparison.GreaterThan, ctx => s => s.Files.Sum(f => f.Bytes) > ctx.Value)
                .Build())
            .WithField(FilterFieldV3.Progress, new ChapterComparisonField())
            .Build())
        .WithEntity(FilterEntity.Series, new FilterEntityBuilder<Chapter>()
            .WithField(FilterFieldV3.SeriesName, new StringFilterFieldBuilder<Chapter>()
                .WithGuard(ctx => !string.IsNullOrWhiteSpace(ctx.Value))
                .WithComparison(FilterComparison.Contains, ctx => c => EF.Functions.Like(c.Volume.Series.Name, $"%{ctx.Value}%"))
                .Build())
            .Build())
        .Build();

    public async Task<FilterResponse> Filter(int userId, FilterV3Dto filter)
    {
        var seriesTask = ApplyFilterWithProgress<Series, SeriesDto>(FilterEntity.Series, SeriesFilterPipeline, filter, userId);
        var chapterTask = ApplyFilterWithProgress<Chapter, ChapterDto>(FilterEntity.Chapters, ChapterFilterPipeline, filter, userId);

        await Task.WhenAll(seriesTask, chapterTask);

        return new FilterResponse
        {
            Series = await seriesTask,
            Chapters = await chapterTask
        };
    }

    public FilterConfigurationDto GetConfiguration()
    {
        return new FilterConfigurationDto
        {
            Series = SeriesFilterPipeline.Configuration(),
            Chapters = ChapterFilterPipeline.Configuration()
        };
    }

    private Task<List<TEntityDto>> ApplyFilterWithProgress<TEntity, TEntityDto>(FilterEntity entity,
        IFilterPipeline<TEntity> filterPipeline, FilterV3Dto filter, int userId)
    where TEntity : class
    {
        if (!filter.RequestedEntities.Contains(entity))
            return Task.FromResult<List<TEntityDto>>([]);

        return filterPipeline.Apply(dataContext.Set<TEntity>().AsNoTracking(), filter, userId)
            .ProjectToWithProgress<TEntity, TEntityDto>(mapper, userId)
            .ToListAsync();
    }
}
