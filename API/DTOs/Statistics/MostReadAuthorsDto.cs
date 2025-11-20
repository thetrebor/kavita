using System.Collections.Generic;
using API.Entities;

namespace API.DTOs.Statistics;

public sealed record MostReadAuthorsDto
{

    public int AuthorId { get; init; }
    public string AuthorName { get; init; }
    public int TotalChaptersRead { get; init; }
    //public int AverageRating { get; init; }

    public IList<ChapterDto> Chapters { get; init; }

}
