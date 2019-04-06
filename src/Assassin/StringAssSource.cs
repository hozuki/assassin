using System;
using System.Runtime.InteropServices;
using System.Text;
using Assassin.Native;
using JetBrains.Annotations;

namespace Assassin {
    public sealed class StringAssSource : IAssSource {

        public StringAssSource([NotNull] string str, [CanBeNull] string codePage = null) {
            Encoding encoding;

            if (codePage == null) {
                encoding = Utilities.Utf8;
            } else {
                encoding = Encoding.GetEncoding(codePage);
            }

            _codePage = codePage;
            _str = str;
            _encoding = encoding;
        }

        public AssTrack CreateTrack(AssLibrary library) {
            byte[] data;

            if (_data == null) {
                data = _encoding.GetBytes(_str);
                _data = data;
            } else {
                data = _data;
            }

            IntPtr trackPtr;

            unsafe {
                fixed (byte* bufferPtr = data) {
                    var buffer = new IntPtr(bufferPtr);

                    trackPtr = NativeMethods.ass_read_memory(library.NativePointer, buffer, data.Length, _codePage);
                }
            }

            if (trackPtr == IntPtr.Zero) {
                throw new AssException("Cannot create track from memory");
            }

            return new AssTrack(trackPtr);
        }

        [CanBeNull]
        private readonly string _codePage;

        [NotNull]
        private readonly string _str;

        [NotNull]
        private readonly Encoding _encoding;

        [CanBeNull]
        private byte[] _data;

    }
}
