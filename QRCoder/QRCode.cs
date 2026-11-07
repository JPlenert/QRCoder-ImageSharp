using QRCoder.Imaging;
using System;
using static QRCoder.QRCodeGenerator;

namespace QRCoder
{
    /// <summary>
    /// Renders QR codes as PNG images, optionally with an icon/logo in the center.
    /// </summary>
    public class QRCode : AbstractQRCode, IDisposable
    {
        /// <summary>
        /// Constructor without params to be used in COM Objects connections
        /// </summary>
        public QRCode() { }

        public QRCode(QRCodeData data) : base(data) { }

        /// <summary>
        /// Renders a black & white QR code as PNG.
        /// </summary>
        public byte[] GetGraphic(int pixelsPerModule)
        {
            return this.GetGraphic(pixelsPerModule, QRColor.Black, QRColor.White, true);
        }

        /// <summary>
        /// Renders a QR code as PNG with colors given in hex format (e.g. #000000).
        /// </summary>
        public byte[] GetGraphic(int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex, bool drawQuietZones = true)
        {
            return this.GetGraphic(pixelsPerModule, QRColor.Parse(darkColorHtmlHex), QRColor.Parse(lightColorHtmlHex), drawQuietZones);
        }

        /// <summary>
        /// Renders a QR code as 2-color PNG.
        /// </summary>
        public byte[] GetGraphic(int pixelsPerModule, QRColor darkColor, QRColor lightColor, bool drawQuietZones = true)
        {
            var size = ModuleScanlines.GetSize(this.QrCodeData.ModuleMatrix, pixelsPerModule, drawQuietZones);
            var scanlines = ModuleScanlines.Build1Bit(this.QrCodeData.ModuleMatrix, pixelsPerModule, drawQuietZones);
            return PngEncoder.EncodeIndexed1Bit(size, size, scanlines, darkColor, lightColor);
        }

        /// <summary>
        /// Renders a QR code as PNG with an icon in its center.
        /// </summary>
        /// <param name="pixelsPerModule">Amount of px each dark/light module of the QR code shall take place in the final QR code image</param>
        /// <param name="darkColor">Color of the dark modules</param>
        /// <param name="lightColor">Color of the light modules</param>
        /// <param name="icon">Icon as PNG file content. If null, a plain QR code is rendered.</param>
        /// <param name="iconSizePercent">Width of the icon relative to the QR code (1-100)</param>
        /// <param name="iconBorderWidth">Width of the border around the icon in px</param>
        /// <param name="drawQuietZones">If true a border of light modules is drawn around the QR code</param>
        /// <param name="iconBackgroundColor">Color of the icon border. Defaults to the light color.</param>
        public byte[] GetGraphic(int pixelsPerModule, QRColor darkColor, QRColor lightColor, byte[] icon, int iconSizePercent = 15, int iconBorderWidth = 0, bool drawQuietZones = true, QRColor? iconBackgroundColor = null)
        {
            if (icon == null || iconSizePercent <= 0 || iconSizePercent > 100)
                return GetGraphic(pixelsPerModule, darkColor, lightColor, drawQuietZones);

            var iconImage = PngDecoder.Decode(icon);
            var img = DrawQRCode(pixelsPerModule, drawQuietZones, darkColor, lightColor);

            float iconDestWidth = iconSizePercent * img.Width / 100f;
            float iconDestHeight = iconDestWidth * iconImage.Height / iconImage.Width;
            float iconX = (img.Width - iconDestWidth) / 2;
            float iconY = (img.Height - iconDestHeight) / 2;

            if (iconBorderWidth > 0)
            {
                var background = iconBackgroundColor ?? lightColor;
                if (background != QRColor.Transparent)
                {
                    // Bounds are inclusive, matching the former rendering
                    img.FillRect((int)(iconX - iconBorderWidth), (int)(iconY - iconBorderWidth),
                        (int)(iconX + iconDestWidth + iconBorderWidth) + 1, (int)(iconY + iconDestHeight + iconBorderWidth) + 1, background);
                }
            }

            if ((int)iconDestWidth > 0 && (int)iconDestHeight > 0)
                img.DrawOver(iconImage.Resize((int)iconDestWidth, (int)iconDestHeight), (int)iconX, (int)iconY);

            return PngEncoder.EncodeRgba(img);
        }

        private RgbaImage DrawQRCode(int pixelsPerModule, bool drawQuietZones, QRColor darkColor, QRColor lightColor)
        {
            var moduleMatrix = this.QrCodeData.ModuleMatrix;
            var moduleOffset = drawQuietZones ? 0 : 4;
            var size = ModuleScanlines.GetSize(moduleMatrix, pixelsPerModule, drawQuietZones);
            var image = new RgbaImage(size, size);
            var stride = size * 4;
            var dark = darkColor.ToRgbaBytes();
            var light = lightColor.ToRgbaBytes();

            for (var modY = moduleOffset; modY < moduleMatrix.Count - moduleOffset; modY++)
            {
                // Generate the first pixel row for this module row
                var rowStart = (modY - moduleOffset) * pixelsPerModule * stride;
                for (var modX = moduleOffset; modX < moduleMatrix.Count - moduleOffset; modX++)
                {
                    var color = moduleMatrix[modY][modX] ? dark : light;
                    var pixel = rowStart + (modX - moduleOffset) * pixelsPerModule * 4;
                    for (var idx = 0; idx < pixelsPerModule; idx++, pixel += 4)
                        Buffer.BlockCopy(color, 0, image.Pixels, pixel, 4);
                }

                // Copy the prepared row to the remaining rows of this module
                for (var idx = 1; idx < pixelsPerModule; idx++)
                    Buffer.BlockCopy(image.Pixels, rowStart, image.Pixels, rowStart + idx * stride, stride);
            }

            return image;
        }
    }

    public static class QRCodeHelper
    {
        public static byte[] GetQRCode(string plainText, int pixelsPerModule, QRColor darkColor, QRColor lightColor, ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false, EciMode eciMode = EciMode.Default, int requestedVersion = -1, byte[] icon = null, int iconSizePercent = 15, int iconBorderWidth = 0, bool drawQuietZones = true)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode, requestedVersion))
            using (var qrCode = new QRCode(qrCodeData))
                return qrCode.GetGraphic(pixelsPerModule, darkColor, lightColor, icon, iconSizePercent, iconBorderWidth, drawQuietZones);
        }
    }
}
