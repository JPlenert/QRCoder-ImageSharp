using System;
using System.IO;
using System.IO.Compression;

namespace QRCoder.Imaging
{
    /// <summary>
    /// zlib (RFC 1950) wrapper around the deflate implementation of the BCL.
    /// </summary>
    internal static class Zlib
    {
        public static byte[] Compress(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                output.WriteByte(0x78); // CM 8 = deflate, CINFO 7 = 32K window
                output.WriteByte(0x9C); // Default compression, check bits
                using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
                {
                    deflate.Write(data, 0, data.Length);
                }
                var adler = Adler32(data, 0, data.Length);
                output.WriteByte((byte)(adler >> 24));
                output.WriteByte((byte)(adler >> 16));
                output.WriteByte((byte)(adler >> 8));
                output.WriteByte((byte)adler);
                return output.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data, int expectedLength)
        {
            if (data.Length < 2)
                throw new InvalidDataException("zlib stream is too short.");
            var cmf = data[0];
            var flg = data[1];
            if ((cmf & 0x0F) != 8 || ((cmf << 8) | flg) % 31 != 0)
                throw new InvalidDataException("Invalid zlib header.");
            if ((flg & 0x20) != 0)
                throw new InvalidDataException("zlib preset dictionaries are not supported.");

            var result = new byte[expectedLength];
            using (var input = new MemoryStream(data, 2, data.Length - 2))
            using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
            {
                var offset = 0;
                while (offset < expectedLength)
                {
                    var read = deflate.Read(result, offset, expectedLength - offset);
                    if (read == 0)
                        throw new InvalidDataException("Compressed image data is truncated.");
                    offset += read;
                }
            }
            return result;
        }

        public static uint Adler32(byte[] data, int index, int length)
        {
            const uint Base = 65521;
            // Largest n such that 255n(n+1)/2 + (n+1)(Base-1) <= 2^32-1, so the modulo can be deferred.
            const int MaxBlock = 5552;
            uint s1 = 1, s2 = 0;

            while (length > 0)
            {
                var block = Math.Min(length, MaxBlock);
                length -= block;
                while (block-- > 0)
                {
                    s1 += data[index++];
                    s2 += s1;
                }
                s1 %= Base;
                s2 %= Base;
            }

            return (s2 << 16) | s1;
        }
    }

    /// <summary>
    /// CRC-32 as used by PNG chunks (ISO 3309 / ITU-T V.42).
    /// </summary>
    internal static class Crc32
    {
        private static readonly uint[] Table = CreateTable();

        private static uint[] CreateTable()
        {
            var table = new uint[256];
            for (uint n = 0; n < 256; n++)
            {
                var c = n;
                for (var k = 0; k < 8; k++)
                    c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
                table[n] = c;
            }
            return table;
        }

        public static uint Update(uint crc, byte[] data, int index, int length)
        {
            var c = crc;
            var end = index + length;
            for (var n = index; n < end; n++)
                c = Table[(c ^ data[n]) & 0xFF] ^ (c >> 8);
            return c;
        }

        public static uint Compute(byte[] type, byte[] data) =>
            Update(Update(0xFFFFFFFF, type, 0, type.Length), data, 0, data.Length) ^ 0xFFFFFFFF;
    }
}
