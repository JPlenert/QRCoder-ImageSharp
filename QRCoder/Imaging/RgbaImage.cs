using System;

namespace QRCoder.Imaging
{
    /// <summary>
    /// Minimal 8-bit RGBA raster (non-premultiplied, row-major) used for compositing icons onto QR codes.
    /// </summary>
    internal sealed class RgbaImage
    {
        public int Width { get; }
        public int Height { get; }
        public byte[] Pixels { get; }

        public RgbaImage(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Image dimensions must be positive.");
            Width = width;
            Height = height;
            Pixels = new byte[checked(width * height * 4)];
        }

        public QRColor GetPixel(int x, int y)
        {
            var i = (y * Width + x) * 4;
            return new QRColor(Pixels[i], Pixels[i + 1], Pixels[i + 2], Pixels[i + 3]);
        }

        public void SetPixel(int x, int y, QRColor color)
        {
            var i = (y * Width + x) * 4;
            Pixels[i] = color.R;
            Pixels[i + 1] = color.G;
            Pixels[i + 2] = color.B;
            Pixels[i + 3] = color.A;
        }

        /// <summary>
        /// Fills the rectangle [x0, x1) x [y0, y1), clipped to the image bounds.
        /// </summary>
        public void FillRect(int x0, int y0, int x1, int y1, QRColor color)
        {
            x0 = Math.Max(0, x0);
            y0 = Math.Max(0, y0);
            x1 = Math.Min(Width, x1);
            y1 = Math.Min(Height, y1);
            for (var y = y0; y < y1; y++)
                for (var x = x0; x < x1; x++)
                    SetPixel(x, y, color);
        }

        /// <summary>
        /// Draws the source image at the given position using source-over alpha compositing, clipped to the image bounds.
        /// </summary>
        public void DrawOver(RgbaImage source, int left, int top)
        {
            for (var sy = 0; sy < source.Height; sy++)
            {
                var dy = top + sy;
                if (dy < 0 || dy >= Height)
                    continue;
                for (var sx = 0; sx < source.Width; sx++)
                {
                    var dx = left + sx;
                    if (dx < 0 || dx >= Width)
                        continue;

                    var si = (sy * source.Width + sx) * 4;
                    var di = (dy * Width + dx) * 4;
                    var sa = source.Pixels[si + 3];
                    if (sa == 0)
                        continue;
                    if (sa == 255)
                    {
                        Buffer.BlockCopy(source.Pixels, si, Pixels, di, 4);
                        continue;
                    }

                    var srcA = sa / 255f;
                    var dstA = Pixels[di + 3] / 255f * (1 - srcA);
                    var outA = srcA + dstA;
                    for (var c = 0; c < 3; c++)
                        Pixels[di + c] = ToByte((source.Pixels[si + c] * srcA + Pixels[di + c] * dstA) / outA);
                    Pixels[di + 3] = ToByte(outA * 255f);
                }
            }
        }

        /// <summary>
        /// Returns a resampled copy using a separable triangle (bilinear) filter. When downscaling, the filter is widened
        /// to the scale factor so all source pixels contribute (area-like averaging instead of aliasing).
        /// Interpolation is done on premultiplied alpha to avoid dark fringes around transparent areas.
        /// </summary>
        public RgbaImage Resize(int width, int height)
        {
            if (width == Width && height == Height)
            {
                var copy = new RgbaImage(width, height);
                Buffer.BlockCopy(Pixels, 0, copy.Pixels, 0, Pixels.Length);
                return copy;
            }

            var xWeights = ComputeWeights(Width, width);
            var yWeights = ComputeWeights(Height, height);

            // Premultiply source
            var src = new float[Width * Height * 4];
            for (var i = 0; i < Pixels.Length; i += 4)
            {
                var a = Pixels[i + 3] / 255f;
                src[i] = Pixels[i] * a;
                src[i + 1] = Pixels[i + 1] * a;
                src[i + 2] = Pixels[i + 2] * a;
                src[i + 3] = Pixels[i + 3];
            }

            // Horizontal pass: Height x width
            var tmp = new float[Height * width * 4];
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var w = xWeights[x];
                    var o = (y * width + x) * 4;
                    for (var k = 0; k < w.Weights.Length; k++)
                    {
                        var si = (y * Width + w.Start + k) * 4;
                        var f = w.Weights[k];
                        tmp[o] += src[si] * f;
                        tmp[o + 1] += src[si + 1] * f;
                        tmp[o + 2] += src[si + 2] * f;
                        tmp[o + 3] += src[si + 3] * f;
                    }
                }
            }

            // Vertical pass: height x width, then unpremultiply
            var result = new RgbaImage(width, height);
            for (var y = 0; y < height; y++)
            {
                var w = yWeights[y];
                for (var x = 0; x < width; x++)
                {
                    float r = 0, g = 0, b = 0, a = 0;
                    for (var k = 0; k < w.Weights.Length; k++)
                    {
                        var ti = ((w.Start + k) * width + x) * 4;
                        var f = w.Weights[k];
                        r += tmp[ti] * f;
                        g += tmp[ti + 1] * f;
                        b += tmp[ti + 2] * f;
                        a += tmp[ti + 3] * f;
                    }

                    var o = (y * width + x) * 4;
                    if (a <= 0.5f)
                        continue; // stays fully transparent
                    var unpremultiply = 255f / a;
                    result.Pixels[o] = ToByte(r * unpremultiply);
                    result.Pixels[o + 1] = ToByte(g * unpremultiply);
                    result.Pixels[o + 2] = ToByte(b * unpremultiply);
                    result.Pixels[o + 3] = ToByte(a);
                }
            }

            return result;
        }

        private struct PixelWeights
        {
            public int Start;
            public float[] Weights;
        }

        private static PixelWeights[] ComputeWeights(int sourceSize, int targetSize)
        {
            var scale = (double)sourceSize / targetSize;
            var support = Math.Max(1.0, scale);
            var result = new PixelWeights[targetSize];

            for (var i = 0; i < targetSize; i++)
            {
                var center = (i + 0.5) * scale - 0.5;
                var start = Math.Max(0, (int)Math.Floor(center - support) + 1);
                var end = Math.Min(sourceSize - 1, (int)Math.Ceiling(center + support) - 1);
                if (end < start)
                    start = end = Math.Min(sourceSize - 1, Math.Max(0, (int)Math.Round(center)));

                var weights = new float[end - start + 1];
                double sum = 0;
                for (var k = start; k <= end; k++)
                {
                    var w = Math.Max(0, 1 - Math.Abs(k - center) / support);
                    weights[k - start] = (float)w;
                    sum += w;
                }

                if (sum <= 0)
                {
                    // Can only happen at degenerate edges; fall back to nearest neighbour.
                    weights = new[] { 1f };
                    start = Math.Min(sourceSize - 1, Math.Max(0, (int)Math.Round(center)));
                }
                else
                {
                    for (var k = 0; k < weights.Length; k++)
                        weights[k] = (float)(weights[k] / sum);
                }

                result[i] = new PixelWeights { Start = start, Weights = weights };
            }

            return result;
        }

        private static byte ToByte(float value)
        {
            if (value <= 0)
                return 0;
            if (value >= 255)
                return 255;
            return (byte)(value + 0.5f);
        }
    }
}
