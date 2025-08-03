using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace API.Services.ImageServices;

/// <summary>
/// Defines a factory for creating and manipulating images from various sources and formats.
/// </summary>
public interface IImageFactory
{
    /// <summary>
    /// Creates an image from the specified file path.
    /// </summary>
    /// <param name="filename">The path to the image file.</param>
    /// <returns>An <see cref="IImage"/> instance representing the loaded image.</returns>
    IImage Create(string filename);

    /// <summary>
    /// Creates an image from the provided stream.
    /// </summary>
    /// <param name="stream">A stream containing image data.</param>
    /// <returns>An <see cref="IImage"/> instance representing the loaded image.</returns>
    IImage Create(Stream stream);

    /// <summary>
    /// Creates a thumbnail image from the provided stream with the specified dimensions.
    /// </summary>
    /// <param name="stream">A stream containing image data.</param>
    /// <param name="width">The width of the thumbnail.</param>
    /// <param name="height">The height of the thumbnail.</param>
    /// <returns>An <see cref="IImage"/> instance representing the thumbnail.</returns>
    IImage CreateThumbnail(Stream stream, int width, int height);

    /// <summary>
    /// Creates a blank image with the specified dimensions and color.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="red">The red component of the color (default is 0).</param>
    /// <param name="green">The green component of the color (default is 0).</param>
    /// <param name="blue">The blue component of the color (default is 0).</param>
    /// <returns>An <see cref="IImage"/> instance representing the blank image.</returns>
    IImage Create(int width, int height, byte red = 0, byte green = 0, byte blue = 0);

    /// <summary>
    /// Creates an image from a base64-encoded string.
    /// </summary>
    /// <param name="base64">A base64 string containing image data.</param>
    /// <param name="width">The width of the resulting image, aspect ratio will be maintained.</param>
    /// <returns>An <see cref="IImage"/> instance representing the decoded image.</returns>
    IImage CreateFromBase64(string base64, int width);

    /// <summary>
    /// Creates an image from a BGRA byte array, typically output from a PDF page.
    /// </summary>
    /// <param name="bgraByteArray">A byte array containing BGRA image data.</param>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <returns>An <see cref="IImage"/> instance representing the image.</returns>
    IImage CreateFromBGRAByteArray(byte[] bgraByteArray, int width, int height);

    /// <summary>
    /// Gets the dimensions of the specified image file.
    /// </summary>
    /// <param name="filename">The path to the image file.</param>
    /// <returns>
    /// A tuple containing the width and height of the image, or <c>null</c> if the dimensions cannot be determined.
    /// </returns>
    (int Width, int Height)? GetDimensions(string filename);

    /// <summary>
    /// Gets the RGB pixel values of the specified image file, resized to a given percentage.
    /// </summary>
    /// <param name="filename">The path to the image file.</param>
    /// <param name="percent">The percentage to resize the image before extracting pixels.</param>
    /// <returns>A list of <see cref="Vector3"/> representing the RGB pixels.</returns>
    List<Vector3> GetRgbPixelsPercentage(string filename, float percent);
}

