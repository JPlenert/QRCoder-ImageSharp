using Xunit;
using QRCoder;
using Shouldly;
using QRCoderTests.Helpers.XUnitExtenstions;
using QRCoderTests.Helpers;
using SixLabors.ImageSharp;

namespace QRCoderTests
{

    public class SvgQRCodeRendererTests
    {
        const string QRCodeContent = "This is a quick test! 123#?";
        const string VisualTestPath = null;

        [Fact]
        [Category("QRRenderer/SvgQRCode")]
        public void can_render_svg_qrcode_simple()
        {
            var svg = HelperFunctions.GenerateSvg(QRCodeContent, sg => sg.GetGraphic(5));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_svg_qrcode_simple), svg);
            HelperFunctions.TestByHash(svg, "5c251275a435a9aed7e591eb9c2e9949");
        }

        [Fact]
        [Category("QRRenderer/SvgQRCode")]
        public void can_render_svg_qrcode()
        {
            var svg = HelperFunctions.GenerateSvg(QRCodeContent, sg => sg.GetGraphic(10, Color.Red, Color.White));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_svg_qrcode), svg);
            HelperFunctions.TestByHash(svg, "1baa8c6ac3bd8c1eabcd2c5422dd9f78");
        }

        [Fact]
        [Category("QRRenderer/SvgQRCode")]
        public void can_render_svg_qrcode_viewbox_mode()
        {
            var svg = HelperFunctions.GenerateSvg(QRCodeContent, sg => sg.GetGraphic(new Size(128, 128)));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_svg_qrcode_viewbox_mode), svg);
            HelperFunctions.TestByHash(svg, "56719c7db39937c74377855a5dc4af0a");
        }

        [Fact]
        [Category("QRRenderer/SvgQRCode")]
        public void can_render_svg_qrcode_viewbox_mode_viewboxattr()
        {
            var svg = HelperFunctions.GenerateSvg(QRCodeContent, sg => sg.GetGraphic(new Size(128, 128), sizingMode: SvgQRCode.SizingMode.ViewBoxAttribute));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_svg_qrcode_viewbox_mode_viewboxattr), svg);
            HelperFunctions.TestByHash(svg, "788afdb693b0b71eed344e495c180b60");
        }

        [Fact]
        [Category("QRRenderer/SvgQRCode")]
        public void can_render_svg_qrcode_without_quietzones()
        {
            var svg = HelperFunctions.GenerateSvg(QRCodeContent, sg => sg.GetGraphic(10, Color.Red, Color.White, false));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_svg_qrcode_without_quietzones), svg);
            HelperFunctions.TestByHash(svg, "2a582427d86b51504c08ebcbcf0472bd");
        }

        [Fact]
        [Category("QRRenderer/SvgQRCode")]
        public void can_render_svg_qrcode_without_quietzones_hex()
        {
            var svg = HelperFunctions.GenerateSvg(QRCodeContent, sg => sg.GetGraphic(10, "#000000", "#ffffff", false));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_svg_qrcode_without_quietzones_hex), svg);
            HelperFunctions.TestByHash(svg, "4ab0417cc6127e347ca1b2322c49ed7d");
        }

        [Fact]
        [Category("QRRenderer/SvgQRCode")]
        public void can_render_svg_qrcode_with_png_logo()
        {
            var logoBitmap = HelperFunctions.LoadAssetImage();
            var logoObj = new SvgQRCode.SvgLogo(iconRasterized: logoBitmap, 15);
            logoObj.GetMediaType().ShouldBe<SvgQRCode.SvgLogo.MediaType>(SvgQRCode.SvgLogo.MediaType.PNG);

            var svg = HelperFunctions.GenerateSvg(QRCodeContent, sg => sg.GetGraphic(10, Color.DarkGray, Color.White, logo: logoObj));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_svg_qrcode_with_png_logo), svg);
            HelperFunctions.TestByHash(svg, "78e02e8ba415f15817d5ed88c4afca31");           
        }

        [Fact]
        [Category("QRRenderer/SvgQRCode")]
        public void can_render_svg_qrcode_with_svg_logo_embedded()
        {
            var logoSvg = HelperFunctions.LoadAssetSvg();
            var logoObj = new SvgQRCode.SvgLogo(logoSvg, 20);
            logoObj.GetMediaType().ShouldBe<SvgQRCode.SvgLogo.MediaType>(SvgQRCode.SvgLogo.MediaType.SVG);

            var svg = HelperFunctions.GenerateSvg(QRCodeContent, sg => sg.GetGraphic(10, Color.DarkGray, Color.White, logo: logoObj));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_svg_qrcode_with_svg_logo_embedded), svg);
            HelperFunctions.TestByHash(svg, "855eb988d3af035abd273ed1629aa952");
          
        }

        [Fact]
        [Category("QRRenderer/SvgQRCode")]
        public void can_render_svg_qrcode_with_svg_logo_image_tag()
        {
            var logoSvg = HelperFunctions.LoadAssetSvg();
            var logoObj = new SvgQRCode.SvgLogo(logoSvg, 20, iconEmbedded: false);

            var svg = HelperFunctions.GenerateSvg(QRCodeContent, sg => sg.GetGraphic(10, Color.DarkGray, Color.White, logo: logoObj));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_svg_qrcode_with_svg_logo_image_tag), svg);
            HelperFunctions.TestByHash(svg, "bd442ea77d45a41a4f490b8d41591e04");
        }

        [Fact]
        [Category("QRRenderer/SvgQRCode")]
        public void can_instantate_parameterless()
        {
            var svgCode = new SvgQRCode();
            svgCode.ShouldNotBeNull();
            svgCode.ShouldBeOfType<SvgQRCode>();
        }

        [Fact]
        [Category("QRRenderer/SvgQRCode")]
        public void can_render_svg_qrcode_from_helper()
        {
            //Create QR code                   
            var svg = SvgQRCodeHelper.GetQRCode("A", 2, "#000000", "#ffffff", QRCodeGenerator.ECCLevel.Q);
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_svg_qrcode_from_helper), svg);
            HelperFunctions.TestByHash(svg, "f5ec37aa9fb207e3701cc0d86c4a357d");
        }
    }
}



