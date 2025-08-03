using System.IO;
using System.Linq;
using System.Text;
using API.Extensions;
using API.Entities.Enums;
using API.Services;
using API.Services.ImageServices;
using Xunit;

namespace API.Tests.Services;

public class ImageServiceTests
{
    private readonly string _testDirectory = Path.Join(Directory.GetCurrentDirectory(), "../../../Services/Test Data/ImageService/Covers");
    private readonly string _testDirectoryColorScapes = Path.Join(Directory.GetCurrentDirectory(), "../../../Services/Test Data/ImageService/ColorScapes");
    private readonly string _testFormatDirectory = Path.Join(Directory.GetCurrentDirectory(), "../../../Services/Test Data/ImageService/Formats");
    private const string OutputPattern = "_output";
    private const string BaselinePattern = "_baseline";
#if ImageMagick
    private IImageFactory _imageFactory = new API.Services.ImageServices.ImageMagick.ImageMagickImageFactory();
#else
    private IImageFactory _imageFactory = new API.Services.ImageServices.NetVips.NetVipsImageFactory();
#endif


    /// <summary>
    /// Run this once to get the baseline generation
    /// </summary>
    [Fact]
    public void GenerateBaseline()
    {
        GenerateFiles(BaselinePattern);
        Assert.True(true);
    }

    /// <summary>
    /// Change the Scaling/Crop code then run this continuously
    /// </summary>
    [Fact]
    public void TestScaling()
    {
        GenerateFiles(OutputPattern);
        GenerateHtmlFile();
        Assert.True(true);
    }

    private void GenerateFiles(string outputExtension)
    {
        // Step 1: Delete any images that have _output in the name
        var outputFiles = Directory.GetFiles(_testDirectory, "*_output.*");
        foreach (var file in outputFiles)
        {
            File.Delete(file);
        }

        // Step 2: Scan the _testDirectory for images
        var imageFiles = Directory.GetFiles(_testDirectory, "*.*")
            .Where(file => !file.EndsWith("html"))
            .Where(file => !file.Contains(OutputPattern) && !file.Contains(BaselinePattern))
            .ToList();

        // Step 3: Process each image
        foreach (var imagePath in imageFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(imagePath);
            var dims = CoverImageSize.Default.GetDimensions();
            using var sourceImage = _imageFactory.Create(imagePath);
            using var thumbnail = sourceImage.Thumbnail(dims.Width, dims.Height);
            var outputFileName = fileName + outputExtension + ".png";
            thumbnail.Save(Path.Join(_testDirectory, outputFileName), EncodeFormat.PNG);
        }
    }

    private void GenerateHtmlFile()
    {
        var imageFiles = Directory.GetFiles(_testDirectory, "*.*")
            .Where(file => !file.EndsWith("html"))
            .Where(file => !file.Contains(OutputPattern) && !file.Contains(BaselinePattern))
            .ToList();

        var htmlBuilder = new StringBuilder();
        htmlBuilder.AppendLine("<!DOCTYPE html>");
        htmlBuilder.AppendLine("<html lang=\"en\">");
        htmlBuilder.AppendLine("<head>");
        htmlBuilder.AppendLine("<meta charset=\"UTF-8\">");
        htmlBuilder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        htmlBuilder.AppendLine("<title>Image Comparison</title>");
        htmlBuilder.AppendLine("<style>");
        htmlBuilder.AppendLine("body { font-family: Arial, sans-serif; }");
        htmlBuilder.AppendLine(".container { display: flex; flex-wrap: wrap; }");
        htmlBuilder.AppendLine(".image-row { display: flex; align-items: center; margin-bottom: 20px; width: 100% }");
        htmlBuilder.AppendLine(".image-row img { margin-right: 10px; max-width: 200px; height: auto; }");
        htmlBuilder.AppendLine("</style>");
        htmlBuilder.AppendLine("</head>");
        htmlBuilder.AppendLine("<body>");
        htmlBuilder.AppendLine("<div class=\"container\">");

        foreach (var imagePath in imageFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(imagePath);
            var baselinePath = Path.Combine(_testDirectory, fileName + "_baseline.png");
            var outputPath = Path.Combine(_testDirectory, fileName + "_output.png");
            var dims = CoverImageSize.Default.GetDimensions();

            using var sourceImage = _imageFactory.Create(imagePath);
            htmlBuilder.AppendLine("<div class=\"image-row\">");
            htmlBuilder.AppendLine($"<p>{fileName} ({((double) sourceImage.Width / sourceImage.Height).ToString("F2")}) - {sourceImage.WillScaleWell(dims.Width, dims.Height)}</p>");
            htmlBuilder.AppendLine($"<img src=\"./{Path.GetFileName(imagePath)}\" alt=\"{fileName}\">");
            if (File.Exists(baselinePath))
            {
                htmlBuilder.AppendLine($"<img src=\"./{Path.GetFileName(baselinePath)}\" alt=\"{fileName} baseline\">");
            }
            if (File.Exists(outputPath))
            {
                htmlBuilder.AppendLine($"<img src=\"./{Path.GetFileName(outputPath)}\" alt=\"{fileName} output\">");
            }
            htmlBuilder.AppendLine("</div>");
        }

        htmlBuilder.AppendLine("</div>");
        htmlBuilder.AppendLine("</body>");
        htmlBuilder.AppendLine("</html>");

        File.WriteAllText(Path.Combine(_testDirectory, "index.html"), htmlBuilder.ToString());
    }


