using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using static QRCoder.QRCodeGenerator;

namespace QRCoder
{
    public class QRCode : AbstractQRCode, IDisposable
    {
        /// <summary>
        /// Constructor without params to be used in COM Objects connections
        /// </summary>
        public QRCode() { }

        public QRCode(QRCodeData data) : base(data) { }

        public Image GetGraphic(int pixelsPerModule)
        {
            return this.GetGraphic(pixelsPerModule, Color.Black, Color.White, true);
        }

        public Image GetGraphic(int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex, bool drawQuietZones = true)
        {
            return this.GetGraphic(pixelsPerModule, Color.Parse(darkColorHtmlHex), Color.Parse(lightColorHtmlHex), drawQuietZones);
        }

        public Image GetGraphic(int pixelsPerModule, Color darkColor, Color lightColor, bool drawQuietZones = true)
        {
            int moduleOffset = drawQuietZones ? 0 : 4;
            int size = (this.QrCodeData.ModuleMatrix.Count - moduleOffset * 2) * pixelsPerModule;

            var image = new Image<Rgba32>(size, size);
            DrawQRCode(image, pixelsPerModule, moduleOffset, darkColor, lightColor);

            return image;
        }

        public Image GetGraphic(int pixelsPerModule, Color darkColor, Color lightColor, Image icon = null, int iconSizePercent = 15, int iconBorderWidth = 0, bool drawQuietZones = true, Color? iconBackgroundColor = null)
        {
            Image<Rgba32> img = GetGraphic(pixelsPerModule, darkColor, lightColor, drawQuietZones) as Image<Rgba32>;
            if (icon != null && iconSizePercent > 0 && iconSizePercent <= 100)
            {
                float iconDestWidth = iconSizePercent * img.Width / 100f;
                float iconDestHeight = iconDestWidth * icon.Height / icon.Width;
                float iconX = (img.Width - iconDestWidth) / 2;
                float iconY = (img.Height - iconDestHeight) / 2;
                var centerDest = new RectangleF(iconX - iconBorderWidth, iconY - iconBorderWidth, iconDestWidth + iconBorderWidth * 2, iconDestHeight + iconBorderWidth * 2);
                var iconDestRect = new RectangleF(iconX, iconY, iconDestWidth, iconDestHeight);

                if (iconBorderWidth > 0)
                {
                    if (!iconBackgroundColor.HasValue)
                        iconBackgroundColor = lightColor;
                    if (iconBackgroundColor != Color.Transparent)
                    {
                        img.ProcessPixelRows(accessor =>
                        {
                            for (int y = (int)centerDest.Top; y <= (int)centerDest.Bottom; y++)
                            {
                                Span<Rgba32> pixelRow = accessor.GetRowSpan(y);

                                for (int x = (int)centerDest.Left; x <= (int)centerDest.Right; x++)
                                {
                                    pixelRow[x] = iconBackgroundColor ?? lightColor;
                                }
                            }
                        });
                    }
                }

                var sizedIcon = icon.Clone(x => x.Resize((int)iconDestWidth, (int)iconDestHeight));
                img.Mutate(x => x.DrawImage(sizedIcon, new Point((int)iconDestRect.X, (int)iconDestRect.Y), 1));
            }
            /*


            var size = (this.QrCodeData.ModuleMatrix.Count - (drawQuietZones ? 0 : 8)) * pixelsPerModule;
            var offset = drawQuietZones ? 0 : 4 * pixelsPerModule;

            var image = new Image<Rgba32>(size, size);

            using (var gfx = Graphics.FromImage(bmp))
            using (var lightBrush = new SolidBrush(lightColor))
            using (var darkBrush = new SolidBrush(darkColor))
            {
                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gfx.CompositingQuality = CompositingQuality.HighQuality;
                gfx.Clear(lightColor);
                var drawIconFlag = icon != null && iconSizePercent > 0 && iconSizePercent <= 100;
                               
                for (var x = 0; x < size + offset; x = x + pixelsPerModule)
                {
                    for (var y = 0; y < size + offset; y = y + pixelsPerModule)
                    {
                        var moduleBrush = this.QrCodeData.ModuleMatrix[(y + pixelsPerModule) / pixelsPerModule - 1][(x + pixelsPerModule) / pixelsPerModule - 1] ? darkBrush : lightBrush;
                        gfx.FillRectangle(moduleBrush , new Rectangle(x - offset, y - offset, pixelsPerModule, pixelsPerModule));
                    }
                }

                if (drawIconFlag)
                {
                    float iconDestWidth = iconSizePercent * bmp.Width / 100f;
                    float iconDestHeight = drawIconFlag ? iconDestWidth * icon.Height / icon.Width : 0;
                    float iconX = (bmp.Width - iconDestWidth) / 2;
                    float iconY = (bmp.Height - iconDestHeight) / 2;
                    var centerDest = new RectangleF(iconX - iconBorderWidth, iconY - iconBorderWidth, iconDestWidth + iconBorderWidth * 2, iconDestHeight + iconBorderWidth * 2);
                    var iconDestRect = new RectangleF(iconX, iconY, iconDestWidth, iconDestHeight);
                    var iconBgBrush = iconBackgroundColor != null ? new SolidBrush((Color)iconBackgroundColor) : lightBrush;
                    //Only render icon/logo background, if iconBorderWith is set > 0
                    if (iconBorderWidth > 0)
                    {                        
                        using (GraphicsPath iconPath = CreateRoundedRectanglePath(centerDest, iconBorderWidth * 2))
                        {                            
                            gfx.FillPath(iconBgBrush, iconPath);
                        }
                    }
                    gfx.DrawImage(icon, iconDestRect, new RectangleF(0, 0, icon.Width, icon.Height), GraphicsUnit.Pixel);
                }

                gfx.Save();
            }
            */
            return img;
        }

