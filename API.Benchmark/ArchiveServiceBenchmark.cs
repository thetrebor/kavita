using System;
using System.IO;
using System.IO.Abstractions;
using API.Entities.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using API.Services;
using API.Services.ImageServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using EasyCaching.Core;
using NSubstitute;


namespace API.Benchmark;

[StopOnFirstError]
[MemoryDiagnoser]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(launchCount: 1, warmupCount: 5, invocationCount: 20)]
public class ArchiveServiceBenchmark
{
    private readonly ArchiveService _archiveService;
    private readonly IDirectoryService _directoryService;
    private readonly IImageService _imageService;
    private readonly IImageFactory _imageFactory;
    private const string SourceImage = "C:/Users/josep/Pictures/obey_by_grrsa-d6llkaa_colored_by_me.png";


    public ArchiveServiceBenchmark()
    {
#if ImageMagick
        _imageFactory = new API.Services.ImageServices.ImageMagick.ImageMagickImageFactory();
#else
        _imageFactory = new API.Services.ImageServices.NetVips.NetVipsImageFactory();
#endif

        _directoryService = new DirectoryService(null, new FileSystem());
        _imageService = new ImageService(null, _directoryService,_imageFactory);
        _archiveService = new ArchiveService(new NullLogger<ArchiveService>(), _directoryService, _imageService, Substitute.For<IMediaErrorService>());
       
    }

    [Benchmark(Baseline = true)]
    public void TestGetComicInfo_baseline()
    {
        if (_archiveService.GetComicInfo("Data/ComicInfo.zip") == null) {
            throw new Exception("ComicInfo not found");
        }
    }

    [Benchmark]
    public void TestGetComicInfo_duplicate()
    {
        if (_archiveService.GetComicInfo("Data/ComicInfo_duplicateInfos.zip") == null) {
            throw new Exception("ComicInfo not found");
        }
    }

    [Benchmark]
    public void TestGetComicInfo_outside_root()
    {
        if (_archiveService.GetComicInfo("Data/ComicInfo_outside_root.zip") == null) {
            throw new Exception("ComicInfo not found");
        }
    }

    [Benchmark]
    public void ImageFactory_ExtractImage_PNG()
    {
        var outputDirectory = "C:/Users/josep/Pictures/netvips/";
        _directoryService.ExistOrCreate(outputDirectory);

        using var stream = new FileStream(SourceImage, FileMode.Open);
        using var thumbnail2 = _imageFactory.Create(stream);
        thumbnail2.Resize(320, 0);
        thumbnail2.Save(_directoryService.FileSystem.Path.Join(outputDirectory, "imagesharp.png"), EncodeFormat.PNG);
    }

    private void Resize320(IImage image)
    {
        int originalWidth = image.Width;
        int originalHeight = image.Height;
        int targetWidth = 320;
        int targetHeight = (int) Math.Round((double)originalHeight * targetWidth / originalWidth);
        image.Resize(targetWidth, targetHeight);
    }

    [Benchmark]
    public void ImageSharp_ExtractImage_WebP()
    {
        var outputDirectory = "C:/Users/josep/Pictures/netvips/";
        _directoryService.ExistOrCreate(outputDirectory);

        using var stream = new FileStream(SourceImage, FileMode.Open);
        using var thumbnail2 = _imageFactory.Create(stream);
        Resize320(thumbnail2);
        thumbnail2.Save(_directoryService.FileSystem.Path.Join(outputDirectory, "imagesharp.webp"), EncodeFormat.WEBP);
    }



    // Benchmark to test default GetNumberOfPages from archive
    // vs a new method where I try to open the archive and return said stream
}
