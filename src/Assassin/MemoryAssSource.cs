using System;
using System.IO;
using System.Runtime.InteropServices;
using Assassin.Native;
using JetBrains.Annotations;

namespace Assassin {
    public sealed class MemoryAssSource : IAssSource {

        public MemoryAssSource([NotNull] byte[] data, [CanBeNull] string codePage = null) {
            _data = (byte[])data.Clone();
            _codePage = codePage;
        }

        public MemoryAssSource([NotNull] MemoryStream stream, [CanBeNull] string codePage = null) {
            _data = stream.ToArray();
            _codePage = codePage;
        }

        public MemoryAssSource([NotNull] Stream stream, [CanBeNull] string codePage = null) {
            using (var memoryStream = new MemoryStream()) {
                stream.CopyTo(memoryStream);

                _data = memoryStream.ToArray();
            }

            _codePage = codePage;
        }

        public AssTrack CreateTrack(AssLibrary library) {
            var buffer = Marshal.AllocHGlobal(_data.Length);

            Marshal.Copy(_data, 0, buffer, _data.Length);

            var trackPtr = NativeMethods.ass_read_memory(library.Handle, buffer, _data.Length, _codePage);

            Marshal.FreeHGlobal(buffer);

            if (trackPtr == IntPtr.Zero) {
                throw new AssException("Cannot create track from memory");
            }

            return new AssTrack(trackPtr);
        }

        [NotNull]
        private readonly byte[] _data;

        [CanBeNull]
        private readonly string _codePage;

    }
}
