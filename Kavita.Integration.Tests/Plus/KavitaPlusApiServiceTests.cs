using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using Kavita.Common;
using Kavita.Common.Extensions;
using Kavita.Integration.Tests.Fixtures;
using Kavita.Models.DTOs.KavitaPlus;
using Kavita.Models.DTOs.KavitaPlus.ExternalMetadata;
using Kavita.Models.DTOs.KavitaPlus.ExternalMetadata.Covers;
using Kavita.Models.DTOs.KavitaPlus.Metadata;
using Kavita.Models.DTOs.Metadata.Matching;
using Kavita.Models.DTOs.Scrobbling;
using Xunit.Abstractions;

namespace Kavita.Integration.Tests.Plus;

[Collection("KavitaPlus")]
[Trait("Category", "Integration")]
public sealed class KavitaPlusApiServiceTests(KavitaPlusFixture fixture, ITestOutputHelper output)
{
    private void SkipIfNoLicense() =>
        Skip.If(fixture.LicenseKey is null, fixture.SkipReason ?? "No license key available.");

    // -------------------------------------------------------------------------
    // Match Series
    // -------------------------------------------------------------------------

    [SkippableFact]
    public async Task MatchSeries_KnownMangaName_ReturnsNonEmptyList()
    {
        SkipIfNoLicense();

        var request = new MatchSeriesRequestDto
        {
            SeriesName = "Berserk",
            Format = PlusMediaFormat.Manga
        };

        var results = await (fixture.ApiUrl + "/api/metadata/v2/match-series")
            .WithKavitaPlusHeaders(fixture.LicenseKey!)
            .PostJsonAsync(request)
            .ReceiveJson<IList<ExternalSeriesMatchDto>>();

        output.WriteLine($"Matches returned: {results?.Count}");
        Assert.NotNull(results);
        Assert.NotEmpty(results);
    }

    // -------------------------------------------------------------------------
    // Series Detail (by request DTO)
    // -------------------------------------------------------------------------

    [SkippableFact]
    public async Task GetSeriesDetail_BerserkByAniListId_ReturnsSeries()
    {
        SkipIfNoLicense();

        var request = new PlusSeriesRequestDto
        {
            AniListId = 30, // AniList ID 30 = Berserk
            SeriesName = "Berserk",
            MediaFormat = PlusMediaFormat.Manga
        };

        var detail = await (fixture.ApiUrl + "/api/metadata/v2/series-detail")
            .WithKavitaPlusHeaders(fixture.LicenseKey!)
            .PostJsonAsync(request)
            .ReceiveJson<SeriesDetailPlusApiDto>();

        output.WriteLine($"Series={detail?.Series?.Name} AniListId={detail?.AniListId}");
        Assert.NotNull(detail);
        Assert.NotNull(detail.Series);
        Assert.False(string.IsNullOrEmpty(detail.Series.Name));
    }

    // -------------------------------------------------------------------------
    // Series Detail (by external IDs)
    // -------------------------------------------------------------------------

    [SkippableFact]
    public async Task GetSeriesDetailById_BerserkByMalId_ReturnsDetail()
    {
        SkipIfNoLicense();

        var request = new ExternalMetadataIdsDto
        {
            MalId = 2, // MAL ID 2 = Berserk
            PlusMediaFormat = PlusMediaFormat.Manga
        };

        var detail = await (fixture.ApiUrl + "/api/metadata/v2/series-by-ids")
            .WithKavitaPlusHeaders(fixture.LicenseKey!)
            .PostJsonAsync(request)
            .ReceiveJson<ExternalSeriesDetailDto>();

        output.WriteLine($"Name={detail?.Name} AniListId={detail?.AniListId}");
        Assert.NotNull(detail);
        Assert.False(string.IsNullOrEmpty(detail.Name));
    }

    // -------------------------------------------------------------------------
    // Cover Images
    // -------------------------------------------------------------------------

    [SkippableFact]
    public async Task GetCoverImages_BerserkSeries_ReturnsCoverUrls()
    {
        SkipIfNoLicense();

        var request = new ExternalCoverRequestDto
        {
            SeriesName = "Berserk",
            AniListId = 30,
            MediaFormat = PlusMediaFormat.Manga,
            IsStandAlone = false
        };

        var result = await (fixture.ApiUrl + "/api/v3/metadata/covers")
            .WithKavitaPlusHeaders(fixture.LicenseKey!)
            .PostJsonAsync(request)
            .ReceiveJson<KPlusResult<IList<ExternalCoverResponseDto>>>();

        output.WriteLine($"IsSuccess={result?.IsSuccess} Count={result?.Data?.Count}");
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, result.ErrorMessage ?? "K+ returned failure");
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
        Assert.All(result.Data, cover => Assert.False(string.IsNullOrEmpty(cover.Url)));
    }
}
