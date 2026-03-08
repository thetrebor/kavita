using System;
using Kavita.Models.Entities.Enums;
using Kavita.Models.Entities.Interfaces;

namespace Kavita.Models.DTOs.ReadingLists;
#nullable enable

public sealed record ReadingListItemDto : IHasDisplayTitle
{
    /// <inheritdoc cref="IHasDisplayTitle.DisplayNumber"/>
    public string DisplayNumber { get; set; } = string.Empty;
    /// <inheritdoc cref="IHasDisplayTitle.DisplayTitle"/>
    public string DisplayTitle { get; set; } = string.Empty;
    /// <inheritdoc cref="IHasDisplayTitle.MetaTitle"/>
    public string MetaTitle { get; set; } = string.Empty;
    public int Id { get; init; }
    public int Order { get; init; }
    public int ChapterId { get; init; }
    public int SeriesId { get; init; }
    public string? SeriesName { get; set; }
    public string? SeriesSortName { get; set; }
    public MangaFormat SeriesFormat { get; set; }
    public int PagesRead { get; set; }
    public int PagesTotal { get; set; }
    public string? ChapterNumber { get; set; }
    public string? VolumeNumber { get; set; }
    public string? ChapterTitleName { get; set; }
    public int VolumeId { get; set; }
    public int LibraryId { get; set; }
    [Obsolete("Use DisplayTitle")]
    public string? Title { get; set; }
    public LibraryType LibraryType { get; set; }
    public string? LibraryName { get; set; }
    /// <summary>
    /// Release Date from Chapter
    /// </summary>
    public DateTime? ReleaseDate { get; set; }
    /// <summary>
    /// Used internally only
    /// </summary>
    public int ReadingListId { get; set; }
    /// <summary>
    /// The last time a reading list item (underlying chapter) was read by current authenticated user
    /// </summary>
    public DateTime? LastReadingProgressUtc { get; set; }
    /// <summary>
    /// File size of underlying item
    /// </summary>
    /// <remarks>This is only used for CDisplayEx</remarks>
    public long FileSize { get; set; }
    /// <summary>
    /// The chapter summary
    /// </summary>
    public string? Summary { get; set; }

    public bool IsSpecial { get; set; }
}
