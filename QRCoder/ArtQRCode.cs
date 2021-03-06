using SixLabors.ImageSharp;
using System;
using static QRCoder.ArtQRCode;
using static QRCoder.QRCodeGenerator;

// pull request raised to extend library used. 
namespace QRCoder
{
    public class ArtQRCode : AbstractQRCode, IDisposable
    {
        /// <summary>
        /// Constructor without params to be used in COM Objects connections
        /// </summary>
        public ArtQRCode() => throw new NotImplementedException();

        /// <summary>
        /// Creates new ArtQrCode object
        /// </summary>
        /// <param name="data">QRCodeData generated by the QRCodeGenerator</param>
        public ArtQRCode(QRCodeData data) : base(data) { }

        /// <summary>
        /// Renders an art-style QR code with dots as modules. (With default settings: DarkColor=Black, LightColor=White, Background=Transparent, QuietZone=true)
        /// </summary>
        /// <param name="pixelsPerModule">Amount of px each dark/light module of the QR code shall take place in the final QR code image</param>
        /// <returns>QRCode graphic as bitmap</returns>
        public Image GetGraphic(int pixelsPerModule) => throw new NotImplementedException();

        /// <summary>
        /// Renders an art-style QR code with dots as modules and a background image (With default settings: DarkColor=Black, LightColor=White, Background=Transparent, QuietZone=true)
        /// </summary>
        /// <param name="backgroundImage">A bitmap object that will be used as background picture</param>
        /// <returns>QRCode graphic as bitmap</returns>
        public Image GetGraphic(Image backgroundImage = null) => throw new NotImplementedException();

        /// <summary>
        /// Renders an art-style QR code with dots as modules and various user settings
        /// </summary>
        /// <param name="pixelsPerModule">Amount of px each dark/light module of the QR code shall take place in the final QR code image</param>
        /// <param name="darkColor">Color of the dark modules</param>
        /// <param name="lightColor">Color of the light modules</param>
        /// <param name="backgroundColor">Color of the background</param>
        /// <param name="backgroundImage">A bitmap object that will be used as background picture</param>
        /// <param name="pixelSizeFactor">Value between 0.0 to 1.0 that defines how big the module dots are. The bigger the value, the less round the dots will be.</param>
        /// <param name="drawQuietZones">If true a white border is drawn around the whole QR Code</param>
        /// <param name="quietZoneRenderingStyle">Style of the quiet zones</param>
        /// <param name="backgroundImageStyle">Style of the background image (if set). Fill=spanning complete graphic; DataAreaOnly=Don't paint background into quietzone</param>
        /// <param name="finderPatternImage">Optional image that should be used instead of the default finder patterns</param>
        /// <returns>QRCode graphic as bitmap</returns>
        public Image GetGraphic(int pixelsPerModule, Color darkColor, Color lightColor, Color backgroundColor, Image backgroundImage = null, double pixelSizeFactor = 0.8, 
                                 bool drawQuietZones = true, QuietZoneStyle quietZoneRenderingStyle = QuietZoneStyle.Dotted, 
                                 BackgroundImageStyle backgroundImageStyle = BackgroundImageStyle.DataAreaOnly, Image finderPatternImage = null) => throw new NotImplementedException();


        /// <summary>
        /// Defines how the quiet zones shall be rendered.
        /// </summary>
        public enum QuietZoneStyle
        {
            Dotted,
            Flat
        }

        /// <summary>
        /// Defines how the background image (if set) shall be rendered.
        /// </summary>
        public enum BackgroundImageStyle
        {
            Fill,
            DataAreaOnly
        }
    }

    public static class ArtQRCodeHelper
    {
        /// <summary>
        /// Helper function to create an ArtQRCode graphic with a single function call
        /// </summary>
        /// <param name="plainText">Text/payload to be encoded inside the QR code</param>
        /// <param name="pixelsPerModule">Amount of px each dark/light module of the QR code shall take place in the final QR code image</param>
        /// <param name="darkColor">Color of the dark modules</param>
        /// <param name="lightColor">Color of the light modules</param>
        /// <param name="backgroundColor">Color of the background</param>
        /// <param name="eccLevel">The level of error correction data</param>
        /// <param name="forceUtf8">Shall the generator be forced to work in UTF-8 mode?</param>
        /// <param name="utf8BOM">Should the byte-order-mark be used?</param>
        /// <param name="eciMode">Which ECI mode shall be used?</param>
        /// <param name="requestedVersion">Set fixed QR code target version.</param>
        /// <param name="backgroundImage">A bitmap object that will be used as background picture</param>
        /// <param name="pixelSizeFactor">Value between 0.0 to 1.0 that defines how big the module dots are. The bigger the value, the less round the dots will be.</param>
        /// <param name="drawQuietZones">If true a white border is drawn around the whole QR Code</param>
        /// <param name="quietZoneRenderingStyle">Style of the quiet zones</param>
        /// <param name="backgroundImageStyle">Style of the background image (if set). Fill=spanning complete graphic; DataAreaOnly=Don't paint background into quietzone</param>
        /// <param name="finderPatternImage">Optional image that should be used instead of the default finder patterns</param>
        /// <returns>QRCode graphic as bitmap</returns>
        public static Image GetQRCode(string plainText, int pixelsPerModule, Color darkColor, Color lightColor, Color backgroundColor, ECCLevel eccLevel, bool forceUtf8 = false, 
                                       bool utf8BOM = false, EciMode eciMode = EciMode.Default, int requestedVersion = -1, Image backgroundImage = null, double pixelSizeFactor = 0.8,
                                       bool drawQuietZones = true, QuietZoneStyle quietZoneRenderingStyle = QuietZoneStyle.Flat, 
                                       BackgroundImageStyle backgroundImageStyle = BackgroundImageStyle.DataAreaOnly, Image finderPatternImage = null) => throw new NotImplementedException();
    }
}
