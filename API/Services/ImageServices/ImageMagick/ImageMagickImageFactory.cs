#if ImageMagick
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ImageMagick;

namespace API.Services.ImageServices.ImageMagick;



/// <summary>
/// Represents an image factory that uses ImageMagick library.
/// </summary>
public class ImageMagickImageFactory : IImageFactory
{
    /// <inheritdoc/>
    public IImage Create(string filename)
    {
        return new ImageMagickImage(filename);
    }

    /// <inheritdoc/>
    public IImage CreateFromBase64(string base64, int width)
    {
        using var thumbnail = ImageMagickImage.CreateFromBase64(base64);
        int height = (int)(thumbnail.Height * ((double)width / thumbnail.Width));
        thumbnail.Resize(width, height);
        return thumbnail;
    }

    /// <inheritdoc/>
    public IImage CreateFromBGRAByteArray(byte[] bgraByteArray, int width, int height)
    {
        return ImageMagickImage.CreateFromBGRAByteArray(bgraByteArray, width, height);
    }

    /// <inheritdoc/>
    public IImage Create(Stream stream)
    {
        return new ImageMagickImage(stream);
    }

    /// <inheritdoc/>
    public IImage CreateThumbnail(Stream stream, int width, int height)
    {
        IImage image = new ImageMagickImage(stream);
        return image.Thumbnail(width, height);
    }

    /// <inheritdoc/>
    public IImage Create(int width, int height, byte red = 0, byte green = 0, byte blue = 0)
    {
        return new ImageMagickImage(width, height, red, green, blue);
    }

    /// <inheritdoc/>
    public (int Width, int Height)? GetDimensions(string filename)
    {
        try
        {
            MagickImageInfo info = new MagickImageInfo(filename);
            return ((int)info.Width, (int)info.Height);
        }
        catch (Exception e)
        {
        }
        return null;
    }

    /// <inheritdoc/>
    public List<Vector3> GetRgbPixelsPercentage(string filename, float percent)
    {
        var settings = new MagickReadSettings { ColorSpace = ColorSpace.RGB };
        using var im = new MagickImage(filename, settings);

        // Resize the image to speed up processing
        im.Resize(new Percentage(percent));
        // Convert image to RGB array
        float[] pixels = ImageMagickImage.GetRGBAFloatImageDataFromImage(im);
        if (pixels == null)
            return new List<Vector3>();
        var rgbPixels = new List<Vector3>();
        // Convert to list of Vector3 (RGB)

        for (uint x = 0; x < pixels.Length; x += 4)
            rgbPixels.Add(new Vector3(pixels[x], pixels[x + 1], pixels[x + 2]));
        return rgbPixels;
    }
}
#endif
