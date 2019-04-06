using System.Runtime.InteropServices;

namespace Assassin.Native {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Color32 {

        public byte A;

        public byte B;

        public byte G;

        public byte R;

        internal static Color32 FromUInt32(uint color) {
            var r = (byte)((color >> 24) & 0xff);
            var g = (byte)((color >> 16) & 0xff);
            var b = (byte)((color >> 8) & 0xff);
            var a = (byte)(color & 0xff);

            return new Color32 {
                R = r,
                G = g,
                B = b,
                A = a
            };
        }

        public override string ToString() {
            return $"{{R={R.ToString()}, G={G.ToString()}, B={B.ToString()}, A={A.ToString()}}}";
        }

    }
}
