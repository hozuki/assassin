using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Assassin.Native;
using JetBrains.Annotations;

namespace Assassin {
    public class AssLibrary : INativeObject {

        private AssLibrary(IntPtr nativePointer) {
            _nativePointer = nativePointer;
        }

        ~AssLibrary() {
            Dispose(false);
        }

        [NotNull]
        public static AssLibrary Create() {
            var handle = NativeMethods.ass_library_init();

            if (handle == IntPtr.Zero) {
                throw new AssException("Cannot initialize library");
            }

            return new AssLibrary(handle);
        }

        [NotNull]
        public AssRenderer CreateRenderer() {
            this.EnsureNotDisposed();

            var rendererHandle = NativeMethods.ass_renderer_init(_nativePointer);

            if (rendererHandle == IntPtr.Zero) {
                throw new AssException("Cannot create renderer");
            }

            return new AssRenderer(rendererHandle);
        }

        [NotNull]
        public AssTrack CreateTrack([NotNull] IAssSource source) {
            this.EnsureNotDisposed();

            var track = source.CreateTrack(this);

            if (track == null) {
                throw new AssException("Failed to create track");
            }

            return track;
        }

        [Obsolete("This method may cause memory leak.")]
        [NotNull]
        public DefaultFontProvider[] GetAvailableFontProviders() {
            this.EnsureNotDisposed();

            DefaultFontProvider[] result;

            unsafe {
                NativeMethods.ass_get_available_font_providers(_nativePointer, out var providers, out var size);

                var s = size.ToUInt32();
                var typedProviders = (DefaultFontProvider*)providers;

                Trace.Assert(typedProviders != null);

                result = new DefaultFontProvider[s];

                for (var i = 0; i < s; i += 1) {
                    result[i] = typedProviders[i];
                }

                // AllocHGlobal/FreeHGlobal is just a wrapper for GlobalAlloc/GlobalFree (LocalAlloc/LocalFree),
                // which calls HeapAlloc/HeapFree in the end. malloc/free may not work like this; different CRTs
                // (e.g. Cygwin's and VC++'s) may be implemented in different ways. In short, this call does not
                // guarantee correct memory freeing.
                Marshal.FreeHGlobal(providers);
            }

            return result;
        }

        public void Dispose() {
            Dispose(true);
        }

        public bool IsDisposed { get; private set; }

        public IntPtr NativePointer {
            [DebuggerStepThrough]
            get => _nativePointer;
        }

        private void Dispose(bool disposing) {
            if (IsDisposed) {
                return;
            }

            if (_nativePointer != IntPtr.Zero) {
                NativeMethods.ass_library_done(_nativePointer);
            }

            _nativePointer = IntPtr.Zero;

            IsDisposed = true;

            if (disposing) {
                GC.SuppressFinalize(this);
            }
        }

        private IntPtr _nativePointer;

    }
}
