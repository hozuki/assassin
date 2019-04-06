using System;
using System.Diagnostics;
using Assassin.Native;
using JetBrains.Annotations;

namespace Assassin {
    public class AssTrack : IDisposableEx {

        internal AssTrack(IntPtr trackPtr) {
            _trackPtr = trackPtr;
        }

        ~AssTrack() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
        }

        public bool IsDisposed { get; private set; }

        internal IntPtr Handle => _trackPtr;

        private void Dispose(bool disposing) {
            if (IsDisposed) {
                return;
            }

            if (_trackPtr != IntPtr.Zero) {
                NativeMethods.ass_free_track(_trackPtr);
            }

            _trackPtr = IntPtr.Zero;

            IsDisposed = true;

            if (disposing) {
                GC.SuppressFinalize(this);
            }
        }

        [NotNull]
        private unsafe Assassin.Native.AssTrack* GetTypedPointer() {
            this.EnsureNotDisposed();

            Trace.Assert(_trackPtr != IntPtr.Zero);

            // ReSharper disable once AssignNullToNotNullAttribute
            return (Assassin.Native.AssTrack*)_trackPtr;
        }

        private IntPtr _trackPtr;

    }
}
