## About

QRCoder.ImageSharp is a simple library, written in C#.NET, which enables you to create QR codes. 
It was forked from the [QRCoder](https://github.com/codebude/QRCoder) project that is using the System.Drawing library.
Since .Net 6 System.Drawing is only supported for Windows (https://docs.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only).
The original QRCoder project decided to no longer support important functionality for None-Windows environments.

QRCoder.ImageSharp is based on (ImageSharp)(https://github.com/SixLabors/ImageSharp) to support all plattforms.

***

## Differences between QRCoder and QRCoder.ImageSharp

- QRCoder.ImageSharp is using ImageSharp instead of System.Drawing
- QRCoder.ImageSharp is not supporting ArtQRCode
- QRCoder.ImageSharp does not make rounded ractangles when including logos into QRCode
- QRCoder.ImageSharp does not support russian QRCode payloads (we stand with ukraine!)

## Legal information and credits

QRCoder is a project by [Raffael Herrmann](https://raffaelherrmann.de) and was first released in 10/2013.
QRCoder.ImageSharp is a project by [Joerg Plenert](https://plenert.net). It's licensed under the [MIT license](https://github.com/JPlenert/QRCoder.ImageSharp/blob/master/license.txt).