using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using API.Entities.Enums;

namespace API.Services.ImageServices;

/// <summary>
/// Defines operations for manipulating and saving images.
/// </summary>
public interface IImage : IDisposable
{
    /// <summary>
    /// Gets the width of the image in pixels.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height of the image in pixels.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Creates a deep copy of the current image instance.
    /// </summary>
    /// <returns>A new <see cref="IImage"/> instance that is a copy of the current image.</returns>
    IImage Clone();

    /// <summary>
    /// Resizes the image to the specified dimensions.
    /// </summary>
    /// <param name="width">The target width in pixels.</param>
    /// <param name="height">The target height in pixels.</param>
    void Resize(int width, int height);

    /// <summary>
    /// Crops the image to the specified rectangular region.
    /// </summary>
    /// <param name="x">The x-coordinate of the top-left corner of the crop region.</param>
    /// <param name="y">The y-coordinate of the top-left corner of the crop region.</param>
    /// <param name="width">The width of the crop region in pixels.</param>
    /// <param name="height">The height of the crop region in pixels.</param>
    void Crop(int x, int y, int width, int height);

    /// <summary>
    /// Creates a thumbnail of the image with the specified dimensions.
    /// </summary>
    /// <param name="width">The width of the thumbnail in pixels.</param>
    /// <param name="height">The height of the thumbnail in pixels.</param>
    /// <returns>A new <see cref="IImage"/> instance representing the thumbnail.</returns>
    IImage Thumbnail(int width, int height);

    /// <summary>
    /// Overlays another image onto the current image at the specified position.
    /// </summary>
    /// <param name="overlay">The image to overlay.</param>
    /// <param name="x">The x-coordinate of the top-left corner for the overlay.</param>
    /// <param name="y">The y-coordinate of the top-left corner for the overlay.</param>
    void Composite(IImage overlay, int x, int y);

    /// <summary>
    /// Saves the image to a file in the specified format.
    /// </summary>
    /// <param name="filename">The file path to save the image.</param>
    /// <param name="format">The format to use for encoding the image.</param>
    void Save(string filename, EncodeFormat format);

    /// <summary>
    /// Saves the image to a stream in the specified format.
    /// </summary>
    /// <param name="stream">The stream to write the image data to.</param>
    /// <param name="format">The format to use for encoding the image.</param>
    void Save(Stream stream, EncodeFormat format);

    /// <summary>
    /// Asynchronously saves the image to a file in the specified format.
    /// </summary>
    /// <param name="filename">The file path to save the image.</param>
    /// <param name="format">The format to use for encoding the image.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task SaveAsync(string filename, EncodeFormat format, CancellationToken token = default);

    /// <summary>
    /// Asynchronously saves the image to a stream in the specified format.
    /// </summary>
    /// <param name="stream">The stream to write the image data to.</param>
    /// <param name="format">The format to use for encoding the image.</param>
    /// <param name="token">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task SaveAsync(Stream stream, EncodeFormat format, CancellationToken token = default);

    /// <summary>
    /// Gets the image data as an array of RGBA float values.
    /// </summary>
    /// <returns>An array of floats representing the RGBA image data.</returns>
    float[] GetRGBAImageData();
}
