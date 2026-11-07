using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using QRCoder;
using QRCoder.Imaging;
using QRCoderTests.Helpers;
using QRCoderTests.Helpers.XUnitExtenstions;
using Shouldly;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using PngDecoder = QRCoder.Imaging.PngDecoder;
using PngEncoder = QRCoder.Imaging.PngEncoder;
using ReferencePngEncoder = SixLabors.ImageSharp.Formats.Png.PngEncoder;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace QRCoderTests
{
    /****************************************************************************************************
     * Tests for the dependency-free imaging code. ImageSharp serves as reference implementation.
     ****************************************************************************************************/
    public class PngCodecTests
    {
        const string QRCodeContent = "This is a quick test! 123#?";

        // Odd dimensions so rows don't end on byte boundaries for sub-byte bit depths
        private static Image<Rgba32> CreateTestImage(int width = 37, int height = 23)
        {
            var image = new Image<Rgba32>(width, height);
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    image[x, y] = new Rgba32((byte)(x * 7), (byte)(y * 11), (byte)((x * y) % 256), (byte)(255 - x * 3));
            return image;
        }

        private static byte[] EncodeWithImageSharp(Image<Rgba32> image, PngColorType colorType, PngBitDepth bitDepth, PngInterlaceMode interlace)
        {
            using (var ms = new MemoryStream())
            {
                image.SaveAsPng(ms, new ReferencePngEncoder { ColorType = colorType, BitDepth = bitDepth, InterlaceMethod = interlace });
                return ms.ToArray();
            }
        }

        private static void ShouldMatchPixels(Image<Rgba32> expected, RgbaImage actual, int tolerance)
        {
            actual.Width.ShouldBe(expected.Width);
            actual.Height.ShouldBe(expected.Height);
            for (var y = 0; y < expected.Height; y++)
                for (var x = 0; x < expected.Width; x++)
                {
                    var e = expected[x, y];
                    var a = actual.GetPixel(x, y);
                    var diff = Math.Max(Math.Max(Math.Abs(e.R - a.R), Math.Abs(e.G - a.G)), Math.Max(Math.Abs(e.B - a.B), Math.Abs(e.A - a.A)));
                    diff.ShouldBeLessThanOrEqualTo(tolerance, $"pixel ({x},{y}): expected {e}, got {a}");
                }
        }

        [Theory]
        [Category("Imaging/PngDecoder")]
        [InlineData(PngColorType.Grayscale, PngBitDepth.Bit1)]
        [InlineData(PngColorType.Grayscale, PngBitDepth.Bit2)]
        [InlineData(PngColorType.Grayscale, PngBitDepth.Bit4)]
        [InlineData(PngColorType.Grayscale, PngBitDepth.Bit8)]
        [InlineData(PngColorType.Grayscale, PngBitDepth.Bit16)]
        [InlineData(PngColorType.GrayscaleWithAlpha, PngBitDepth.Bit8)]
        [InlineData(PngColorType.GrayscaleWithAlpha, PngBitDepth.Bit16)]
        [InlineData(PngColorType.Rgb, PngBitDepth.Bit8)]
        [InlineData(PngColorType.Rgb, PngBitDepth.Bit16)]
        [InlineData(PngColorType.RgbWithAlpha, PngBitDepth.Bit8)]
        [InlineData(PngColorType.RgbWithAlpha, PngBitDepth.Bit16)]
        [InlineData(PngColorType.Palette, PngBitDepth.Bit1)]
        [InlineData(PngColorType.Palette, PngBitDepth.Bit2)]
        [InlineData(PngColorType.Palette, PngBitDepth.Bit4)]
        [InlineData(PngColorType.Palette, PngBitDepth.Bit8)]
        public void decoder_matches_reference(PngColorType colorType, PngBitDepth bitDepth)
        {
            foreach (var interlace in new[] { PngInterlaceMode.None, PngInterlaceMode.Adam7 })
            {
                using (var source = CreateTestImage())
                {
                    var png = EncodeWithImageSharp(source, colorType, bitDepth, interlace);
                    using (var reference = Image.Load<Rgba32>(png))
                    {
                        // 16-bit samples: we truncate to the high byte, ImageSharp rounds
                        ShouldMatchPixels(reference, PngDecoder.Decode(png), bitDepth == PngBitDepth.Bit16 ? 1 : 0);
                    }
                }
            }
        }

        [Fact]
        [Category("Imaging/PngDecoder")]
        public void decoder_reads_asset_logo()
        {
            var png = HelperFunctions.LoadAssetImage();
            using (var reference = Image.Load<Rgba32>(png))
                ShouldMatchPixels(reference, PngDecoder.Decode(png), 0);
        }

        [Fact]
        [Category("Imaging/PngDecoder")]
        public void decoder_rejects_invalid_data()
        {
            Should.Throw<InvalidDataException>(() => PngDecoder.Decode(Encoding.ASCII.GetBytes("GIF89a this is not a png")));

            var png = HelperFunctions.LoadAssetImage();
            png[40] ^= 0xFF; // Inside the IDAT chunk
            Should.Throw<InvalidDataException>(() => PngDecoder.Decode(png));

            Should.Throw<InvalidDataException>(() => PngDecoder.Decode(HelperFunctions.LoadAssetImage().AsSpan(0, 100).ToArray()));
        }

        [Fact]
        [Category("Imaging/PngEncoder")]
        public void rgba_encoder_roundtrips_through_reference()
        {
            using (var source = CreateTestImage(41, 19))
            {
                var image = new RgbaImage(source.Width, source.Height);
                for (var y = 0; y < source.Height; y++)
                    for (var x = 0; x < source.Width; x++)
                    {
                        var p = source[x, y];
                        image.SetPixel(x, y, new QRColor(p.R, p.G, p.B, p.A));
                    }

                var png = PngEncoder.EncodeRgba(image);
                using (var decoded = Image.Load<Rgba32>(png))
                    ShouldMatchPixels(decoded, image, 0);
                ShouldMatchPixels(source, PngDecoder.Decode(png), 0);
            }
        }

        [Theory]
        [Category("Imaging/PngEncoder")]
        [InlineData(1, true)]
        [InlineData(3, false)]
        [InlineData(7, true)]
        [InlineData(20, false)]
        public void indexed_encoder_with_transparency_roundtrips(int pixelsPerModule, bool drawQuietZones)
        {
            var dark = new QRColor(10, 20, 30);
            var light = new QRColor(200, 100, 50, 0);
            var png = HelperFunctions.GeneratePng(QRCodeContent, pr => pr.GetGraphic(pixelsPerModule, dark.ToRgbaBytes(), light.ToRgbaBytes(), drawQuietZones));

            using (var reference = Image.Load<Rgba32>(png))
            {
                var decoded = PngDecoder.Decode(png);
                ShouldMatchPixels(reference, decoded, 0);
                decoded.GetPixel(0, 0).ShouldBe(drawQuietZones ? light : dark);
            }
        }

        [Fact]
        [Category("Imaging/RgbaImage")]
        public void resize_keeps_uniform_color_and_alpha()
        {
            var image = new RgbaImage(100, 60);
            image.FillRect(0, 0, 100, 60, new QRColor(12, 34, 56, 128));
            foreach (var (w, h) in new[] { (33, 17), (250, 151), (1, 1), (100, 60) })
            {
                var resized = image.Resize(w, h);
                for (var y = 0; y < h; y++)
                    for (var x = 0; x < w; x++)
                        resized.GetPixel(x, y).ShouldBe(new QRColor(12, 34, 56, 128));
            }
        }

        [Fact]
        [Category("Imaging/RgbaImage")]
        public void draw_over_composites_alpha()
        {
            var target = new RgbaImage(2, 1);
            target.FillRect(0, 0, 2, 1, QRColor.White);
            var source = new RgbaImage(2, 1);
            source.SetPixel(0, 0, new QRColor(0, 0, 0, 128));
            source.SetPixel(1, 0, QRColor.Transparent);

            target.DrawOver(source, 0, 0);

            target.GetPixel(0, 0).ShouldBe(new QRColor(127, 127, 127));
            target.GetPixel(1, 0).ShouldBe(QRColor.White);
        }

        [Theory]
        [Category("Imaging/QRColor")]
        [InlineData("#000", 0, 0, 0, 255)]
        [InlineData("#fff8", 255, 255, 255, 0x88)]
        [InlineData("#1a2B3c", 0x1a, 0x2b, 0x3c, 255)]
        [InlineData("1a2b3c4d", 0x1a, 0x2b, 0x3c, 0x4d)]
        [InlineData("DarkGreen", 0, 100, 0, 255)]
        [InlineData("transparent", 0, 0, 0, 0)]
        public void color_parses(string value, byte r, byte g, byte b, byte a)
        {
            QRColor.Parse(value).ShouldBe(new QRColor(r, g, b, a));
        }

        [Theory]
        [Category("Imaging/QRColor")]
        [InlineData("")]
        [InlineData("#12")]
        [InlineData("#gggggg")]
        [InlineData("notacolor")]
        public void color_rejects_invalid(string value)
        {
            Should.Throw<FormatException>(() => QRColor.Parse(value));
        }

        [Fact]
        [Category("Imaging/QRColor")]
        public void color_to_hex()
        {
            new QRColor(0x1a, 0x2b, 0x3c).ToHex().ShouldBe("1A2B3CFF");
        }

        [Fact]
        [Category("QRRenderer/Base64QRCode")]
        public void can_render_base64_qrcode()
        {
            var data = new QRCodeGenerator().CreateQrCode(QRCodeContent, QRCodeGenerator.ECCLevel.H);
            var plain = new Base64QRCode(data).GetGraphic(5);
            HelperFunctions.TestByDecode(Convert.FromBase64String(plain), QRCodeContent);

            var withIcon = new Base64QRCode(data).GetGraphic(10, QRColor.Black, QRColor.White, HelperFunctions.LoadAssetImage());
            HelperFunctions.TestByDecode(Convert.FromBase64String(withIcon), QRCodeContent);
        }

        [Fact]
        [Category("QRRenderer/PdfByteQRCode")]
        public void can_render_pdf_qrcode()
        {
            const int pixelsPerModule = 5;
            var data = new QRCodeGenerator().CreateQrCode(QRCodeContent, QRCodeGenerator.ECCLevel.H);
            var pdf = new PdfByteQRCode(data).GetGraphic(pixelsPerModule, "#102030", "#ffffff");
            var text = Encoding.ASCII.GetString(pdf);

            text.ShouldStartWith("%PDF-1.5");
            text.ShouldEndWith("%%EOF");
            text.ShouldContain("/Filter /FlateDecode");
            text.ShouldContain("/ColorSpace [ /Indexed /DeviceRGB 1 <102030FFFFFF> ]");
            text.ShouldNotContain("DCTDecode");

            // The embedded image stream must inflate to the 1-bit PNG scanlines of the code
            const string marker = "stream\r\n";
            var start = text.IndexOf(marker, text.IndexOf("/Subtype /Image", StringComparison.Ordinal), StringComparison.Ordinal) + marker.Length;
            var end = text.IndexOf("\r\nendstream", start, StringComparison.Ordinal);
            var size = data.ModuleMatrix.Count * pixelsPerModule;
            var expectedLength = ((size + 7) / 8 + 1) * size;

            byte[] inflated;
            using (var input = new MemoryStream(pdf, start + 2, end - start - 2)) // skip zlib header
            using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                deflate.CopyTo(output);
                inflated = output.ToArray();
            }
            inflated.Length.ShouldBe(expectedLength);
            inflated.ShouldBe(ModuleScanlines.Build1Bit(data.ModuleMatrix, pixelsPerModule, true));
        }
    }
}
