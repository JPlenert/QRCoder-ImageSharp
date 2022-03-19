using Xunit;
using QRCoder;
using Shouldly;
using QRCoderTests.Helpers.XUnitExtenstions;
using QRCoderTests.Helpers;

namespace QRCoderTests
{
    /****************************************************************************************************
     * Note: Test cases compare the outcome visually even if it's slower than a byte-wise compare.
     *       This is necessary, because the Deflate implementation differs on the different target
     *       platforms and thus the outcome, even if visually identical, differs. Thus only a visual
     *       test method makes sense. In addition bytewise differences shouldn't be important, if the
     *       visual outcome is identical and thus the qr code is identical/scannable.
     ****************************************************************************************************/
    public class PngByteQRCodeRendererTests
    {
        const string QRCodeContent = "This is a quick test! 123#?";
        const string VisualTestPath = null;

        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_blackwhite()
        {
            var pngCodeGfx = HelperFunctions.GeneratePng(QRCodeContent, pr => pr.GetGraphic(5));
            HelperFunctions.TestByHash(pngCodeGfx, "90869fd365fe75e8aef3da40765dd5cc");
            HelperFunctions.TestByDecode(pngCodeGfx, QRCodeContent);
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_pngbyte_qrcode_blackwhite), pngCodeGfx);
        }

        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_color()
        {
            var pngCodeGfx = HelperFunctions.GeneratePng(QRCodeContent, pr => pr.GetGraphic(5, new byte[] { 255, 0, 0 }, new byte[] { 0, 0, 255 }));
            HelperFunctions.TestByHash(pngCodeGfx, "55093e9b9e39dc8368721cb535844425");
            // HelperFunctions.TestByDecode(pngCodeGfx, QRCodeContent); => Not decodable
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_pngbyte_qrcode_color), pngCodeGfx);
        }


        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_color_with_alpha()
        {
            var pngCodeGfx = HelperFunctions.GeneratePng(QRCodeContent, pr => pr.GetGraphic(5, new byte[] { 255, 255, 255, 127 }, new byte[] { 0, 0, 255 }));
            HelperFunctions.TestByHash(pngCodeGfx, "afc7674cb4849860cbf73684970e5332");
            // HelperFunctions.TestByDecode(pngCodeGfx, QRCodeContent); => Not decodable
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_pngbyte_qrcode_color_with_alpha), pngCodeGfx);
        }

        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_color_without_quietzones()
        {
            var pngCodeGfx = HelperFunctions.GeneratePng(QRCodeContent, pr => pr.GetGraphic(5, new byte[] { 255, 255, 255, 127 }, new byte[] { 0, 0, 255 }, false));
            HelperFunctions.TestByHash(pngCodeGfx, "af60811deaa524e0d165baecdf40ab72");
            // HelperFunctions.TestByDecode(pngCodeGfx, QRCodeContent); => not decodable
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_pngbyte_qrcode_color_without_quietzones), pngCodeGfx);
        }

        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_instantate_pngbyte_qrcode_parameterless()
        {
            var pngCode = new PngByteQRCode();
            pngCode.ShouldNotBeNull();
            pngCode.ShouldBeOfType<PngByteQRCode>();
        }

        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_from_helper()
        {
            //Create QR code                   
            var pngCodeGfx = PngByteQRCodeHelper.GetQRCode(QRCodeContent, QRCodeGenerator.ECCLevel.L, 10);
            HelperFunctions.TestByHash(pngCodeGfx, "e649d6a485873ac18b5aab791f325284");
            HelperFunctions.TestByDecode(pngCodeGfx, QRCodeContent);
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_pngbyte_qrcode_from_helper), pngCodeGfx);
        }

        [Fact]
        [Category("QRRenderer/PngByteQRCode")]
        public void can_render_pngbyte_qrcode_from_helper_2()
        {
            //Create QR code                   
            var pngCodeGfx = PngByteQRCodeHelper.GetQRCode("This is a quick test! 123#?", 5, new byte[] { 255, 255, 255, 127 }, new byte[] { 0, 0, 255 }, QRCodeGenerator.ECCLevel.L);
            HelperFunctions.TestByHash(pngCodeGfx, "afc7674cb4849860cbf73684970e5332");
            // HelperFunctions.TestByDecode(pngCodeGfx, QRCodeContent); => not decodable
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_pngbyte_qrcode_from_helper_2), pngCodeGfx);
        }
    }
}
