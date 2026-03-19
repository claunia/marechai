#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using SkiaSharp;
using Svg.Skia;
using Windows.Storage.Streams;

namespace Marechai.App.Services;

/// <summary>
///     Factory for creating ImageSource instances on the UI thread.
///     Uses Svg.Skia to rasterize SVGs into bitmaps, bypassing Uno's SvgImageSource
///     which doesn't initialize properly under PrismApplication.
/// </summary>
public sealed class ImageSourceFactory
{
    private readonly DispatcherQueue _dispatcherQueue;

    public ImageSourceFactory(DispatcherQueue dispatcherQueue) => _dispatcherQueue = dispatcherQueue;

    /// <summary>
    ///     Rasterizes an SVG stream to a BitmapImage using Svg.Skia.
    /// </summary>
    public Task<BitmapImage?> CreateSvgImageSourceAsync(Stream svgStream)
    {
        // Rasterize SVG to PNG bytes on the current (possibly background) thread
        byte[] pngBytes;

        try
        {
            using var svg = new SKSvg();
            svg.Load(svgStream);

            if(svg.Picture is null)
                return Task.FromResult<BitmapImage?>(null);

            SKRect bounds = svg.Picture.CullRect;

            if(bounds.Width <= 0 || bounds.Height <= 0)
                return Task.FromResult<BitmapImage?>(null);

            using var bitmap = new SKBitmap((int)bounds.Width, (int)bounds.Height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(svg.Picture);

            using SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            pngBytes = data.ToArray();
        }
        catch
        {
            // Some SVGs contain <image> elements with relative URIs that can't be
            // resolved when loading from a stream. Fall back gracefully.
            return Task.FromResult<BitmapImage?>(null);
        }

        // Create BitmapImage on the UI thread
        return CreateBitmapFromBytesAsync(pngBytes);
    }

    /// <summary>
    ///     Creates a BitmapImage from a raster image stream, ensuring UI thread affinity.
    /// </summary>
    public Task<BitmapImage?> CreateBitmapImageSourceAsync(Stream stream)
    {
        var tcs = new TaskCompletionSource<BitmapImage?>();

        if(_dispatcherQueue.HasThreadAccess)
            _ = CreateBitmapCoreAsync(stream, tcs);
        else
            _dispatcherQueue.TryEnqueue(() => _ = CreateBitmapCoreAsync(stream, tcs));

        return tcs.Task;
    }

    private Task<BitmapImage?> CreateBitmapFromBytesAsync(byte[] imageBytes)
    {
        var tcs = new TaskCompletionSource<BitmapImage?>();

        if(_dispatcherQueue.HasThreadAccess)
            _ = CreateBitmapFromBytesCoreAsync(imageBytes, tcs);
        else
            _dispatcherQueue.TryEnqueue(() => _ = CreateBitmapFromBytesCoreAsync(imageBytes, tcs));

        return tcs.Task;
    }

    private static async Task CreateBitmapFromBytesCoreAsync(byte[]                          imageBytes,
                                                              TaskCompletionSource<BitmapImage?> tcs)
    {
        try
        {
            var bitmap = new BitmapImage();
            using var ms = new MemoryStream(imageBytes);

            using(IRandomAccessStream randomStream = ms.AsRandomAccessStream())
            {
                await bitmap.SetSourceAsync(randomStream);
            }

            tcs.TrySetResult(bitmap);
        }
        catch(Exception ex)
        {
            tcs.TrySetException(ex);
        }
    }

    private static async Task CreateBitmapCoreAsync(Stream stream, TaskCompletionSource<BitmapImage?> tcs)
    {
        try
        {
            var bitmap = new BitmapImage();

            using(IRandomAccessStream randomStream = stream.AsRandomAccessStream())
            {
                await bitmap.SetSourceAsync(randomStream);
            }

            tcs.TrySetResult(bitmap);
        }
        catch(Exception ex)
        {
            tcs.TrySetException(ex);
        }
    }
}