        private void DrawQRCode(Image<Rgba32> image, int pixelsPerModule, int moduleOffset, Color darkColor, Color lightColor)
        {
            Rgba32[] row = new Rgba32[image.Width];

            image.ProcessPixelRows(accessor =>
            {
                for (var modY = moduleOffset; modY < QrCodeData.ModuleMatrix.Count - moduleOffset; modY++)
                {
                    // Generate row for this y-Module
                    for (var modX = moduleOffset; modX < QrCodeData.ModuleMatrix.Count - moduleOffset; modX++)
                    {
                        for (var idx = 0; idx < pixelsPerModule; idx++)
                            row[(modX - moduleOffset) * pixelsPerModule + idx] = this.QrCodeData.ModuleMatrix[modY][modX] ? darkColor : lightColor;
                    }

                    // Copy the prepared row to the image
                    for (var idx = 0; idx < pixelsPerModule; idx++)
                    {
                        Span<Rgba32> pixelRow = accessor.GetRowSpan((modY - moduleOffset) * pixelsPerModule + idx);
                        row.CopyTo(pixelRow);
                    }
                }
            });
        }
        /*
        internal GraphicsPath CreateRoundedRectanglePath(RectangleF rect, int cornerRadius)
        {
            var roundedRect = new GraphicsPath();
            roundedRect.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
            roundedRect.AddLine(rect.X + cornerRadius, rect.Y, rect.Right - cornerRadius * 2, rect.Y);
            roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
            roundedRect.AddLine(rect.Right, rect.Y + cornerRadius * 2, rect.Right, rect.Y + rect.Height - cornerRadius * 2);
            roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
            roundedRect.AddLine(rect.Right - cornerRadius * 2, rect.Bottom, rect.X + cornerRadius * 2, rect.Bottom);
            roundedRect.AddArc(rect.X, rect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
            roundedRect.AddLine(rect.X, rect.Bottom - cornerRadius * 2, rect.X, rect.Y + cornerRadius * 2);
            roundedRect.CloseFigure();
            return roundedRect;
        }
        */
    }

    public static class QRCodeHelper
    {
        public static Image GetQRCode(string plainText, int pixelsPerModule, Color darkColor, Color lightColor, ECCLevel eccLevel, bool forceUtf8 = false, bool utf8BOM = false, EciMode eciMode = EciMode.Default, int requestedVersion = -1, Image icon = null, int iconSizePercent = 15, int iconBorderWidth = 0, bool drawQuietZones = true)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(plainText, eccLevel, forceUtf8, utf8BOM, eciMode, requestedVersion))
            using (var qrCode = new QRCode(qrCodeData))
                return qrCode.GetGraphic(pixelsPerModule, darkColor, lightColor, icon, iconSizePercent, iconBorderWidth, drawQuietZones);
        }
    }
}