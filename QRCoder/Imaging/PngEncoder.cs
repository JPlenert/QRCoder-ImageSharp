using System;
using System.IO;

namespace QRCoder.Imaging
{
    /// <summary>
    /// Writes PNG files from prepared scanlines.
    /// </summary>
    /// <remarks>
    /// See https://www.w3.org/TR/2003/REC-PNG-20031110 and https://www.ietf.org/rfc/rfc1950.txt.
    /// </remarks>
    internal static class PngEncoder
    {
        internal static readonly byte[] Signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        // ReSharper disable InconsistentNaming
        private static readonly byte[] IHDR = { 73, 72, 68, 82 };
        private static readonly byte[] PLTE = { 80, 76, 84, 69 };
        private static readonly byte[] tRNS = { 116, 82, 78, 83 };
        private static readonly byte[] IDAT = { 73, 68, 65, 84 };
        private static readonly byte[] IEND = { 73, 69, 78, 68 };
        // ReSharper enable InconsistentNaming

        internal enum ColorType : byte
        {
            Greyscale = 0,
            Rgb = 2,
            Indexed = 3,
            GreyscaleAlpha = 4,
            Rgba = 6
        }

        internal enum FilterType : byte
        {
            None = 0,
            Sub = 1,
            Up = 2,
            Average = 3,
            Paeth = 4
        }

        /// <summary>
        /// Encodes a 1-bit greyscale PNG (bit 0 = black, bit 1 = white).
        /// </summary>
        /// <param name="scanlines">Filtered scanlines, each starting with its filter type byte.</param>
        public static byte[] EncodeGreyscale1Bit(int width, int height, byte[] scanlines)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(Signature, 0, Signature.Length);
                WriteHeader(stream, width, height, 1, ColorType.Greyscale);
                WriteChunk(stream, IDAT, Zlib.Compress(scanlines));
                WriteChunk(stream, IEND, new byte[0]);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Encodes a 1-bit indexed PNG. Bit 0 uses the first palette entry, bit 1 the second.
        /// </summary>
        /// <param name="scanlines">Filtered scanlines, each starting with its filter type byte.</param>
        public static byte[] EncodeIndexed1Bit(int width, int height, byte[] scanlines, QRColor color0, QRColor color1)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(Signature, 0, Signature.Length);
                WriteHeader(stream, width, height, 1, ColorType.Indexed);
                WriteChunk(stream, PLTE, new[] { color0.R, color0.G, color0.B, color1.R, color1.G, color1.B });
                if (color0.A < 255 || color1.A < 255)
                    WriteChunk(stream, tRNS, new[] { color0.A, color1.A });
                WriteChunk(stream, IDAT, Zlib.Compress(scanlines));
                WriteChunk(stream, IEND, new byte[0]);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Encodes an 8-bit RGBA PNG.
        /// </summary>
        public static byte[] EncodeRgba(RgbaImage image)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(Signature, 0, Signature.Length);
                WriteHeader(stream, image.Width, image.Height, 8, ColorType.Rgba);
                WriteChunk(stream, IDAT, Zlib.Compress(FilterRgbaScanlines(image)));
                WriteChunk(stream, IEND, new byte[0]);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Builds filtered RGBA scanlines, choosing None or Up per row by the minimum sum of absolute differences heuristic.
        /// </summary>
        private static byte[] FilterRgbaScanlines(RgbaImage image)
        {
            var stride = image.Width * 4;
            var pixels = image.Pixels;
            var scanlines = new byte[(stride + 1) * image.Height];

            for (var y = 0; y < image.Height; y++)
            {
                var src = y * stride;
                var dst = y * (stride + 1);

                var useUp = false;
                if (y > 0)
                {
                    long sumNone = 0, sumUp = 0;
                    for (var i = 0; i < stride; i++)
                    {
                        sumNone += Math.Abs((int)(sbyte)pixels[src + i]);
                        sumUp += Math.Abs((int)(sbyte)(pixels[src + i] - pixels[src + i - stride]));
                    }
                    useUp = sumUp < sumNone;
                }

                if (useUp)
                {
                    scanlines[dst] = (byte)FilterType.Up;
                    for (var i = 0; i < stride; i++)
                        scanlines[dst + 1 + i] = (byte)(pixels[src + i] - pixels[src + i - stride]);
                }
                else
                {
                    scanlines[dst] = (byte)FilterType.None;
                    Buffer.BlockCopy(pixels, src, scanlines, dst + 1, stride);
                }
            }

            return scanlines;
        }

        private static void WriteHeader(Stream stream, int width, int height, byte bitDepth, ColorType colorType)
        {
            var data = new byte[13];
            WriteUInt32BigEndian(data, 0, (uint)width);
            WriteUInt32BigEndian(data, 4, (uint)height);
            data[8] = bitDepth;
            data[9] = (byte)colorType;
            // data[10..12]: compression method, filter method, interlace method - all 0
            WriteChunk(stream, IHDR, data);
        }

        private static void WriteChunk(Stream stream, byte[] type, byte[] data)
        {
            var buffer = new byte[4];
            WriteUInt32BigEndian(buffer, 0, (uint)data.Length);
            stream.Write(buffer, 0, 4);
            stream.Write(type, 0, 4);
            stream.Write(data, 0, data.Length);
            WriteUInt32BigEndian(buffer, 0, Crc32.Compute(type, data));
            stream.Write(buffer, 0, 4);
        }

        private static void WriteUInt32BigEndian(byte[] buffer, int offset, uint value)
        {
            buffer[offset] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)value;
        }
    }
}
