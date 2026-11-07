using System;
using QRCoder.Imaging;
using static QRCoder.QRCodeGenerator;

namespace QRCoder
{
    public sealed class PngByteQRCode : AbstractQRCode, IDisposable
    {
        /// <summary>
        /// Constructor without params to be used in COM Objects connections
        /// </summary>
        public PngByteQRCode() { }

        public PngByteQRCode(QRCodeData data) : base(data)
        {
        }


        /// <summary>
        /// Creates a black & white PNG of the QR code, using 1-bit grayscale.
        /// </summary>
        public byte[] GetGraphic(int pixelsPerModule, bool drawQuietZones = true)
        {
            var size = ModuleScanlines.GetSize(this.QrCodeData.ModuleMatrix, pixelsPerModule, drawQuietZones);
            var scanlines = ModuleScanlines.Build1Bit(this.QrCodeData.ModuleMatrix, pixelsPerModule, drawQuietZones);
            return PngEncoder.EncodeGreyscale1Bit(size, size, scanlines);
        }

        /// <summary>
        /// Creates 2-color PNG of the QR code, using 1-bit indexed color. Accepts 3-byte RGB colors for normal images and 4-byte RGBA-colors for transparent images.
        /// </summary>
        public byte[] GetGraphic(int pixelsPerModule, byte[] darkColorRgba, byte[] lightColorRgba, bool drawQuietZones = true)
        {
            return GetGraphic(pixelsPerModule, ToColor(darkColorRgba), ToColor(lightColorRgba), drawQuietZones);
        }

        /// <summary>
        /// Creates 2-color PNG of the QR code, using 1-bit indexed color.
        /// </summary>
        public byte[] GetGraphic(int pixelsPerModule, QRColor darkColor, QRColor lightColor, bool drawQuietZones = true)
        {
            var size = ModuleScanlines.GetSize(this.QrCodeData.ModuleMatrix, pixelsPerModule, drawQuietZones);
            var scanlines = ModuleScanlines.Build1Bit(this.QrCodeData.ModuleMatrix, pixelsPerModule, drawQuietZones);
            return PngEncoder.EncodeIndexed1Bit(size, size, scanlines, darkColor, lightColor);
        }

        private static QRColor ToColor(byte[] rgba)
        {
            if (rgba == null || rgba.Length < 3)
                throw new ArgumentException("Color must consist of 3 (RGB) or 4 (RGBA) bytes.", nameof(rgba));
            return new QRColor(rgba[0], rgba[1], rgba[2], rgba.Length > 3 ? rgba[3] : (byte)255);
        }
    }

    public static class PngByteQRCodeHelper
    {
        public static byte[] GetQRCode(string plainText, int pixelsPerModule, byte[] darkColorRgba, byte[] lightColorRgba, ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false, EciMode eciMode = EciMode.Default, int requestedVersion = -1, bool drawQuietZones = true)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode, requestedVersion))
            using (var qrCode = new PngByteQRCode(qrCodeData))
                return qrCode.GetGraphic(pixelsPerModule, darkColorRgba, lightColorRgba, drawQuietZones);
        }
    


        public static byte[] GetQRCode(string txt, QRCodeGenerator.ECCLevel eccLevel, int size, bool drawQuietZones = true)
        {
            using (var qrGen = new QRCodeGenerator())
            using (var qrCode = qrGen.CreateQrCode(txt, eccLevel))
            using (var qrPng = new PngByteQRCode(qrCode))
                return qrPng.GetGraphic(size, drawQuietZones);
        }
    }    
}
