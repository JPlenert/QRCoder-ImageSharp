using System;
using static QRCoder.QRCodeGenerator;

namespace QRCoder
{
    public class Base64QRCode : AbstractQRCode, IDisposable
    {
        private QRCode qr;

        /// <summary>
        /// Constructor without params to be used in COM Objects connections
        /// </summary>
        public Base64QRCode() {
            qr = new QRCode();
        }

        public Base64QRCode(QRCodeData data) : base(data) {
            qr = new QRCode(data);
        }

        public override void SetQRCodeData(QRCodeData data) {
            this.qr.SetQRCodeData(data);
        }

        public string GetGraphic(int pixelsPerModule)
        {
            return this.GetGraphic(pixelsPerModule, QRColor.Black, QRColor.White, true);
        }

        public string GetGraphic(int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex, bool drawQuietZones = true)
        {
            return this.GetGraphic(pixelsPerModule, QRColor.Parse(darkColorHtmlHex), QRColor.Parse(lightColorHtmlHex), drawQuietZones);
        }

        /// <summary>
        /// Returns the QR code as Base64 encoded PNG.
        /// </summary>
        public string GetGraphic(int pixelsPerModule, QRColor darkColor, QRColor lightColor, bool drawQuietZones = true)
        {
            return Convert.ToBase64String(qr.GetGraphic(pixelsPerModule, darkColor, lightColor, drawQuietZones), Base64FormattingOptions.None);
        }

        /// <summary>
        /// Returns the QR code with an icon (PNG file content) as Base64 encoded PNG.
        /// </summary>
        public string GetGraphic(int pixelsPerModule, QRColor darkColor, QRColor lightColor, byte[] icon, int iconSizePercent = 15, int iconBorderWidth = 6, bool drawQuietZones = true)
        {
            return Convert.ToBase64String(qr.GetGraphic(pixelsPerModule, darkColor, lightColor, icon, iconSizePercent, iconBorderWidth, drawQuietZones), Base64FormattingOptions.None);
        }
    }

    public static class Base64QRCodeHelper
    {
        public static string GetQRCode(string plainText, int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex, ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false, EciMode eciMode = EciMode.Default, int requestedVersion = -1, bool drawQuietZones = true)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode, requestedVersion))
            using (var qrCode = new Base64QRCode(qrCodeData))
                return qrCode.GetGraphic(pixelsPerModule, darkColorHtmlHex, lightColorHtmlHex, drawQuietZones);
        }
    }
}
