using System.Collections.Generic;
using Kavita.Models.Entities.Enums;

namespace Kavita.Services.Plus;

/// <summary>
/// Maps a <see cref="ScrobbleProvider"/> to the <see cref="MetadataProvider"/> that controls it, if any.
/// </summary>
/// <remarks>
/// AniList and Mal have no controlling metadata provider (Mangabaka's data always carries their ids), so they are
/// governed purely by id-presence and can never be muted by a <see cref="MetadataProvider"/> exclusion.
/// </remarks>
public static class ScrobbleProviderMap
{
    private static readonly Dictionary<ScrobbleProvider, MetadataProvider?> Source = new()
    {
        [ScrobbleProvider.AniList] = null,
        [ScrobbleProvider.Mal] = null,
        [ScrobbleProvider.Hardcover] = MetadataProvider.Hardcover,
        [ScrobbleProvider.MangaBaka] = MetadataProvider.Mangabaka,
    };

    /// <summary>
    /// Providers whose scrobble Id lives on the <see cref="Series"/> (AniList/Mal/MangaBaka). Hardcover is
    /// chapter-scoped (scrobbles <c>chapter.HardcoverId</c>, parsed from a different weblink than
    /// <c>series.HardcoverId</c>), so a series-level id-presence check cannot gate it.
    /// </summary>
    private static readonly HashSet<ScrobbleProvider> SeriesScopedIdProviders =
    [
        ScrobbleProvider.AniList, ScrobbleProvider.Mal, ScrobbleProvider.MangaBaka
    ];

    /// <summary>
    /// The metadata provider that controls this scrobble provider, or null if it is governed purely by id-presence.
    /// </summary>
    public static MetadataProvider? SourceOf(ScrobbleProvider provider) => Source.GetValueOrDefault(provider);

    /// <summary>
    /// Whether this provider's scrobble id is authoritative at the <see cref="Series"/> level (and thus can be
    /// gated by a series-level id-presence check). Chapter-scoped providers (Hardcover) return false.
    /// </summary>
    public static bool HasSeriesLevelId(ScrobbleProvider provider) => SeriesScopedIdProviders.Contains(provider);
}
