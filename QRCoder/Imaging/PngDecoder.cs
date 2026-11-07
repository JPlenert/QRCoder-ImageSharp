using System;
using System.IO;

namespace QRCoder.Imaging
{
    /// <summary>
    /// Decodes PNG files into RGBA images. Supports all standard color types and bit depths, tRNS transparency and
    /// Adam7 interlacing. 16-bit samples are reduced to 8 bits, ancillary chunks (gamma, color profiles, ...) are ignored.
    /// </summary>
    internal static class PngDecoder
    {
        // Guards against absurd allocations from malformed or hostile headers.
        private const long MaxPixels = 64L * 1024 * 1024;

        private static readonly int[] Adam7XStart = { 0, 4, 0, 2, 0, 1, 0 };
        private static readonly int[] Adam7YStart = { 0, 0, 4, 0, 2, 0, 1 };
        private static readonly int[] Adam7XStep = { 8, 8, 4, 4, 2, 2, 1 };
        private static readonly int[] Adam7YStep = { 8, 8, 8, 4, 4, 2, 2 };

        private sealed class Header
        {
            public int Width;
            public int Height;
            public int BitDepth;
            public PngEncoder.ColorType ColorType;
            public bool Interlaced;
            public int Channels;
            public int BitsPerPixel => Channels * BitDepth;
            public int FilterBytesPerPixel => Math.Max(1, BitsPerPixel / 8);
            public int RowBytes(int width) => (int)(((long)width * BitsPerPixel + 7) / 8);
        }

        public static RgbaImage Decode(byte[] png)
        {
            if (png == null)
                throw new ArgumentNullException(nameof(png));
            if (png.Length < PngEncoder.Signature.Length)
                throw new InvalidDataException("Data is too short to be a PNG file.");
            for (var i = 0; i < PngEncoder.Signature.Length; i++)
                if (png[i] != PngEncoder.Signature[i])
                    throw new InvalidDataException("Data is not a PNG file (invalid signature).");

            Header header = null;
            byte[] palette = null;
            byte[] transparency = null;
            var idat = new MemoryStream();

            var offset = PngEncoder.Signature.Length;
            var seenEnd = false;
            while (!seenEnd)
            {
                if (offset + 12 > png.Length)
                    throw new InvalidDataException("PNG file is truncated.");
                var length = ReadUInt32BigEndian(png, offset);
                if (length > int.MaxValue || offset + 12 + length > png.Length)
                    throw new InvalidDataException("PNG chunk length exceeds file size.");

                var dataOffset = offset + 8;
                var type = System.Text.Encoding.ASCII.GetString(png, offset + 4, 4);
                var expectedCrc = ReadUInt32BigEndian(png, dataOffset + (int)length);
                var actualCrc = Crc32.Update(0xFFFFFFFF, png, offset + 4, (int)length + 4) ^ 0xFFFFFFFF;
                if (expectedCrc != actualCrc)
                    throw new InvalidDataException($"CRC mismatch in PNG chunk '{type}'.");

                switch (type)
                {
                    case "IHDR":
                        header = ReadHeader(png, dataOffset, (int)length);
                        break;
                    case "PLTE":
                        palette = Slice(png, dataOffset, (int)length);
                        break;
                    case "tRNS":
                        transparency = Slice(png, dataOffset, (int)length);
                        break;
                    case "IDAT":
                        if (header == null)
                            throw new InvalidDataException("PNG IDAT chunk before IHDR.");
                        idat.Write(png, dataOffset, (int)length);
                        break;
                    case "IEND":
                        seenEnd = true;
                        break;
                    default:
                        // Unknown critical chunks (uppercase first letter) cannot be safely ignored.
                        if (char.IsUpper(type[0]))
                            throw new InvalidDataException($"Unsupported critical PNG chunk '{type}'.");
                        break;
                }

                offset = dataOffset + (int)length + 4;
            }

            if (header == null)
                throw new InvalidDataException("PNG file has no IHDR chunk.");
            if (header.ColorType == PngEncoder.ColorType.Indexed && palette == null)
                throw new InvalidDataException("Indexed PNG file has no PLTE chunk.");

            var raw = Zlib.Decompress(idat.ToArray(), (int)GetRawDataLength(header));
            var image = new RgbaImage(header.Width, header.Height);

            if (!header.Interlaced)
            {
                Unfilter(raw, 0, header.RowBytes(header.Width), header.Height, header.FilterBytesPerPixel);
                WritePixels(header, raw, 0, header.Width, header.Height, 0, 0, 1, 1, palette, transparency, image);
            }
            else
            {
                var passOffset = 0;
                for (var pass = 0; pass < 7; pass++)
                {
                    GetPassSize(header, pass, out var passWidth, out var passHeight);
                    if (passWidth == 0 || passHeight == 0)
                        continue;
                    var rowBytes = header.RowBytes(passWidth);
                    Unfilter(raw, passOffset, rowBytes, passHeight, header.FilterBytesPerPixel);
                    WritePixels(header, raw, passOffset, passWidth, passHeight,
                        Adam7XStart[pass], Adam7YStart[pass], Adam7XStep[pass], Adam7YStep[pass], palette, transparency, image);
                    passOffset += (rowBytes + 1) * passHeight;
                }
            }

            return image;
        }

