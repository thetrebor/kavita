using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Kavita.API.Services;
using Kavita.API.Services.Filtering;
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

public class FilterService(DataContext dataContext, IMapper mapper): IFilterService
{

    private static readonly IFilterPipeline<Series> SeriesFilterPipeline = new FilterPipelineBuilder<Series>()
        .WithEntity(FilterEntity.Series, new FilterEntityBuilder<Series>()
            .WithField(EntityFilterField.Summary, new StringFilterFieldBuilder<Series>()
                .WithComparison(FilterComparison.Equal, ctx => s => s.Metadata.Summary.Equals(ctx.Value))
                .WithComparison(FilterComparison.Contains, ctx => s => EF.Functions.Like(s.Metadata.Summary, $"%{ctx.Value}%"))
                .WithComparison(FilterComparison.BeginsWith, ctx => s => EF.Functions.Like(s.Metadata.Summary, $"{ctx.Value}%"))
                .WithComparison(FilterComparison.EndsWith, ctx => s => EF.Functions.Like(s.Metadata.Summary, $"%{ctx.Value}"))
                .WithComparison(FilterComparison.Matches, ctx => s => EF.Functions.Like(s.Metadata.Summary, $"%{ctx.Value}%"))
                .WithComparison(FilterComparison.NotEqual, ctx => s => s.Metadata.Summary != ctx.Value)
                .WithComparison(FilterComparison.IsEmpty, ctx => s => string.IsNullOrEmpty(s.Metadata.Summary))
                .Build())
            .WithField(EntityFilterField.SeriesName, new StringFilterFieldBuilder<Series>()
                .WithGuard(ctx => !string.IsNullOrWhiteSpace(ctx.Value))
                .WithComparison(FilterComparison.Matches, ctx => s =>
                    EF.Functions.Like(s.Name, $"%{ctx.Value}%") ||
                    EF.Functions.Like(s.OriginalName, $"%{ctx.Value}%") ||
                    EF.Functions.Like(s.LocalizedName, $"%{ctx.Value}%") ||
                    EF.Functions.Like(s.SortName, $"%{ctx.Value}%"))
                .WithComparison(FilterComparison.BeginsWith, ctx => s =>
                    EF.Functions.Like(s.Name, $"{ctx.Value}%") ||
                    EF.Functions.Like(s.OriginalName, $"{ctx.Value}%") ||
                    EF.Functions.Like(s.LocalizedName, $"{ctx.Value}%") ||
                    EF.Functions.Like(s.SortName, $"{ctx.Value}%"))
                .WithComparison(FilterComparison.EndsWith, ctx => s =>
                    EF.Functions.Like(s.Name, $"%{ctx.Value}") ||
                    EF.Functions.Like(s.OriginalName, $"%{ctx.Value}") ||
                    EF.Functions.Like(s.LocalizedName, $"%{ctx.Value}") ||
                    EF.Functions.Like(s.SortName, $"%{ctx.Value}"))
                .WithComparison(FilterComparison.Equal, ctx => s =>
                    s.Name == ctx.Value ||
                    s.OriginalName == ctx.Value ||
                    s.LocalizedName == ctx.Value ||
                    s.SortName == ctx.Value)
                .WithComparison(FilterComparison.NotEqual, ctx => s =>
                    s.Name != ctx.Value &&
                    s.OriginalName != ctx.Value &&
                    s.LocalizedName != ctx.Value &&
                    s.SortName != ctx.Value)
                .Build())
            .WithField(EntityFilterField.Path, new FilterFieldBuilder<Series, string>(s => s.NormalizePath())
                .WithComparison(FilterComparison.Equal, ctx => s => s.FolderPath != null && s.FolderPath == ctx.Value)
                .WithComparison(FilterComparison.BeginsWith, ctx => s => s.FolderPath != null && EF.Functions.Like(s.FolderPath, $"{ctx.Value}%"))
                .WithComparison(FilterComparison.EndsWith, ctx => s => s.FolderPath != null && EF.Functions.Like(s.FolderPath, $"%{ctx.Value}"))
                .WithComparison(FilterComparison.Matches, ctx => s => s.FolderPath != null && EF.Functions.Like(s.FolderPath, $"%{ctx.Value}%"))
                .WithComparison(FilterComparison.NotEqual, ctx => s => s.FolderPath != null && s.FolderPath != ctx.Value)
                .Build())
            .WithField(EntityFilterField.FilePath, new FilterFieldBuilder<Series, string>(s => s.NormalizePath())
                .WithComparison(FilterComparison.Equal, ctx => s =>
                    s.Volumes.Any(v => v.Chapters.Any(c => c.Files.Any(f => f.FilePath != null && f.FilePath == ctx.Value))))
                .WithComparison(FilterComparison.BeginsWith, ctx => s =>
                    s.Volumes.Any(v => v.Chapters.Any(c => c.Files.Any(f => f.FilePath != null && EF.Functions.Like(f.FilePath, $"{ctx.Value}%")))))
                .WithComparison(FilterComparison.EndsWith, ctx => s =>
                    s.Volumes.Any(v => v.Chapters.Any(c => c.Files.Any(f => f.FilePath != null && EF.Functions.Like(f.FilePath, $"%{ctx.Value}")))))
                .WithComparison(FilterComparison.Matches, ctx => s =>
                    s.Volumes.Any(v => v.Chapters.Any(c => c.Files.Any(f => f.FilePath != null && EF.Functions.Like(f.FilePath, $"%{ctx.Value}%")))))
                .WithComparison(FilterComparison.NotEqual, ctx => s =>
                    s.Volumes.Any(v => v.Chapters.Any(c => c.Files.Any(f => f.FilePath != null && f.FilePath != ctx.Value))))
                .Build())
            .WithField(EntityFilterField.PublicationStatus, new EnumArrayFilterFieldBuilder<Series, PublicationStatus>()
                .WithGuard(ctx => ctx.Value.Count > 0)
                .WithComparison(FilterComparison.Equal, ctx => s => s.Metadata.PublicationStatus == ctx.Value[0])
                .WithComparison(FilterComparison.Contains, ctx => s => ctx.Value.Contains(s.Metadata.PublicationStatus))
                .WithComparison(FilterComparison.NotContains, ctx => s => !ctx.Value.Contains(s.Metadata.PublicationStatus))
                .WithComparison(FilterComparison.NotEqual, ctx => s => s.Metadata.PublicationStatus != ctx.Value[0])
                .Build())
            .WithField(EntityFilterField.AgeRating, new EnumArrayFilterFieldBuilder<Series, AgeRating>()
                .WithGuard(ctx => ctx.Value.Count > 0)
                .WithComparison(FilterComparison.LessThan, ctx => s => s.Metadata.AgeRating < ctx.Value[0])
                .WithComparison(FilterComparison.LessThanEqual, ctx => s => s.Metadata.AgeRating <= ctx.Value[0])
                .WithComparison(FilterComparison.Equal, ctx => s => s.Metadata.AgeRating == ctx.Value[0])
                .WithComparison(FilterComparison.GreaterThanEqual, ctx => s => s.Metadata.AgeRating > ctx.Value[0])
                .WithComparison(FilterComparison.GreaterThan, ctx => s => s.Metadata.AgeRating <= ctx.Value[0])
                .WithComparison(FilterComparison.Contains, ctx => s => ctx.Value.Contains(s.Metadata.AgeRating))
                .Build())
            .WithField(EntityFilterField.CollectionTags, new NopComparisionField<Series>()
                .WithComparisons(FilterComparison.Contains, FilterComparison.NotContains, FilterComparison.NotEqual))
            .WithField(EntityFilterField.Libraries, new NopComparisionField<Series>()
                .WithComparisons(FilterComparison.Contains, FilterComparison.NotContains, FilterComparison.NotEqual))
            .WithField(EntityFilterField.WantToRead, new NopComparisionField<Series>()
                .WithComparisons(FilterComparison.Contains, FilterComparison.NotContains, FilterComparison.NotEqual))
            .WithField(EntityFilterField.ReadProgress, new SeriesComparisonField())
            .Build())
        .WithEntity(FilterEntity.Chapters, new FilterEntityBuilder<Series>()
            .WithField(EntityFilterField.FileSize, new FilterFieldBuilder<Series, long>(s => s.ParseHumanReadableBytes())
                .WithGuard(ctx => ctx.Value > 0)
                .WithComparison(FilterComparison.GreaterThan, ctx => s => s.Volumes
                    .Any(v => v.Chapters.Any(c => c.Files.Sum(f => f.Bytes) > ctx.Value)))
                .Build())
            .Build())
        .Build();

