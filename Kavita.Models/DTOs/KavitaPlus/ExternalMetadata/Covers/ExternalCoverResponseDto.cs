namespace Kavita.Models.DTOs.KavitaPlus.ExternalMetadata.Covers;
#nullable enable

public sealed record ExternalCoverResponseDto
{
    public required string Url { get; set; } = string.Empty;
    /// <summary>
    /// "series" for the series-level cover; otherwise the MangaBaka image type
    /// (volume, volume_back, banner, chapter, season, audiobook, other).
    /// </summary>
    public string? Type { get; set; }
    public float? VolumeNumber { get; set; }
    public string? Language { get; set; }
}