    [Fact]
    public void TestColorScapes()
    {
        // Step 1: Delete any images that have _output in the name
        var outputFiles = Directory.GetFiles(_testDirectoryColorScapes, "*_output.*");
        foreach (var file in outputFiles)
        {
            File.Delete(file);
        }

        // Step 2: Scan the _testDirectory for images
        var imageFiles = Directory.GetFiles(_testDirectoryColorScapes, "*.*")
            .Where(file => !file.EndsWith("html"))
            .Where(file => !file.Contains(OutputPattern) && !file.Contains(BaselinePattern))
            .ToList();

        // Step 3: Process each image
        foreach (var imagePath in imageFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(imagePath);
            ImageService imageService = new ImageService(null, null, _imageFactory);

            var colors = imageService.CalculateColorScape(imagePath);

            // Generate primary color image
            GenerateColorImage(colors.Primary, Path.Combine(_testDirectoryColorScapes, $"{fileName}_primary_output.png"));

            // Generate secondary color image
            GenerateColorImage(colors.Secondary, Path.Combine(_testDirectoryColorScapes, $"{fileName}_secondary_output.png"));
        }

        // Step 4: Generate HTML file
        GenerateHtmlFileForColorScape();
        Assert.True(true);
    }

    private void GenerateColorImage(string hexColor, string outputPath)
    {
        var (r, g, b) = ImageService.HexToRgb(hexColor);
        using var colorImage = _imageFactory.Create(200, 100, (byte)r, (byte)g, (byte)b);
        colorImage.Save(outputPath, EncodeFormat.PNG);
    }

    private void GenerateHtmlFileForColorScape()
    {
        var imageFiles = Directory.GetFiles(_testDirectoryColorScapes, "*.*")
            .Where(file => !file.EndsWith("html"))
            .Where(file => !file.Contains(OutputPattern) && !file.Contains(BaselinePattern))
            .ToList();

        var htmlBuilder = new StringBuilder();
        htmlBuilder.AppendLine("<!DOCTYPE html>");
        htmlBuilder.AppendLine("<html lang=\"en\">");
        htmlBuilder.AppendLine("<head>");
        htmlBuilder.AppendLine("<meta charset=\"UTF-8\">");
        htmlBuilder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        htmlBuilder.AppendLine("<title>Color Scape Comparison</title>");
        htmlBuilder.AppendLine("<style>");
        htmlBuilder.AppendLine("body { font-family: Arial, sans-serif; }");
        htmlBuilder.AppendLine(".container { display: flex; flex-wrap: wrap; }");
        htmlBuilder.AppendLine(".image-row { display: flex; align-items: center; margin-bottom: 20px; width: 100% }");
        htmlBuilder.AppendLine(".image-row img { margin-right: 10px; max-width: 200px; height: auto; }");
        htmlBuilder.AppendLine(".color-square { width: 100px; height: 100px; margin-right: 10px; }");
        htmlBuilder.AppendLine("</style>");
        htmlBuilder.AppendLine("</head>");
        htmlBuilder.AppendLine("<body>");
        htmlBuilder.AppendLine("<div class=\"container\">");

        foreach (var imagePath in imageFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(imagePath);
            var primaryPath = Path.Combine(_testDirectoryColorScapes, $"{fileName}_primary_output.png");
            var secondaryPath = Path.Combine(_testDirectoryColorScapes, $"{fileName}_secondary_output.png");

            htmlBuilder.AppendLine("<div class=\"image-row\">");
            htmlBuilder.AppendLine($"<p>{fileName}</p>");
            htmlBuilder.AppendLine($"<img src=\"./{Path.GetFileName(imagePath)}\" alt=\"{fileName}\">");
            if (File.Exists(primaryPath))
            {
                htmlBuilder.AppendLine($"<img class=\"color-square\" src=\"./{Path.GetFileName(primaryPath)}\" alt=\"{fileName} primary color\">");
            }
            if (File.Exists(secondaryPath))
            {
                htmlBuilder.AppendLine($"<img class=\"color-square\" src=\"./{Path.GetFileName(secondaryPath)}\" alt=\"{fileName} secondary color\">");
            }
            htmlBuilder.AppendLine("</div>");
        }

        htmlBuilder.AppendLine("</div>");
        htmlBuilder.AppendLine("</body>");
        htmlBuilder.AppendLine("</html>");

        File.WriteAllText(Path.Combine(_testDirectoryColorScapes, "colorscape_index.html"), htmlBuilder.ToString());
    }

