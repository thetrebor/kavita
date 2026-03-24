using System.Collections.Generic;

namespace Kavita.Models.DTOs.Filtering.v3;

public sealed record FilterResponse
{

    public List<SeriesDto> Series { get; set; }
    public List<ChapterDto> Chapters { get; set; }

}
