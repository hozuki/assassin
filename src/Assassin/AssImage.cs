using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Assassin.Native;
using JetBrains.Annotations;

namespace Assassin;

/// <summary>
/// An image rendered by libass. This struct must be consumed immediately using <see cref="Blend()"/> or <see cref="Blend(RgbaImage)"/>.
/// </summary>
public readonly ref struct AssImage
{

    internal AssImage(int rendererWidth, int rendererHeight, IntPtr nativePointer)
    {
        _rendererWidth = rendererWidth;
        _rendererHeight = rendererHeight;
        _nativePointer = nativePointer;
    }

    public RgbaImage Blend()
    {
        if (_nativePointer == IntPtr.Zero)
        {
            throw new ArgumentException("Underlying pointer is null.");
        }

        var imageBuffer = new RgbaImage(_rendererWidth, _rendererHeight);

        Blend(imageBuffer);

        return imageBuffer;
    }

    public void Blend(RgbaImage rgbaImage)
    {
        if (_nativePointer == IntPtr.Zero)
        {
            throw new ArgumentException("Underlying pointer is null.");
        }

        if (rgbaImage is null)
        {
            throw new ArgumentNullException(nameof(rgbaImage));
        }

        if (rgbaImage.Width < _rendererWidth || rgbaImage.Height < _rendererHeight)
        {
            throw new ArgumentException("Cannot blend into a buffer smaller than renderer size", nameof(rgbaImage));
        }

        unsafe
        {
            var image = (ASS_Image*)_nativePointer;

            Blend(rgbaImage, image);
        }
    }

    private static unsafe void Blend(RgbaImage rgbaImage, [CanBeNull] ASS_Image* image)
    {
        var blendedCount = 0;

        while (image is not null)
        {
            BlendSingle(rgbaImage, image);

            blendedCount += 1;

            var next = (ASS_Image*)image->Next;

            image = next;
        }

#if DEBUG
        Debug.Print("Blended images: {0}", blendedCount.ToString());
#endif
    }

    private static unsafe void BlendSingle(RgbaImage rgbaImage, [NotNull] ASS_Image* image)
    {
        var w = image->Width;
        var h = image->Height;
        var s = image->Stride;

        if (w == 0 || h == 0)
        {
            // "w/h can be zero, in this case the bitmap should not be rendered at all."
            return;
        }

        if (rgbaImage.Width < w || rgbaImage.Height < h)
        {
            throw new AssException("Cannot render an ASS_Image to a buffer smaller than it");
        }

        var color = Color32.FromUInt32(image->Color);

        color.A = (byte)(byte.MaxValue - color.A);

        var opacity = color.A / (float)byte.MaxValue;

        var src = (byte*)image->Bitmap;

        fixed (Color32* dst = rgbaImage.Buffer)
        {
            for (var y = 0; y < h; y += 1)
            {
                // "The last bitmap row is not guaranteed to be padded up to stride size, e.g. in the worst case a bitmap has the size stride * (h - 1) + w."
                var width = y < h - 1 ? w : s;

                for (var x = 0; x < width; x += 1)
                {
                    var srcIndex = y * image->Stride + x;
                    var dstIndex = (y + image->DestY) * rgbaImage.Width + (x + image->DestX);

                    var k = (src[srcIndex] * opacity) / byte.MaxValue;

                    dst[dstIndex] = AlphaBlend(k, in color, in dst[dstIndex]);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Color32 AlphaBlend(float pixel, in Color32 src, in Color32 dst)
    {
        var mk = 1 - pixel;

        var r = Utilities.Clamp((int)(pixel * src.R + dst.R * mk), byte.MinValue, byte.MaxValue);
        var g = Utilities.Clamp((int)(pixel * src.G + dst.G * mk), byte.MinValue, byte.MaxValue);
        var b = Utilities.Clamp((int)(pixel * src.B + dst.B * mk), byte.MinValue, byte.MaxValue);
        var a = Utilities.Clamp((int)(pixel * src.A + dst.A * mk), byte.MinValue, byte.MaxValue);

        return new Color32
        {
            R = (byte)r,
            G = (byte)g,
            B = (byte)b,
            A = (byte)a
        };
    }

    private readonly int _rendererWidth;
    private readonly int _rendererHeight;
    private readonly IntPtr _nativePointer;

}