    private static readonly IFilterPipeline<Chapter> ChapterFilterPipeline = new FilterPipelineBuilder<Chapter>()
        .WithEntity(FilterEntity.Chapters, new FilterEntityBuilder<Chapter>()
            .WithField(EntityFilterField.FileSize, new FilterFieldBuilder<Chapter, long>(s => s.ParseHumanReadableBytes())
                .WithComparison(FilterComparison.GreaterThan, ctx => s => s.Files.Sum(f => f.Bytes) > ctx.Value)
                .Build())
            .WithField(EntityFilterField.ReadProgress, new ChapterComparisonField())
            .Build())
        .WithEntity(FilterEntity.Series, new FilterEntityBuilder<Chapter>()
            .WithField(EntityFilterField.SeriesName, new StringFilterFieldBuilder<Chapter>()
                .WithGuard(ctx => !string.IsNullOrWhiteSpace(ctx.Value))
                .WithComparison(FilterComparison.Contains, ctx => c => EF.Functions.Like(c.Volume.Series.Name, $"%{ctx.Value}%"))
                .Build())
            .Build())
        .Build();

    public async Task<FilterResponse> Filter(int userId, EntityFilterDto entityFilter)
    {
        var seriesTask = ApplyFilterWithProgress<Series, SeriesDto>(FilterEntity.Series, SeriesFilterPipeline, entityFilter, userId);
        var chapterTask = ApplyFilterWithProgress<Chapter, ChapterDto>(FilterEntity.Chapters, ChapterFilterPipeline, entityFilter, userId);

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
        IFilterPipeline<TEntity> filterPipeline, EntityFilterDto entityFilter, int userId)
    where TEntity : class
    {
        if (!entityFilter.RequestedEntities.Contains(entity))
            return Task.FromResult<List<TEntityDto>>([]);

        return filterPipeline.Apply(dataContext.Set<TEntity>().AsNoTracking(), entityFilter, userId)
            .ProjectToWithProgress<TEntity, TEntityDto>(mapper, userId)
            .ToListAsync();
    }
}
