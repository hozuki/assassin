using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using Assassin.Native;
using JetBrains.Annotations;

namespace Assassin {
    public sealed class AssImage {

        internal AssImage([NotNull] AssRenderer renderer, IntPtr imagePtr) {
            _imagePtr = imagePtr;
            _renderer = renderer;
        }

        [NotNull]
        public RgbaImage Blend() {
            var imageBuffer = new RgbaImage(_renderer.Width, _renderer.Height);

            Blend(imageBuffer);

            return imageBuffer;
        }

        public void Blend([NotNull] RgbaImage rgbaImage) {
            if (rgbaImage == null) {
                throw new ArgumentNullException(nameof(rgbaImage));
            }

            if (rgbaImage.Width < _renderer.Width || rgbaImage.Height < _renderer.Height) {
                throw new ArgumentException("Cannot blend into a buffer smaller than renderer size", nameof(rgbaImage));
            }

            unsafe {
                var image = GetTypedPointer();

                Blend(rgbaImage, image);
            }
        }

        private static unsafe void Blend([NotNull] RgbaImage rgbaImage, [CanBeNull] Assassin.Native.AssImage* image) {
            var blendedCount = 0;

            while (image != null) {
                BlendSingle(rgbaImage, image);

                blendedCount += 1;

                var next = (Assassin.Native.AssImage*)image->Next;

                image = next;
            }

#if DEBUG
            Debug.Print("Blended images: {0}", blendedCount.ToString());
#endif
        }

        private static unsafe void BlendSingle([NotNull] RgbaImage rgbaImage, [NotNull] Assassin.Native.AssImage* image) {
            var w = image->Width;
            var h = image->Height;

            if (w == 0 || h == 0) {
                return;
            }

            if (rgbaImage.Width < w || rgbaImage.Height < h) {
                throw new AssException("Cannot render an ASS_Image to a buffer smaller than it");
            }

            var color = Color32.FromUInt32(image->Color);

            color.A = (byte)(byte.MaxValue - color.A);

            var opacity = color.A / (float)byte.MaxValue;

            var src = (byte*)image->Bitmap;

            Trace.Assert(src != null);

            fixed (Color32* buffer = rgbaImage.Buffer) {
                var dst = buffer;

                for (var y = 0; y < h; y += 1) {
                    for (var x = 0; x < w; x += 1) {
                        var srcIndex = y * image->Stride + x;
                        var dstIndex = (y + image->DestY) * rgbaImage.Width + (x + image->DestX);

                        var k = (src[srcIndex] * opacity) / byte.MaxValue;

                        dst[dstIndex] = AlphaBlend(k, in color, in dst[dstIndex]);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Color32 AlphaBlend(float pixel, in Color32 src, in Color32 dst) {
            var mk = 1 - pixel;

            var r = Utilities.Clamp((int)(pixel * src.R + dst.R * mk), byte.MinValue, byte.MaxValue);
            var g = Utilities.Clamp((int)(pixel * src.G + dst.G * mk), byte.MinValue, byte.MaxValue);
            var b = Utilities.Clamp((int)(pixel * src.B + dst.B * mk), byte.MinValue, byte.MaxValue);
            var a = Utilities.Clamp((int)(pixel * src.A + dst.A * mk), byte.MinValue, byte.MaxValue);

            return new Color32 {
                R = (byte)r,
                G = (byte)g,
                B = (byte)b,
                A = (byte)a
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CanBeNull]
        private unsafe Assassin.Native.AssImage* GetTypedPointer() {
            // ReSharper disable once AssignNullToNotNullAttribute
            return (Assassin.Native.AssImage*)_imagePtr;
        }

        private readonly IntPtr _imagePtr;

        [NotNull]
        private readonly AssRenderer _renderer;

    }
}
