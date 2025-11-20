namespace API.DTOs.Statistics;

public sealed record MostReadAuthorsDto
{

    public int AuthorId { get; init; }
    public string AuthorName { get; init; }
    public int TotalChaptersRead { get; init; }
    //public int AverageRating { get; init; }

    /**
     * Comma seperated list of ids
     */
    public string Chapters { get; init; }

}
