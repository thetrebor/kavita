#if !ImageMagick
using API.Entities.Enums;
using NetVips;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using API.Extensions;

namespace API.Services.ImageServices.NetVips
{

    /// <summary>
    /// NetVips implementation of <see cref="IImage"/> for image manipulation.
    /// </summary>
    public class NetVipsImage : IImage
    {
        Image _image;

        /// <inheritdoc/>
        public int Width => _image?.Width ?? 0;

        /// <inheritdoc/>
        public int Height => _image?.Height ?? 0;

        /// <summary>
        /// Creates an image from a base64 string and resizes it to the specified width.
        /// </summary>
        /// <param name="base64">The base64 string representing the image.</param>
        /// <param name="width">The width to resize the image to.</param>
        /// <returns>An <see cref="IImage"/> instance.</returns>
        public static IImage CreateFromBase64(string base64, int width)
        {
            if (string.IsNullOrEmpty(base64))
                throw new ArgumentNullException(nameof(base64));
            NetVipsImage m = new NetVipsImage();
            m._image = Image.ThumbnailBuffer(Convert.FromBase64String(base64), width);
            return m;
        }

        /// <summary>
        /// Creates an image from a BGRA byte array.
        /// </summary>
        /// <param name="bgraByteArray">The BGRA byte array.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <returns>An <see cref="IImage"/> instance.</returns>
        public static IImage CreateFromBGRAByteArray(byte[] bgraByteArray, int width, int height)
        {
            const int bands = 4; // BGRA

            if (bgraByteArray.Length != width * height * bands)
                throw new ArgumentException("Input byte array size doesn't match BGRA dimensions.");

            // Load raw BGRA image
            var image = Image.NewFromMemory(bgraByteArray, width, height, bands, Enums.BandFormat.Uchar);

            // Extract and reorder BGRA → RGBA
            var b = image.ExtractBand(0);
            var g = image.ExtractBand(1);
            var r = image.ExtractBand(2);
            var a = image.ExtractBand(3);
            NetVipsImage m = new NetVipsImage();
            m._image = r.Bandjoin(g).Bandjoin(b).Bandjoin(a);
            return m;
        }

        /// <summary>
        /// Loads an image from a file.
        /// </summary>
        /// <param name="filename">The file path.</param>
        public NetVipsImage(string filename)
        {
            _image = Image.NewFromFile(filename, false, access: Enums.Access.SequentialUnbuffered);
        }

        /// <summary>
        /// Creates a thumbnail from a stream with the specified dimensions.
        /// </summary>
        /// <param name="stream">The image stream.</param>
        /// <param name="width">The width of the thumbnail.</param>
        /// <param name="height">The height of the thumbnail.</param>
        /// <returns>An <see cref="IImage"/> thumbnail.</returns>
        public static IImage CreateThumbnail(Stream stream, int width, int height)
        {
            using NetVipsImage image = new NetVipsImage(stream);
            var scalingSize = NetVipsImage.GetSizeForDimensions(image, width, height);
            var scalingCrop = NetVipsImage.GetCropForDimensions(image, width, height);
            using var thumbnail2 = Image.ThumbnailStream(stream, width, height: height,
                size: scalingSize,
                crop: scalingCrop);
            NetVipsImage g = new NetVipsImage();
            g._image = thumbnail2;
            return g;
        }

        /// <summary>
        /// Private constructor for internal use.
        /// </summary>
        private NetVipsImage()
        {

        }

        /// <summary>
        /// Loads an image from a stream.
        /// </summary>
        /// <param name="stream">The image stream.</param>
        public NetVipsImage(Stream stream)
        {
            _image = Image.NewFromStream(stream, access: Enums.Access.SequentialUnbuffered);
        }

        /// <summary>
        /// Creates a copy of the provided NetVips image.
        /// </summary>
        /// <param name="image">The NetVips image to copy.</param>
        public NetVipsImage(Image image)
        {
            _image = image.Copy();
        }

        /// <summary>
        /// Creates a blank image with the specified dimensions and background color.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="red">Red channel value.</param>
        /// <param name="green">Green channel value.</param>
        /// <param name="blue">Blue channel value.</param>
        public NetVipsImage(int width, int height, byte red = 0, byte green = 0, byte blue = 0)
        {
            double[] background = { red, green, blue };

            _image = Image.Black(width, height).NewFromImage(background);
        }

        /// <inheritdoc/>
        public IImage Clone()
        {
            return new NetVipsImage(_image);
        }

        /// <inheritdoc/>
        public void Resize(int width, int height)
        {
            // Scale separately in X and Y
            double scaleX = (double)width / _image.Width;
            double scaleY = (double)height / _image.Height;
            _image = _image.Resize(scaleX, kernel: Enums.Kernel.Lanczos3, vscale: scaleY);
        }

        /// <inheritdoc/>
        public void Crop(int x, int y, int width, int height)
        {
            _image = _image.Crop(x, y, width, height);
        }

        /// <inheritdoc/>
        public IImage Thumbnail(int width, int height)
        {
            NetVipsImage im = new NetVipsImage();
            im._image = _image.ThumbnailImage(width, height: height,
                size: GetSizeForDimensions(this, width, height),
                crop: GetCropForDimensions(this, width, height));
            return im;
        }