    [Fact]
    public void TestLoadAvifFormat()
    {
        var avifPath = Path.Combine(_testFormatDirectory, "cover.avif");
        
        try
        {
            using var image = _imageFactory.Create(avifPath);
            Assert.NotNull(image);
            Assert.True(image.Width > 0);
            Assert.True(image.Height > 0);
        }
        catch
        {
            Assert.Fail("Failed to load AVIF image format");
        }
    }

    [Fact]
    public void TestLoadHeifFormat()
    {
        var heifPath = Path.Combine(_testFormatDirectory, "cover.heif");
        
        try
        {
            using var image = _imageFactory.Create(heifPath);
            Assert.NotNull(image);
            Assert.True(image.Width > 0);
            Assert.True(image.Height > 0);
        }
        catch
        {
            Assert.Fail("Failed to load HEIF image format");
        }
    }

    [Fact]
    public void TestLoadJp2Format()
    {
        var jp2Path = Path.Combine(_testFormatDirectory, "cover.jp2");
        
        try
        {
            using var image = _imageFactory.Create(jp2Path);
            Assert.NotNull(image);
            Assert.True(image.Width > 0);
            Assert.True(image.Height > 0);
        }
        catch
        {
            Assert.Fail("Failed to load Jpeg 2000 image format");
        }
    }

    [Fact]
    public void TestLoadJpgFormat()
    {
        var jpgPath = Path.Combine(_testFormatDirectory, "cover.jpg");
        
        try
        {
            using var image = _imageFactory.Create(jpgPath);
            Assert.NotNull(image);
            Assert.True(image.Width > 0);
            Assert.True(image.Height > 0);
        }
        catch
        {
            Assert.Fail("Failed to load JPEG image format");
        }
    }

    [Fact]
    public void TestLoadJxlFormat()
    {
        var jxlPath = Path.Combine(_testFormatDirectory, "cover.jxl");
        
        try
        {
            using var image = _imageFactory.Create(jxlPath);
            Assert.NotNull(image);
            Assert.True(image.Width > 0);
            Assert.True(image.Height > 0);
        }
        catch
        {
            Assert.Fail("Failed to load JXL image format");
        }
    }

    [Fact]
    public void TestLoadPngFormat()
    {
        var pngPath = Path.Combine(_testFormatDirectory, "cover.png");
        
        try
        {
            using var image = _imageFactory.Create(pngPath);
            Assert.NotNull(image);
            Assert.True(image.Width > 0);
            Assert.True(image.Height > 0);
        }
        catch
        {
            Assert.Fail("Failed to load PNG image format");
        }
    }

    [Fact]
    public void TestLoadWebpFormat()
    {
        var webpPath = Path.Combine(_testFormatDirectory, "cover.webp");
        
        try
        {
            using var image = _imageFactory.Create(webpPath);
            Assert.NotNull(image);
            Assert.True(image.Width > 0);
            Assert.True(image.Height > 0);
        }
        catch
        {
            Assert.Fail("Failed to load WebP image format");
        }
    }

    [Fact]
    public void TestLoadGifFormat()
    {
        var gifPath = Path.Combine(_testFormatDirectory, "cover.gif");
        
        try
        {
            using var image = _imageFactory.Create(gifPath);
            Assert.NotNull(image);
            Assert.True(image.Width > 0);
            Assert.True(image.Height > 0);
        }
        catch
        {
            Assert.Fail("Failed to load GIF image format");
        }
    }

    [Fact]
    public void TestLoadTiffFormat()
    {
        var tiffPath = Path.Combine(_testFormatDirectory, "cover.tiff");
        
        try
        {
            using var image = _imageFactory.Create(tiffPath);
            Assert.NotNull(image);
            Assert.True(image.Width > 0);
            Assert.True(image.Height > 0);
        }
        catch
        {
            Assert.Fail("Failed to load TIFF image format");
        }
    }


}
