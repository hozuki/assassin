using System;
using System.Diagnostics;
using Assassin.Native;
using JetBrains.Annotations;

namespace Assassin {
    public class AssTrack : INativeObject {

        internal AssTrack(IntPtr nativePointer) {
            _nativePointer = nativePointer;
        }

        ~AssTrack() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
        }

        public bool IsDisposed { get; private set; }

        public IntPtr NativePointer => _nativePointer;

        private void Dispose(bool disposing) {
            if (IsDisposed) {
                return;
            }

            if (_nativePointer != IntPtr.Zero) {
                NativeMethods.ass_free_track(_nativePointer);
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
