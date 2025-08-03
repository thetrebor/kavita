
#if !ImageMagick
using NetVips;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace API.Services.ImageServices.NetVips;

public class NetVipsImageFactory : IImageFactory
{
    /// <inheritdoc/>
    public IImage Create(string filename)
    {
        return new NetVipsImage(filename);
    }

    /// <inheritdoc/>
    public IImage CreateFromBase64(string base64, int width)
    {
        return NetVipsImage.CreateFromBase64(base64, width);
    }

    /// <inheritdoc/>
    public IImage CreateFromBGRAByteArray(byte[] bgraByteArray, int width, int height)
    {
        return NetVipsImage.CreateFromBGRAByteArray(bgraByteArray, width, height);
    }

    /// <inheritdoc/>
    public IImage Create(Stream stream)
    {
        return new NetVipsImage(stream);
    }

    /// <inheritdoc/>
    public IImage CreateThumbnail(Stream stream, int width, int height)
    {
        return NetVipsImage.CreateThumbnail(stream, width, height);
    }

    /// <inheritdoc/>
    public IImage Create(int width, int height, byte red = 0, byte green = 0, byte blue = 0)
    {
        return new NetVipsImage(width, height, red, green, blue);
    }

    /// <inheritdoc/>
    public (int Width, int Height)? GetDimensions(string filename)
    {
        try
        {
            NetVipsImage image = new NetVipsImage(filename);
            return (image.Width, image.Height);
        }
        catch (Exception)
        {
        }
        return null;
    }

    /// <inheritdoc/>
    public List<Vector3> GetRgbPixelsPercentage(string filename, float percent)
    {
        using var im = Image.NewFromFile(filename);
        using var res = im.Resize(percent / 100f);
        float[] pixels = NetVipsImage.GetRGBAFloatImageDataFromImage(res);
        if (pixels == null)
            return new List<Vector3>();
        var rgbPixels = new List<Vector3>();
        for (uint x = 0; x < pixels.Length; x += 4)
            rgbPixels.Add(new Vector3(pixels[x], pixels[x + 1], pixels[x + 2]));
        return rgbPixels;
    }
}
#endif
