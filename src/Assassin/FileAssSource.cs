using System;
using Assassin.Native;
using JetBrains.Annotations;

namespace Assassin {
    public sealed class FileAssSource : IAssSource {

        public FileAssSource([NotNull] string fileName, [CanBeNull] string codePage = null) {
            _fileName = fileName;
            _codePage = codePage;
        }

        public AssTrack CreateTrack(AssLibrary library) {
            var trackPtr = NativeMethods.ass_read_file(library.Handle, _fileName, _codePage);

            if (trackPtr == IntPtr.Zero) {
                throw new AssException("Cannot create track from file");
            }

            return new AssTrack(trackPtr);
        }

        [NotNull]
        private readonly string _fileName;

        [CanBeNull]
        private readonly string _codePage;

    }
}