        /// <inheritdoc/>
        public void Composite(IImage overlay, int x, int y)
        {
            NetVipsImage im = overlay as NetVipsImage;
            if (im == null)
                throw new ArgumentNullException(nameof(overlay));
            _image = _image.Insert(im._image, x, y);
        }

        /// <summary>
        /// Gets the save options for the specified format.
        /// </summary>
        /// <param name="format">The encoding format.</param>
        /// <returns>A <see cref="VOption"/> with the appropriate options.</returns>
        private static VOption GetSaveOptions(EncodeFormat format)
        {
            var quality = format.DefaultQuality();
            var options = new VOption();

            switch (format)
            {
                case EncodeFormat.JPEG:
                case EncodeFormat.WEBP:
                case EncodeFormat.AVIF:
                    options["Q"] = (int)quality; // Quality for lossy formats
                    break;
                case EncodeFormat.PNG:
                    options["compression"] = 9 - (int)(quality / 11); // 0–9 deflate level
                    break;
            }

            return options;
        }

        /// <inheritdoc/>
        public void Save(string filename, EncodeFormat format)
        {
            _image.WriteToFile(filename, GetSaveOptions(format));
        }

        /// <inheritdoc/>
        public void Save(Stream stream, EncodeFormat format)
        {
            var buffer = _image.WriteToBuffer(format.GetExtension(), GetSaveOptions(format));
            stream.Write(buffer, 0, buffer.Length);
        }

        /// <inheritdoc/>
        public Task SaveAsync(string filename, EncodeFormat format, CancellationToken token = default)
        {
            // NetVips is synchronous — wrap in Task.Run
            return Task.Run(() => Save(filename, format), token);
        }

        /// <inheritdoc/>
        public Task SaveAsync(Stream stream, EncodeFormat format, CancellationToken token = default)
        {
            return Task.Run(() => Save(stream, format), token);
        }

        /// <summary>
        /// Gets RGBA float image data from a NetVips image.
        /// </summary>
        /// <param name="image">The NetVips image.</param>
        /// <returns>Float array of RGBA pixel data.</returns>
        public static float[] GetRGBAFloatImageDataFromImage(Image image)
        {
            if (image.Interpretation != Enums.Interpretation.Srgb)
            {
                image = image.Colourspace(Enums.Interpretation.Srgb);
            }

            // Ensure band count is 4 (RGBA)
            if (image.Bands == 1)
            {
                // Grayscale → R=G=B, A=255
                image = image.Bandjoin(image).Bandjoin(image).Bandjoin(255);
            }
            else if (image.Bands == 2)
            {
                // Grayscale + Alpha → RGB = channel 0, A = channel 1
                var gray = image.ExtractBand(0);
                var alpha = image.ExtractBand(1);
                image = gray.Bandjoin(gray).Bandjoin(gray).Bandjoin(alpha);
            }
            else if (image.Bands == 3)
            {
                // RGB → RGBA with opaque alpha
                image = image.Bandjoin(255);
            }
            else if (image.Bands == 4)
            {
                // Assume RGBA already
            }
            else if (image.Bands > 4)
            {
                // Extract first 4 bands
                image = image.ExtractBand(0, 4);
            }

            // Ensure Uchar (8-bit per channel), then convert to Float
            if (image.Format != Enums.BandFormat.Uchar)
            {
                image = image.Cast(Enums.BandFormat.Uchar);
            }

            image = image.Cast(Enums.BandFormat.Float);

            // Write pixel data to memory (float = 4 bytes per channel)
            byte[] raw = image.WriteToMemory();

            // Convert to float[]
            float[] pixels = new float[raw.Length / 4];
            Buffer.BlockCopy(raw, 0, pixels, 0, raw.Length);

            return pixels;
        }

        /// <inheritdoc/>
        public float[] GetRGBAImageData()
        {
            return GetRGBAFloatImageDataFromImage(_image);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _image?.Dispose();
        }

        /// <summary>
        /// Gets the recommended scaling size for the specified dimensions.
        /// </summary>
        /// <param name="image">The image to scale.</param>
        /// <param name="targetWidth">Target width.</param>
        /// <param name="targetHeight">Target height.</param>
        /// <returns>The recommended <see cref="Enums.Size"/>.</returns>
        public static Enums.Size GetSizeForDimensions(IImage image, int targetWidth, int targetHeight)
        {
            try
            {
                if (image.WillScaleWell(targetWidth, targetHeight) || image.Width.IsLikelyWideImage(image.Height))
                {
                    return Enums.Size.Force;
                }
            }
            catch (Exception)
            {
                /* Swallow */
            }

            return Enums.Size.Both;
        }

        /// <summary>
        /// Gets the recommended crop for the specified dimensions.
        /// </summary>
        /// <param name="image">The image to crop.</param>
        /// <param name="targetWidth">Target width.</param>
        /// <param name="targetHeight">Target height.</param>
        /// <returns>The recommended <see cref="Enums.Interesting"/> value, or null.</returns>
        public static Enums.Interesting? GetCropForDimensions(IImage image, int targetWidth, int targetHeight)
        {
            try
            {
                if (image.WillScaleWell(targetWidth, targetHeight) || image.Width.IsLikelyWideImage(image.Height))
                {
                    return null;
                }
            }
            catch (Exception)
            {
                /* Swallow */
                return null;
            }

            return Enums.Interesting.Attention;
        }
    }
}
#endif
