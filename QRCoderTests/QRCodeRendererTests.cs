using Xunit;
using QRCoder;
using Shouldly;
using QRCoderTests.Helpers.XUnitExtenstions;
using QRCoderTests.Helpers;

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
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(10));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_standard_graphic), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "aabd78913d146331c4a74921af587d2a");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_standard_graphic_hex()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(10, "#000000", "#ffffff"));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_standard_graphic_hex), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "aabd78913d146331c4a74921af587d2a");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_standard_graphic_without_quietzones()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(5, QRColor.Black, QRColor.White, false));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_standard_graphic_without_quietzones), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "60b07f40d4419608e7304075d579fa96");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_transparent_logo_graphic()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(10, QRColor.Black, QRColor.Transparent, icon: HelperFunctions.LoadAssetImage()));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_with_transparent_logo_graphic), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "7bb219d764f01359f0cad2613afef8d3");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_non_transparent_logo_graphic()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(10, QRColor.Black, QRColor.White, icon: HelperFunctions.LoadAssetImage()));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_with_non_transparent_logo_graphic), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "aba29cbbcc93312087b4cc1fa9587ecc");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_logo_and_with_transparent_border()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(10, QRColor.Black, QRColor.Transparent, iconBorderWidth: 6, icon: HelperFunctions.LoadAssetImage()));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_with_logo_and_with_transparent_border), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "7bb219d764f01359f0cad2613afef8d3");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_logo_and_with_standard_border()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(10, QRColor.Black, QRColor.White, iconBorderWidth: 6, icon: HelperFunctions.LoadAssetImage()));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_with_logo_and_with_standard_border), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "eee1cc2cc70aadec6597d78154f24a4b");
        }

        [Fact]
        [Category("QRRenderer/QRCode")]
        public void can_create_qrcode_with_logo_and_with_custom_border()
        {
            var image = HelperFunctions.GenerateImage(QRCodeContent, (qr) => qr.GetGraphic(10, QRColor.Black, QRColor.Transparent, iconBorderWidth: 6, iconBackgroundColor: QRColor.DarkGreen, icon: HelperFunctions.LoadAssetImage()));
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_create_qrcode_with_logo_and_with_custom_border), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "df3ab1809a0a7ffb520881d233c7f434");
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
            var image = QRCodeHelper.GetQRCode(QRCodeContent, 10, QRColor.Black, QRColor.White, QRCodeGenerator.ECCLevel.H);
            HelperFunctions.TestImageToFile(VisualTestPath, nameof(can_render_qrcode_from_helper), image);
            HelperFunctions.TestByDecode(image, QRCodeContent);
            HelperFunctions.TestByHash(image, "aabd78913d146331c4a74921af587d2a");
        }
    }
}
