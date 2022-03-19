using Xunit;
using QRCoder;
using Shouldly;
using QRCoderTests.Helpers.XUnitExtenstions;
using QRCoderTests.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace QRCoderTests
{
    public class QRCodeRendererTests
    {
        const string QRCodeContent = "This is a quick test! 123#?";
        const string VisualTestPath = null;

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_standard_graphic()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(10) as Image<Rgba32>);
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_standard_graphic), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "c0f8af4256eddc7e566983e539cce389");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_standard_graphic_hex()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(10, "#000000", "#ffffff") as Image<Rgba32>);
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_standard_graphic_hex), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "c0f8af4256eddc7e566983e539cce389");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_standard_graphic_without_quietzones()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(5, Color.Black, Color.White, false) as Image<Rgba32>);
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_standard_graphic_without_quietzones), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "8a2d62fa98c09d764a21466b8d6bb6c8");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_transparent_logo_graphic()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(10, Color.Black, Color.Transparent, icon: HelperFunctions.LoadAssetImage()) as Image<Rgba32>);
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_with_transparent_logo_graphic), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "d19c708b8e2b28c62a6b9db3e630179a");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_non_transparent_logo_graphic()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(10, Color.Black, Color.White, icon: HelperFunctions.LoadAssetImage()) as Image<Rgba32>);
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_with_non_transparent_logo_graphic), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "5e535aac60c1bc7ee8ec506916cd2dd8");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_logo_and_with_transparent_border()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(10, Color.Black, Color.Transparent, iconBorderWidth: 6, icon: HelperFunctions.LoadAssetImage()) as Image<Rgba32>);
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_with_logo_and_with_transparent_border), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "d19c708b8e2b28c62a6b9db3e630179a");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_logo_and_with_standard_border()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(10, Color.Black, Color.White, iconBorderWidth: 6, icon: HelperFunctions.LoadAssetImage()) as Image<Rgba32>);
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_with_logo_and_with_standard_border), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "91f35d10164ccd4a9ad621e2dc81c86b");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_logo_and_with_custom_border()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(10, Color.Black, Color.Transparent, iconBorderWidth: 6, iconBackgroundColor: Color.DarkGreen, icon: HelperFunctions.LoadAssetImage()) as Image<Rgba32>);
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_with_logo_and_with_custom_border), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "ce13cc3372aa477a914c9828cdad4754");
        }


        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_instantate_qrcode_parameterless()
        {
            var svgCode = new QRCode();
            svgCode.ShouldNotBeNull();
            svgCode.ShouldBeOfType<QRCode>();
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_render_qrcode_from_helper()
        {
            var image = QRCodeHelper.GetQRCode(QRCodeContent, 10, Color.Black, Color.White, QRCodeGenerator.ECCLevel.H) as Image<Rgba32>;
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_qrcode_from_helper), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "c0f8af4256eddc7e566983e539cce389");
        }
    }
}
