using System;
using System.Globalization;

namespace QRCoder
{
    /// <summary>
    /// A 32-bit RGBA color used by the QR code renderers.
    /// </summary>
    public readonly struct QRColor : IEquatable<QRColor>
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }

        public QRColor(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static QRColor Black => new QRColor(0, 0, 0);
        public static QRColor White => new QRColor(255, 255, 255);
        public static QRColor Transparent => new QRColor(0, 0, 0, 0);
        public static QRColor Red => new QRColor(255, 0, 0);
        public static QRColor Green => new QRColor(0, 128, 0);
        public static QRColor Blue => new QRColor(0, 0, 255);
        public static QRColor Gray => new QRColor(128, 128, 128);
        public static QRColor DarkGray => new QRColor(169, 169, 169);
        public static QRColor DarkGreen => new QRColor(0, 100, 0);

        /// <summary>
        /// Parses a hex color (#rgb, #rgba, #rrggbb or #rrggbbaa, leading # optional) or one of the named colors of this struct.
        /// </summary>
        public static QRColor Parse(string value)
        {
            if (TryParse(value, out var color))
                return color;
            throw new FormatException($"'{value}' is not a valid color. Expected #rgb, #rgba, #rrggbb, #rrggbbaa or a named color.");
        }

        public static bool TryParse(string value, out QRColor color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var s = value.Trim();
            switch (s.ToLowerInvariant())
            {
                case "black": color = Black; return true;
                case "white": color = White; return true;
                case "transparent": color = Transparent; return true;
                case "red": color = Red; return true;
                case "green": color = Green; return true;
                case "blue": color = Blue; return true;
                case "gray": color = Gray; return true;
                case "darkgray": color = DarkGray; return true;
                case "darkgreen": color = DarkGreen; return true;
            }

            if (s.StartsWith("#"))
                s = s.Substring(1);

            // Expand short forms (#rgb, #rgba) to their long equivalent
            if (s.Length == 3 || s.Length == 4)
            {
                var expanded = new char[s.Length * 2];
                for (var i = 0; i < s.Length; i++)
                    expanded[i * 2] = expanded[i * 2 + 1] = s[i];
                s = new string(expanded);
            }

            if (s.Length != 6 && s.Length != 8)
                return false;
            if (!uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var v))
                return false;

            if (s.Length == 6)
                color = new QRColor((byte)(v >> 16), (byte)(v >> 8), (byte)v);
            else
                color = new QRColor((byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v);
            return true;
        }

        /// <summary>
        /// Returns the color as hex string in RRGGBBAA format (without leading #).
        /// </summary>
        public string ToHex() => $"{R:X2}{G:X2}{B:X2}{A:X2}";

        /// <summary>
        /// Returns the color as byte array in R, G, B, A order.
        /// </summary>
        public byte[] ToRgbaBytes() => new[] { R, G, B, A };

        public bool Equals(QRColor other) => R == other.R && G == other.G && B == other.B && A == other.A;
        public override bool Equals(object obj) => obj is QRColor other && Equals(other);
        public override int GetHashCode() => (R << 24) | (G << 16) | (B << 8) | A;
        public static bool operator ==(QRColor left, QRColor right) => left.Equals(right);
        public static bool operator !=(QRColor left, QRColor right) => !left.Equals(right);
        public override string ToString() => "#" + ToHex();
    }
}
