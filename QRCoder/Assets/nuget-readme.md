## About

QRCoder-Pdf is a simple library, written in C#.NET, which enables you to create QR codes. 
It was forked from the [QRCoder](https://github.com/codebude/QRCoder) project that is using the System.Drawing assembly.

Because of the System.Drawing assembly does not suport [non-Windows](https://docs.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only) plattforms, this assembly has no imaging dependency at all: PNG files are written (and logos read) by a small built-in PNG encoder/decoder.

Formerly this library was knwon as QRCoder-ImageSharp and used the [ImageSharp](https://github.com/SixLabors/ImageSharp) assembly as depencency. 

## Differences between QRCoder and QRCoder-Pdf

- QRCoder-Pdf does not depend on System.Drawing or any other imaging library
- Raster renderers (`QRCode`, `Base64QRCode`) return PNG data (`byte[]` / Base64 string), logos are passed as PNG file content (`byte[]`)
- Colors are given as `QRColor` (e.g. `QRColor.Black`, `QRColor.Parse("#ff0000")`), sizes as `QRSize`
- Only PNG is supported as raster format, the PDF renderer embeds a lossless image
- QRCoder is not supporting ArtQRCode
- QRCoder does not make rounded ractangles when including logos into QRCode
- QRCoder does not support russian QRCode payloads (we stand with ukraine!)

If you need other formats than PNG, use can use the `ModuleScanlines` and create an image with another imaging library.
(Check `QRCode.GetGraphic` for an example)

## Legal information and credits

QRCoder is a project by [Raffael Herrmann](https://raffaelherrmann.de) and was first released in 10/2013.
QRCoder-Pdf is a project by [Joerg Plenert](https://plenert.net). It's licensed under the [MIT license](https://github.com/JPlenert/QRCoder.ImageSharp/blob/master/license.txt).