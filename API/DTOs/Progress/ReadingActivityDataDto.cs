using System;
using API.DTOs.Misc;

namespace API.DTOs.Progress;
#nullable enable

public class ReadingActivityDataDto
{
    public int ChapterId { get; set; }
    public int VolumeId { get; set; }
    public int SeriesId { get; set; }
    public int LibraryId { get; set; }
    public int StartPage { get; set; }
    public int EndPage { get; set; }
    public string? StartBookScrollId { get; set; }
    public string? EndBookScrollId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime? EndTimeUtc { get; set; }
    public int PagesRead { get; set; }
    /// <summary>
    /// Only applicable for Book entries
    /// </summary>
    public int WordsRead { get; set; }
    /// <summary>
    /// Client information for this reading activity.
    /// Tracks device, browser, and authentication details.
    /// </summary>
    public ClientInfoDto? ClientInfo { get; set; }
}
