using API.Entities.Enums;
using API.Services.ImageServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using API.Services;

namespace API.Extensions;

public static class ImageExtensions
{

    /// <summary>
    /// Structure to hold various image quality metrics
    /// </summary>
    private sealed class ImageQualityMetrics
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsColor { get; set; }
        public double Colorfulness { get; set; }
        public double Contrast { get; set; }
        public double Sharpness { get; set; }
        public double NoiseLevel { get; set; }
    }


    /// <summary>
    /// Calculate a similarity score (0-1f) based on resolution difference and MSE.
    /// </summary>
    /// <param name="imagePath1">Path to first image</param>
    /// <param name="imagePath2">Path to the second image</param>
    /// <returns>Similarity score between 0-1, where 1 is identical</returns>
    public static float CalculateSimilarity(this IImageService imageService, string imagePath1, string imagePath2)
    {
        if (!File.Exists(imagePath1) || !File.Exists(imagePath2))
        {
            throw new FileNotFoundException("One or both image files do not exist");
        }
        using var im1 = imageService.ImageFactory.Create(imagePath1);
        using var im2 = imageService.ImageFactory.Create(imagePath2);
        var res1 = im1.Width * im1.Height;
        var res2 = im2.Width * im2.Height;
        var resolutionDiff = Math.Abs(res1 - res2) / (float)Math.Max(res1, res2);
        if (im1.Width != im2.Width || im1.Height != im2.Height)
        {
            im2.Resize(im1.Width, im1.Height);
        }

        // Calculate resolution difference factor

        var img1 = im1.GetRGBAImageData();
        var img2 = im2.GetRGBAImageData();
        // Calculate mean squared error for pixel differences
        var mse = img1.GetMeanSquaredError(img2, res1);

        // Normalize MSE (65025 = 255², which is the max possible squared difference per channel)
        var normalizedMse = 1f - Math.Min(1f, mse / 65025f);

        // Final similarity score (weighted average of resolution difference and color difference)
        return Math.Max(0f, 1f - (resolutionDiff * 0.5f) - (1f - normalizedMse) * 0.5f);
    }

    /// <summary>
    /// Smaller is better
    /// </summary>
    /// <param name="img1"></param>
    /// <param name="img2"></param>
    /// <returns></returns>
    public static float GetMeanSquaredError(this float[] img1, float[] img2, int pixelCount)
    {
        double totalDiff = 0;
        for (int x = 0; x < img1.Length; x += 4)
        {
            var r = img1[x] - img2[x];
            var g = img1[x + 1] - img2[x + 1];
            var b = img1[x + 2] - img2[x + 2];
            totalDiff += (r * r) + (g * g) + (b * b);
        }

        return (float)(totalDiff / pixelCount);
    }

    /// <summary>
    /// Determines which image is "better" based on multiple quality factors
    /// using only the cross-platform ImageSharp library
    /// </summary>
    /// <param name="imagePath1">Path to first image</param>
    /// <param name="imagePath2">Path to the second image</param>
    /// <param name="preferColor">Whether to prefer color images over grayscale (default: true)</param>
    /// <returns>The path of the better image</returns>
    public static string GetBetterImage(this IImageService imageService, string imagePath1, string imagePath2, bool preferColor = true)
    {
        if (!File.Exists(imagePath1) || !File.Exists(imagePath2))
        {
            throw new FileNotFoundException("One or both image files do not exist");
        }

        // Quick metadata check to get width/height without loading full pixel data
        var info1 = imageService.ImageFactory.GetDimensions(imagePath1);
        var info2 = imageService.ImageFactory.GetDimensions(imagePath2);

        // Calculate resolution factor
        double resolutionFactor1 = info1.Value.Width * info1.Value.Height;
        double resolutionFactor2 = info2.Value.Width * info2.Value.Height;

        // If one image is significantly higher resolution (3x or more), just pick it
        // This avoids fully loading both images when the choice is obvious
        if (resolutionFactor1 > resolutionFactor2 * 3)
            return imagePath1;
        if (resolutionFactor2 > resolutionFactor1 * 3)
            return imagePath2;

        // Otherwise, we need to analyze the actual image data for both

        // NOTE: We HAVE to use these scope blocks and load image here otherwise memory-mapped section exception will occur
        ImageQualityMetrics metrics1;
        using (var img1 = imageService.ImageFactory.Create(imagePath1))
        {
            metrics1 = GetImageQualityMetrics(img1);
        }

        ImageQualityMetrics metrics2;
        using (var img2 = imageService.ImageFactory.Create(imagePath2))
        {
            metrics2 = GetImageQualityMetrics(img2);
        }


        // If one is color, and one is grayscale, then we prefer color
        if (preferColor && metrics1.IsColor != metrics2.IsColor)
        {
            return metrics1.IsColor ? imagePath1 : imagePath2;
        }

        // Calculate overall quality scores
        var score1 = CalculateOverallScore(metrics1);
        var score2 = CalculateOverallScore(metrics2);

        return score1 >= score2 ? imagePath1 : imagePath2;
    }


    /// <summary>
    /// Calculate a weighted overall score based on metrics
    /// </summary>
    private static double CalculateOverallScore(ImageQualityMetrics metrics)
    {
        // Resolution factor (normalized to HD resolution)
        var resolutionFactor = Math.Min(1.0, (metrics.Width * metrics.Height) / (double) (1920 * 1080));

        // Color factor
        var colorFactor = metrics.IsColor ? (0.5 + 0.5 * metrics.Colorfulness) : 0.3;

        // Quality factors
        var contrastFactor = Math.Min(1.0, metrics.Contrast);
        var sharpnessFactor = Math.Min(1.0, metrics.Sharpness);

        // Noise penalty (less noise is better)
        var noisePenalty = Math.Max(0, 1.0 - metrics.NoiseLevel);

        // Weighted combination
        return (resolutionFactor * 0.35) +
               (colorFactor * 0.3) +
               (contrastFactor * 0.15) +
               (sharpnessFactor * 0.15) +
               (noisePenalty * 0.05);
    }

    /// <summary>
    /// Gets quality metrics for an image
    /// </summary>
    private static ImageQualityMetrics GetImageQualityMetrics(IImage image)
    {
        // Create a smaller version if the image is large to speed up analysis
        float[] workingImage;
        if (image.Width > 512 || image.Height > 512)
        {
            image.Resize(512, 512);
            workingImage = image.GetRGBAImageData();
        }
        else
        {
            workingImage = image.GetRGBAImageData();
        }

        var metrics = new ImageQualityMetrics
        {
            Width = image.Width,
            Height = image.Height
        };

        // Color analysis (is the image color or grayscale?)
        var colorInfo = AnalyzeColorfulness(workingImage, image.Width, image.Height);
        metrics.IsColor = colorInfo.IsColor;
        metrics.Colorfulness = colorInfo.Colorfulness;

        // Contrast analysis
        metrics.Contrast = CalculateContrast(workingImage, image.Width, image.Height);

        // Sharpness estimation
        metrics.Sharpness = EstimateSharpness(workingImage, image.Width, image.Height);

        // Noise estimation
        metrics.NoiseLevel = EstimateNoiseLevel(workingImage, image.Width, image.Height);

        // Clean up
        image.Dispose();

        return metrics;
    }

    /// <summary>
    /// Analyzes colorfulness of an image
    /// </summary>
    private static (bool IsColor, double Colorfulness) AnalyzeColorfulness(float[] rgbaData, int width, int height)
    {
        int totalPixels = width * height;

        if (rgbaData.Length != totalPixels * 4)
            throw new ArgumentException("Invalid RGBA buffer length or dimensions.");

        int sampleSize = Math.Min(1000, totalPixels);
        int stepSize = Math.Max(1, totalPixels / sampleSize);

        int colorCount = 0;
        List<(double R, double G, double B)> samples = [];

        for (int i = 0; i < totalPixels; i += stepSize)
        {
            int index = i * 4;

            double r = rgbaData[index];
            double g = rgbaData[index + 1];
            double b = rgbaData[index + 2];

            double r255 = r * 255;
            double g255 = g * 255;
            double b255 = b * 255;

            // Count pixel as "color" if RGB channel differences are significant
            if (Math.Abs(r255 - g255) > 15 ||
                Math.Abs(r255 - b255) > 15 ||
                Math.Abs(g255 - b255) > 15)
            {
                colorCount++;
            }

            samples.Add((r255, g255, b255));
        }

        if (samples.Count == 0)
            return (false, 0);

        // Opponent channels: rg and yb
        var rg = samples.Select(p => p.R - p.G).ToList();
        var yb = samples.Select(p => 0.5 * (p.R + p.G) - p.B).ToList();

        double rgStd = CalculateStdDev(rg);
        double ybStd = CalculateStdDev(yb);
        double rgMean = rg.Average();
        double ybMean = yb.Average();

        double stdRoot = Math.Sqrt(rgStd * rgStd + ybStd * ybStd);
        double meanRoot = Math.Sqrt(rgMean * rgMean + ybMean * ybMean);

        double colorfulness = stdRoot + 0.3 * meanRoot;
        colorfulness = Math.Min(1.0, colorfulness / 100.0);

        bool isColor = (double)colorCount / samples.Count > 0.05;

        return (isColor, colorfulness);
    }

    /// <summary>
    /// Calculate standard deviation of a list of values
    /// </summary>
    private static double CalculateStdDev(List<int> values)
    {
        var mean = values.Average();
        var sumOfSquaresOfDifferences = values.Select(val => (val - mean) * (val - mean)).Sum();
        return Math.Sqrt(sumOfSquaresOfDifferences / values.Count);
    }

    /// <summary>
    /// Calculate standard deviation of a list of values
    /// </summary>
    private static double CalculateStdDev(List<double> values)
    {
        var mean = values.Average();
        var sumOfSquaresOfDifferences = values.Select(val => (val - mean) * (val - mean)).Sum();
        return Math.Sqrt(sumOfSquaresOfDifferences / values.Count);
    }

    /// <summary>
    /// Calculates contrast of an image
    /// </summary>
    private static double CalculateContrast(float[] rgbaData, int width, int height)
    {
        int totalPixels = width * height;

        if (rgbaData.Length != totalPixels * 4)
            throw new ArgumentException("Invalid image dimensions or RGBA buffer length.");

        int sampleSize = Math.Min(1000, totalPixels);
        int stepSize = Math.Max(1, totalPixels / sampleSize);

        List<double> luminanceValues = new();

        for (int i = 0; i < totalPixels; i += stepSize)
        {
            int index = i * 4;
            float r = rgbaData[index];
            float g = rgbaData[index + 1];
            float b = rgbaData[index + 2];

            // Calculate luminance in 0.0–1.0 space
            double luminance = 0.299 * r + 0.587 * g + 0.114 * b;
            luminanceValues.Add(luminance);
        }

        if (luminanceValues.Count < 2)
            return 0;

        double mean = luminanceValues.Average();
        double sumSq = luminanceValues.Sum(l => Math.Pow(l - mean, 2));
        double rmsContrast = Math.Sqrt(sumSq / luminanceValues.Count) / mean;

        // Clamp to [0.0, 1.0]
        return Math.Min(1.0, rmsContrast);
    }

    /// <summary>
    /// Estimates sharpness using simple Laplacian-based method
    /// </summary>
    private static double EstimateSharpness(float[] rgbaData, int width, int height)
    {
        if (rgbaData.Length != width * height * 4)
            throw new ArgumentException("Invalid image dimensions or data length.");

        float[,] gray = new float[width, height];

        // Convert RGBA to grayscale
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width + x) * 4;
                float r = rgbaData[index];
                float g = rgbaData[index + 1];
                float b = rgbaData[index + 2];
                // float a = rgbaData[index + 3]; // unused

                gray[x, y] = 0.299f * r + 0.587f * g + 0.114f * b;
            }
        }

        // Apply Laplacian filter
        double laplacianSum = 0;
        int validPixels = 0;

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                float laplacian =
                    gray[x, y - 1] +
                    gray[x - 1, y] -
                    4 * gray[x, y] +
                    gray[x + 1, y] +
                    gray[x, y + 1];

                laplacianSum += Math.Abs(laplacian);
                validPixels++;
            }
        }

        if (validPixels == 0)
            return 0;

        double laplacianVariance = laplacianSum / validPixels;

        // Normalize to range [0, 1]
        return Math.Min(1.0, laplacianVariance / 1000.0);
    }

    /// <summary>
    /// Estimates noise level using simple block-based variance method
    /// </summary>
    private static double EstimateNoiseLevel(float[] rgbaData, int width, int height)
    {
        // Block size for noise estimation

        const int blockSize = 8;
        List<double> blockVariances = new();
        // Calculate variance in small blocks throughout the image

        if (rgbaData.Length != width * height * 4)
            throw new ArgumentException("Invalid dimensions or RGBA buffer length.");

        for (int y = 0; y <= height - blockSize; y += blockSize)
        {
            for (int x = 0; x <= width - blockSize; x += blockSize)
            {
                List<double> blockValues = new();

                for (int by = 0; by < blockSize; by++)
                {
                    for (int bx = 0; bx < blockSize; bx++)
                    {
                        int px = x + bx;
                        int py = y + by;
                        int index = (py * width + px) * 4;

                        float r = rgbaData[index];
                        float g = rgbaData[index + 1];
                        float b = rgbaData[index + 2];

                        // Convert to grayscale [0, 1]
                        double gray = 0.299 * r + 0.587 * g + 0.114 * b;
                        blockValues.Add(gray);
                    }
                }
                // Calculate variance of this block

                double mean = blockValues.Average();
                double variance = blockValues.Sum(v => Math.Pow(v - mean, 2)) / blockValues.Count;
                blockVariances.Add(variance);
            }
        }

        if (blockVariances.Count == 0)
            return 0;

        blockVariances.Sort();
        int smoothCount = Math.Max(1, blockVariances.Count / 10);
        double avgNoise = blockVariances.Take(smoothCount).Average();

        // Normalize (heuristically for 0–1 range)
        return Math.Min(1.0, avgNoise / 0.02); // 0.02 is empirical for 0–1 grayscale
    }

    public static uint DefaultQuality(this EncodeFormat encodeFormat)
    {
        return encodeFormat switch
        {
            EncodeFormat.PNG => 100, // (Maximum Deflate Compression) (In case of PNG, png is always lossless, Quality indicates the compression level)
            EncodeFormat.WEBP => 100,
            EncodeFormat.AVIF => 100,
            EncodeFormat.JPEG => 99, // (Best Compression speed, with almost no visual quality loss)
            _ => throw new ArgumentOutOfRangeException(nameof(encodeFormat), encodeFormat, null)
        };
    }

    /// <summary>
    /// Tries to determine if there is a better mode for resizing
    /// </summary>
    /// <param name="image"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <returns></returns>
    public static bool WillScaleWell(this IImage sourceImage, int targetWidth, int targetHeight, double tolerance = 0.1)
    {
        // Calculate the aspect ratios
        var sourceAspectRatio = (double)sourceImage.Width / sourceImage.Height;
        var targetAspectRatio = (double)targetWidth / targetHeight;

        // Compare aspect ratios
        if (Math.Abs(sourceAspectRatio - targetAspectRatio) > tolerance)
        {
            return false; // Aspect ratios differ significantly
        }

        // Calculate scaling factors
        var widthScaleFactor = (double)targetWidth / sourceImage.Width;
        var heightScaleFactor = (double)targetHeight / sourceImage.Height;

        // Check resolution quality (example thresholds)
        if (widthScaleFactor > 2.0 || heightScaleFactor > 2.0)
        {
            return false; // Scaling factor too large
        }

        return true; // Image will scale well
    }

    public static bool IsLikelyWideImage(this int width, int height)
    {
        var aspectRatio = (double)width / height;
        return aspectRatio > 1.25;
    }

}
