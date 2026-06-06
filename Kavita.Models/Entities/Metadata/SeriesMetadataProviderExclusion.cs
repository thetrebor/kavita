using System;
using Kavita.Models.Entities.Enums;
using Kavita.Models.Entities.Interfaces;

namespace Kavita.Models.Entities.Metadata;

/// <summary>
/// Records that a <see cref="Series"/> is excluded from being matched against a given <see cref="MetadataProvider"/>.
/// </summary>
/// <remarks>
/// A row only exists for an excluded Provider, absence means the provider is allowed. This governs matching.
/// Scrobble eligibility is derived (external id present and the corresponding metadata provider not excluded).
/// </remarks>
public class SeriesMetadataProviderExclusion : IEntityDate
{
    public int Id { get; set; }
    /// <summary>
    /// The metadata provider this Series will not be matched against.
    /// </summary>
    public MetadataProvider Provider { get; set; }

    public int SeriesId { get; set; }
    public Series Series { get; set; } = null!;

    public DateTime Created { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime LastModifiedUtc { get; set; }
}
