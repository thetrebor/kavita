using System.Collections.Generic;
using Kavita.Models.Entities.Enums;

namespace Kavita.Models.DTOs.Metadata.Matching;

/// <summary>
/// Replaces the full set of <see cref="MetadataProvider"/>s a Series is excluded from being matched against.
/// </summary>
public sealed record UpdateSeriesMetadataProviderExclusionsDto
{
    /// <summary>
    /// Series to update
    /// </summary>
    public int SeriesId { get; set; }
    /// <summary>
    /// The full desired set of excluded providers. An absent provider is allowed.
    /// </summary>
    public IList<MetadataProvider> Excluded { get; set; } = new List<MetadataProvider>();
}
