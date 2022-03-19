using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Shouldly;
using QRCoder;
using System.Reflection;

namespace QRCoderTests.Helpers
{
    public static class HelperFunctions
    {
        public static string BitmapToHash(Image img)
        {
            byte[] imgBytes = null;
            using (var ms = new MemoryStream())
            {
                img.SaveAsPng(ms);
                imgBytes = ms.ToArray();
                ms.Dispose();
            }
            return ByteArrayToHash(imgBytes);
        }

        public static Image LoadAssetImage() =>
            Image.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("QRCoderTests.assets.noun_software engineer_2909346.png"));

        public static string LoadAssetSvg()
        {
            using (var sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"QRCoderTests.assets.noun_Scientist_2909361.svg")))
                return sr.ReadToEnd();
        }

        public static string ByteArrayToHash(byte[] data)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public static string StringToHash(string data)
        {
            return ByteArrayToHash(Encoding.UTF8.GetBytes(data));
        }

        public static void TestByDecode(Image<Rgba32> image, string desiredContent)
        {
            ZXing.ImageSharp.BarcodeReader<Rgba32> reader = new ZXing.ImageSharp.BarcodeReader<Rgba32>();
            ZXing.Result result = reader.Decode(image);
            result.Text.ShouldBe(desiredContent);
        }

        public static void TestByDecode(byte[] pngCodeGfx, string desiredContent)
        {
            using (var mStream = new MemoryStream(pngCodeGfx))
            {
                ZXing.Result result;

                Image image = Image.Load(mStream);
                Type pixelType = image.GetType().GetGenericArguments()[0];
                if (pixelType == typeof(Rgba32))
                {
                    ZXing.ImageSharp.BarcodeReader<Rgba32> reader = new ZXing.ImageSharp.BarcodeReader<Rgba32>();
                    result = reader.Decode(image as Image<Rgba32>);
                }
                else if (pixelType == typeof(L8))
                {
                    ZXing.ImageSharp.BarcodeReader<L8> reader = new ZXing.ImageSharp.BarcodeReader<L8>();
                    result = reader.Decode(image as Image<L8>);
                }
                else
                    throw new NotImplementedException(pixelType.ToString());
                result.Text.ShouldBe(desiredContent);
            }
        }

        public static void TestByHash(Image<Rgba32> image, string desiredHash) =>
            BitmapToHash(image).ShouldBe(desiredHash);

        public static void TestByHash(byte[] pngCodeGfx, string desiredHash)
        {
            using (var mStream = new MemoryStream(pngCodeGfx))
            {
                var img = Image.Load(mStream);
                var result = BitmapToHash(img);
                result.ShouldBe(desiredHash);
            }
        }

        public static void TestByHash(string svg, string desiredHash) =>
            ByteArrayToHash(UTF8Encoding.UTF8.GetBytes(desiredHash));

        public static void TestImageToFile(string path, string testName, Image<Rgba32> image)
        {
            if (String.IsNullOrEmpty(path))
                return;

            image.Save(Path.Combine(path, $"qrtest_{testName}.png"));
        }

        public static void TestImageToFile(string path, string testName, byte[] data)
        {
            if (String.IsNullOrEmpty(path))
                return;
            //Used logo is licensed under public domain. Ref.: https://thenounproject.com/Iconathon1/collection/redefining-women/?i=2909346
            File.WriteAllBytes(Path.Combine(path, $"qrtestPNG_{testName}.png"), data);
        }

        public static void TestImageToFile(string path, string testName, string svg)
        {
            if (String.IsNullOrEmpty(path))
                return;

            //Used logo is licensed under public domain. Ref.: https://thenounproject.com/Iconathon1/collection/redefining-women/?i=2909346
            File.WriteAllText(Path.Combine(path, $"qrtestSVG_{testName}.svg"), svg);
        }

        public static Image<Rgba32> GenerateImage(string content, Func<QRCode, Image<Rgba32>> getGraphic)
        {
            QRCodeGenerator gen = new QRCodeGenerator();
            QRCodeData data = gen.CreateQrCode(content, QRCodeGenerator.ECCLevel.H);
            return getGraphic(new QRCode(data));
        }

        public static byte[] GeneratePng(string content, Func<PngByteQRCode, byte[]> getGraphic)
        {
            QRCodeGenerator gen = new QRCodeGenerator();
            QRCodeData data = gen.CreateQrCode(content, QRCodeGenerator.ECCLevel.L);
            return getGraphic(new PngByteQRCode(data));
        }

        public static string GenerateSvg(string content, Func<SvgQRCode, string> getGraphic)
        {
            QRCodeGenerator gen = new QRCodeGenerator();
            QRCodeData data = gen.CreateQrCode(content, QRCodeGenerator.ECCLevel.H);
            return getGraphic(new SvgQRCode(data));
        }
    }
}
