using System;
using System.Diagnostics;
using Assassin.Native;
using JetBrains.Annotations;

namespace Assassin {
    public class AssRenderer : INativeObject {

        internal AssRenderer(IntPtr nativePointer) {
            _nativePointer = nativePointer;
        }

        ~AssRenderer() {
            Dispose(false);
        }

        public void SetFrameSize(int width, int height) {
            this.EnsureNotDisposed();

            NativeMethods.ass_set_frame_size(_nativePointer, width, height);

            _width = width;
            _height = height;
        }

        public void SetFonts([NotNull] string familyName) {
            this.EnsureNotDisposed();

            NativeMethods.ass_set_fonts(_nativePointer, null, familyName, DefaultFontProvider.AutoDetect, null, true);
        }

        [NotNull]
        public AssImage RenderFrame([NotNull] AssTrack track, long timestamp) {
            return RenderFrame(track, timestamp, out _);
        }

        [NotNull]
        public AssImage RenderFrame([NotNull] AssTrack track, long timestamp, out FrameChange frameChange) {
            this.EnsureNotDisposed();

            if (_width <= 0 || _height <= 0) {
                throw new AssException("Cannot render frame: frame dimensions are not initialized");
            }

            var framePointer = NativeMethods.ass_render_frame(_nativePointer, track.NativePointer, timestamp, out frameChange);

            // framePointer == nullptr => the image is blank

            return new AssImage(this, framePointer);
        }

        public IntPtr NativePointer {
            [DebuggerStepThrough]
            get => _nativePointer;
        }

        public void Dispose() {
            Dispose(true);
        }

        public bool IsDisposed { get; private set; }

        public int Width {
            [DebuggerStepThrough]
            get => _width;
        }

        public int Height {
            [DebuggerStepThrough]
            get => _height;
        }

        private void Dispose(bool disposing) {
            if (IsDisposed) {
                return;
            }

            if (_nativePointer != IntPtr.Zero) {
                NativeMethods.ass_renderer_done(_nativePointer);
            }

            _nativePointer = IntPtr.Zero;

            IsDisposed = true;

            if (disposing) {
                GC.SuppressFinalize(this);
            }
        }

        private int _width;
        private int _height;

        private IntPtr _nativePointer;

    }
}
