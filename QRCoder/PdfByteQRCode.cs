using QRCoder.Imaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using static QRCoder.QRCodeGenerator;

/* This renderer is inspired by RemusVasii: https://github.com/codebude/QRCoder/issues/223 */
namespace QRCoder
{

    // ReSharper disable once InconsistentNaming
    public class PdfByteQRCode : AbstractQRCode, IDisposable
    {
        private readonly byte[] pdfBinaryComment = new byte[] { 0x25, 0xe2, 0xe3, 0xcf, 0xd3 };

        /// <summary>
        /// Constructor without params to be used in COM Objects connections
        /// </summary>
        public PdfByteQRCode() { }

        public PdfByteQRCode(QRCodeData data) : base(data) { }

        /// <summary>
        /// Creates a PDF document with a black & white QR code
        /// </summary>
        /// <param name="pixelsPerModule"></param>
        /// <returns></returns>
        public byte[] GetGraphic(int pixelsPerModule)
        {
            return GetGraphic(pixelsPerModule, "#000000", "#ffffff");
        }

        /// <summary>
        /// Creates a PDF document with given colors DPI and quality
        /// </summary>
        /// <param name="pixelsPerModule"></param>
        /// <param name="darkColorHtmlHex"></param>
        /// <param name="lightColorHtmlHex"></param>
        /// <param name="dpi"></param>
        /// <returns></returns>
        public byte[] GetGraphic(int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex, int dpi = 150)
        {
            var imgSize = ModuleScanlines.GetSize(QrCodeData.ModuleMatrix, pixelsPerModule, true);
            var pdfMediaSize = (imgSize * 72 / dpi).ToString(CultureInfo.InvariantCulture);
            var darkColor = QRColor.Parse(darkColorHtmlHex);
            var lightColor = QRColor.Parse(lightColorHtmlHex);

            // 1-bit image (dark = 0, light = 1) with PNG row filters, as supported by FlateDecode's predictor 15
            var imgData = Zlib.Compress(ModuleScanlines.Build1Bit(QrCodeData.ModuleMatrix, pixelsPerModule, true));
            var palette = $"{darkColor.R:X2}{darkColor.G:X2}{darkColor.B:X2}{lightColor.R:X2}{lightColor.G:X2}{lightColor.B:X2}";
            
            //Create PDF document
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream, System.Text.Encoding.GetEncoding("ASCII"));

                var xrefs = new List<long>();

                writer.Write("%PDF-1.5\r\n");
                writer.Flush();

                stream.Write(pdfBinaryComment, 0, pdfBinaryComment.Length);
                writer.WriteLine();

                writer.Flush();
                xrefs.Add(stream.Position);

                writer.Write(
                    xrefs.Count.ToString() + " 0 obj\r\n" +
                    "<<\r\n" +
                    "/Type /Catalog\r\n" +
                    "/Pages 2 0 R\r\n" +
                    ">>\r\n" +
                    "endobj\r\n"
                );

                writer.Flush();
                xrefs.Add(stream.Position);

                writer.Write(
                    xrefs.Count.ToString() + " 0 obj\r\n" +
                    "<<\r\n" +
                    "/Count 1\r\n" +
                    "/Kids [ <<\r\n" +
                    "/Type /Page\r\n" +
                    "/Parent 2 0 R\r\n" +
                    "/MediaBox [0 0 " + pdfMediaSize + " " + pdfMediaSize + "]\r\n" +
                    "/Resources << /ProcSet [ /PDF /ImageC /ImageI ]\r\n" +
                    "/XObject << /Im1 4 0 R >> >>\r\n" +
                    "/Contents 3 0 R\r\n" +
                    ">> ]\r\n" +
                    ">>\r\n" +
                    "endobj\r\n"
                );

                var X = "q\r\n" +
                    pdfMediaSize + " 0 0 " + pdfMediaSize + " 0 0 cm\r\n" +
                    "/Im1 Do\r\n" +
                    "Q";

                writer.Flush();
                xrefs.Add(stream.Position);

                writer.Write(
                    xrefs.Count.ToString() + " 0 obj\r\n" +
                    "<< /Length " + X.Length.ToString() + " >>\r\n" +
                    "stream\r\n" +
                    X + "endstream\r\n" +
                    "endobj\r\n"
                );

                writer.Flush();
                xrefs.Add(stream.Position);

                writer.Write(
                    xrefs.Count.ToString() + " 0 obj\r\n" +
                    "<<\r\n" +
                    "/Name /Im1\r\n" +
                    "/Type /XObject\r\n" +
                    "/Subtype /Image\r\n" +
                    "/Width " + imgSize.ToString() + "/Height " + imgSize.ToString() + "/Length 5 0 R\r\n" +
                    "/Filter /FlateDecode\r\n" +
                    "/DecodeParms << /Predictor 15 /Colors 1 /BitsPerComponent 1 /Columns " + imgSize.ToString() + " >>\r\n" +
                    "/ColorSpace [ /Indexed /DeviceRGB 1 <" + palette + "> ]\r\n" +
                    "/BitsPerComponent 1\r\n" +
                    ">>\r\n" +
                    "stream\r\n"
                );
                writer.Flush();
                stream.Write(imgData, 0, imgData.Length);
                writer.Write(
                    "\r\n" +
                    "endstream\r\n" +
                    "endobj\r\n"
                );

                writer.Flush();
                xrefs.Add(stream.Position);

                writer.Write(
                    xrefs.Count.ToString() + " 0 obj\r\n" +
                    imgData.Length.ToString() + " endobj\r\n"
                );

                writer.Flush();
                var startxref = stream.Position;

                writer.Write(
                    "xref\r\n" +
                    "0 " + (xrefs.Count + 1).ToString() + "\r\n" +
                    "0000000000 65535 f\r\n"
                );

                foreach (var refValue in xrefs)
                    writer.Write(refValue.ToString("0000000000") + " 00000 n\r\n");

                writer.Write(
                    "trailer\r\n" +
                    "<<\r\n" +
                    "/Size " + (xrefs.Count + 1).ToString() + "\r\n" +
                    "/Root 1 0 R\r\n" +
                    ">>\r\n" +
                    "startxref\r\n" +
                    startxref.ToString() + "\r\n" +
                    "%%EOF"
                );

                writer.Flush();

                stream.Position = 0;

                return stream.ToArray();
            }
        }
    }

    public static class PdfByteQRCodeHelper
    {
        public static byte[] GetQRCode(string plainText, int pixelsPerModule, string darkColorHtmlHex,
            string lightColorHtmlHex, ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false,
            EciMode eciMode = EciMode.Default, int requestedVersion = -1)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (
                var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode,
                    requestedVersion))
            using (var qrCode = new PdfByteQRCode(qrCodeData))
                return qrCode.GetGraphic(pixelsPerModule, darkColorHtmlHex, lightColorHtmlHex);
        }

        public static byte[] GetQRCode(string txt, ECCLevel eccLevel, int size)
        {
            using (var qrGen = new QRCodeGenerator())
            using (var qrCode = qrGen.CreateQrCode(txt, eccLevel))
            using (var qrBmp = new PdfByteQRCode(qrCode))
                return qrBmp.GetGraphic(size);

        }
    }
}
