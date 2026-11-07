using System.Collections;
using System.Collections.Generic;

namespace QRCoder.Imaging
{
    /// <summary>
    /// Renders a module matrix into 1-bit PNG scanlines (dark = 0, light = 1).
    /// </summary>
    internal static class ModuleScanlines
    {
        /// <summary>
        /// Returns the pixel width/height of the rendered code.
        /// </summary>
        public static int GetSize(List<BitArray> moduleMatrix, int pixelsPerModule, bool drawQuietZones) =>
            (moduleMatrix.Count - (drawQuietZones ? 0 : 8)) * pixelsPerModule;

        /// <summary>
        /// Creates filtered scanlines, each starting with its filter type byte. The first pixel row of each module row is
        /// stored unfiltered, the repeated rows use the Up filter and therefore consist of zeros only, which compresses very well.
        /// </summary>
        public static byte[] Build1Bit(List<BitArray> moduleMatrix, int pixelsPerModule, bool drawQuietZones)
        {
            var matrixSize = moduleMatrix.Count - (drawQuietZones ? 0 : 8);
            var quietZoneOffset = drawQuietZones ? 0 : 4;
            var bytesPerScanline = (matrixSize * pixelsPerModule + 7) / 8 + 1; // One byte for filter type then one bit per pixel.
            var scanlines = new byte[bytesPerScanline * matrixSize * pixelsPerModule];

            for (var y = 0; y < matrixSize; y++)
            {
                var modules = moduleMatrix[y + quietZoneOffset];
                var scanlineOffset = y * pixelsPerModule * bytesPerScanline;

                // Draw a scanline with the modules from the QR code.
                for (var x = 0; x < matrixSize; x++)
                {
                    if (modules[x + quietZoneOffset])
                        continue;

                    var pixelIndex = x * pixelsPerModule;
                    var endIndex = pixelIndex + pixelsPerModule;
                    for (; pixelIndex < endIndex; pixelIndex++)
                        scanlines[scanlineOffset + 1 + pixelIndex / 8] |= (byte)(0x80 >> (pixelIndex % 8));
                }

                // The following rows of this module are identical: Up filter with all-zero data.
                for (var copyCount = 1; copyCount < pixelsPerModule; copyCount++)
                    scanlines[scanlineOffset + copyCount * bytesPerScanline] = (byte)PngEncoder.FilterType.Up;
            }

            return scanlines;
        }
    }
}