        private static Header ReadHeader(byte[] png, int offset, int length)
        {
            if (length != 13)
                throw new InvalidDataException("Invalid PNG IHDR chunk length.");

            var width = ReadUInt32BigEndian(png, offset);
            var height = ReadUInt32BigEndian(png, offset + 4);
            if (width == 0 || height == 0 || width > int.MaxValue || height > int.MaxValue || (long)width * height > MaxPixels)
                throw new InvalidDataException($"Unsupported PNG dimensions {width}x{height}.");

            var header = new Header
            {
                Width = (int)width,
                Height = (int)height,
                BitDepth = png[offset + 8],
                ColorType = (PngEncoder.ColorType)png[offset + 9],
            };

            if (png[offset + 10] != 0 || png[offset + 11] != 0)
                throw new InvalidDataException("Unsupported PNG compression or filter method.");
            if (png[offset + 12] > 1)
                throw new InvalidDataException("Unsupported PNG interlace method.");
            header.Interlaced = png[offset + 12] == 1;

            bool validDepth;
            switch (header.ColorType)
            {
                case PngEncoder.ColorType.Greyscale:
                    header.Channels = 1;
                    validDepth = header.BitDepth == 1 || header.BitDepth == 2 || header.BitDepth == 4 || header.BitDepth == 8 || header.BitDepth == 16;
                    break;
                case PngEncoder.ColorType.Indexed:
                    header.Channels = 1;
                    validDepth = header.BitDepth == 1 || header.BitDepth == 2 || header.BitDepth == 4 || header.BitDepth == 8;
                    break;
                case PngEncoder.ColorType.Rgb:
                    header.Channels = 3;
                    validDepth = header.BitDepth == 8 || header.BitDepth == 16;
                    break;
                case PngEncoder.ColorType.GreyscaleAlpha:
                    header.Channels = 2;
                    validDepth = header.BitDepth == 8 || header.BitDepth == 16;
                    break;
                case PngEncoder.ColorType.Rgba:
                    header.Channels = 4;
                    validDepth = header.BitDepth == 8 || header.BitDepth == 16;
                    break;
                default:
                    throw new InvalidDataException($"Unsupported PNG color type {(byte)header.ColorType}.");
            }
            if (!validDepth)
                throw new InvalidDataException($"Invalid PNG bit depth {header.BitDepth} for color type {(byte)header.ColorType}.");

            return header;
        }

        private static long GetRawDataLength(Header header)
        {
            if (!header.Interlaced)
                return (long)(header.RowBytes(header.Width) + 1) * header.Height;

            long total = 0;
            for (var pass = 0; pass < 7; pass++)
            {
                GetPassSize(header, pass, out var passWidth, out var passHeight);
                if (passWidth > 0 && passHeight > 0)
                    total += (long)(header.RowBytes(passWidth) + 1) * passHeight;
            }
            return total;
        }

        private static void GetPassSize(Header header, int pass, out int width, out int height)
        {
            width = (header.Width - Adam7XStart[pass] + Adam7XStep[pass] - 1) / Adam7XStep[pass];
            height = (header.Height - Adam7YStart[pass] + Adam7YStep[pass] - 1) / Adam7YStep[pass];
            width = Math.Max(0, width);
            height = Math.Max(0, height);
        }

        /// <summary>
        /// Reverses the PNG row filters in place. Each row starts with its filter type byte.
        /// </summary>
        private static void Unfilter(byte[] data, int offset, int rowBytes, int rows, int bpp)
        {
            var stride = rowBytes + 1;
            for (var y = 0; y < rows; y++)
            {
                var row = offset + y * stride + 1;
                var prev = y > 0 ? row - stride : -1;
                var filter = data[row - 1];

                switch ((PngEncoder.FilterType)filter)
                {
                    case PngEncoder.FilterType.None:
                        break;
                    case PngEncoder.FilterType.Sub:
                        for (var i = bpp; i < rowBytes; i++)
                            data[row + i] += data[row + i - bpp];
                        break;
                    case PngEncoder.FilterType.Up:
                        if (prev >= 0)
                            for (var i = 0; i < rowBytes; i++)
                                data[row + i] += data[prev + i];
                        break;
                    case PngEncoder.FilterType.Average:
                        for (var i = 0; i < rowBytes; i++)
                        {
                            var left = i >= bpp ? data[row + i - bpp] : 0;
                            var up = prev >= 0 ? data[prev + i] : 0;
                            data[row + i] += (byte)((left + up) >> 1);
                        }
                        break;
                    case PngEncoder.FilterType.Paeth:
                        for (var i = 0; i < rowBytes; i++)
                        {
                            var left = i >= bpp ? data[row + i - bpp] : 0;
                            var up = prev >= 0 ? data[prev + i] : 0;
                            var upLeft = prev >= 0 && i >= bpp ? data[prev + i - bpp] : 0;
                            data[row + i] += PaethPredictor(left, up, upLeft);
                        }
                        break;
                    default:
                        throw new InvalidDataException($"Invalid PNG filter type {filter}.");
                }
            }
        }

        private static byte PaethPredictor(int a, int b, int c)
        {
            var p = a + b - c;
            var pa = Math.Abs(p - a);
            var pb = Math.Abs(p - b);
            var pc = Math.Abs(p - c);
            if (pa <= pb && pa <= pc)
                return (byte)a;
            return pb <= pc ? (byte)b : (byte)c;
        }

        private static void WritePixels(Header header, byte[] data, int offset, int width, int height,
            int xStart, int yStart, int xStep, int yStep, byte[] palette, byte[] transparency, RgbaImage image)
        {
            var stride = header.RowBytes(width) + 1;
            var depth = header.BitDepth;
            var pixels = image.Pixels;

            // tRNS for greyscale/RGB defines a single fully transparent color, compared at full sample precision.
            var transparentGrey = -1;
            int transparentR = -1, transparentG = -1, transparentB = -1;
            if (transparency != null)
            {
                if (header.ColorType == PngEncoder.ColorType.Greyscale && transparency.Length >= 2)
                    transparentGrey = (transparency[0] << 8) | transparency[1];
                else if (header.ColorType == PngEncoder.ColorType.Rgb && transparency.Length >= 6)
                {
                    transparentR = (transparency[0] << 8) | transparency[1];
                    transparentG = (transparency[2] << 8) | transparency[3];
                    transparentB = (transparency[4] << 8) | transparency[5];
                }
            }

            for (var y = 0; y < height; y++)
            {
                var row = offset + y * stride + 1;
                var targetY = yStart + y * yStep;
                for (var x = 0; x < width; x++)
                {
                    var o = (targetY * image.Width + xStart + x * xStep) * 4;
                    var s = x * header.Channels;

                    switch (header.ColorType)
                    {
                        case PngEncoder.ColorType.Greyscale:
                        {
                            var v = ReadSample(data, row, s, depth);
                            var grey = ScaleTo8Bit(v, depth);
                            pixels[o] = pixels[o + 1] = pixels[o + 2] = grey;
                            pixels[o + 3] = v == transparentGrey ? (byte)0 : (byte)255;
                            break;
                        }
                        case PngEncoder.ColorType.Rgb:
                        {
                            var r = ReadSample(data, row, s, depth);
                            var g = ReadSample(data, row, s + 1, depth);
                            var b = ReadSample(data, row, s + 2, depth);
                            pixels[o] = ScaleTo8Bit(r, depth);
                            pixels[o + 1] = ScaleTo8Bit(g, depth);
                            pixels[o + 2] = ScaleTo8Bit(b, depth);
                            pixels[o + 3] = r == transparentR && g == transparentG && b == transparentB ? (byte)0 : (byte)255;
                            break;
                        }
                        case PngEncoder.ColorType.Indexed:
                        {
                            var index = ReadSample(data, row, s, depth);
                            if (index * 3 + 2 >= palette.Length)
                                throw new InvalidDataException($"PNG palette index {index} out of range.");
                            pixels[o] = palette[index * 3];
                            pixels[o + 1] = palette[index * 3 + 1];
                            pixels[o + 2] = palette[index * 3 + 2];
                            pixels[o + 3] = transparency != null && index < transparency.Length ? transparency[index] : (byte)255;
                            break;
                        }
                        case PngEncoder.ColorType.GreyscaleAlpha:
                        {
                            var grey = ScaleTo8Bit(ReadSample(data, row, s, depth), depth);
                            pixels[o] = pixels[o + 1] = pixels[o + 2] = grey;
                            pixels[o + 3] = ScaleTo8Bit(ReadSample(data, row, s + 1, depth), depth);
                            break;
                        }
                        case PngEncoder.ColorType.Rgba:
                        {
                            pixels[o] = ScaleTo8Bit(ReadSample(data, row, s, depth), depth);
                            pixels[o + 1] = ScaleTo8Bit(ReadSample(data, row, s + 1, depth), depth);
                            pixels[o + 2] = ScaleTo8Bit(ReadSample(data, row, s + 2, depth), depth);
                            pixels[o + 3] = ScaleTo8Bit(ReadSample(data, row, s + 3, depth), depth);
                            break;
                        }
                    }
                }
            }
        }

        private static int ReadSample(byte[] data, int row, int sampleIndex, int bitDepth)
        {
            switch (bitDepth)
            {
                case 8:
                    return data[row + sampleIndex];
                case 16:
                    return (data[row + sampleIndex * 2] << 8) | data[row + sampleIndex * 2 + 1];
                default:
                    var bitOffset = sampleIndex * bitDepth;
                    var shift = 8 - bitDepth - (bitOffset & 7);
                    return (data[row + (bitOffset >> 3)] >> shift) & ((1 << bitDepth) - 1);
            }
        }

        private static byte ScaleTo8Bit(int value, int bitDepth)
        {
            switch (bitDepth)
            {
                case 1: return (byte)(value * 255);
                case 2: return (byte)(value * 85);
                case 4: return (byte)(value * 17);
                case 16: return (byte)(value >> 8);
                default: return (byte)value;
            }
        }

        private static byte[] Slice(byte[] data, int offset, int length)
        {
            var result = new byte[length];
            Buffer.BlockCopy(data, offset, result, 0, length);
            return result;
        }

        private static uint ReadUInt32BigEndian(byte[] data, int offset) =>
            ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) | ((uint)data[offset + 2] << 8) | data[offset + 3];
    }
}
