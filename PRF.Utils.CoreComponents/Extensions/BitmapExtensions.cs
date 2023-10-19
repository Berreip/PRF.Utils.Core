using System;
using System.Drawing;
using System.Drawing.Imaging;

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Extension methods for Bitmaps
    /// </summary>
    public static class BitmapExtensions
    {
        /// <summary>
        /// Converts the image to grayscale without creating a copy.
        /// The original instance is therefore overwritten!
        /// </summary>
        /// <param name="bmp">Image to convert</param>
        /// <param name="redPart">Proportion of red (in %)</param>
        /// <param name="greenPart">Proportion of green (in %)</param>
        /// <param name="bluePart">Proportion of blue (in %)</param>
        public static void ToGrayScale(this Bitmap bmp, int redPart, int greenPart, int bluePart)
        {
            if (redPart + greenPart + bluePart != 100)
                throw new ArgumentException("The sum of red, green and blue percentages must be equal to 100.");

            var redFactor = (float)redPart / 100;
            var greenFactor = (float)greenPart / 100;
            var blueFactor = (float)bluePart / 100;

            var colorMatrix = new ColorMatrix(
                new[]
                {
                    new[] { redFactor, redFactor, redFactor, 0, 0 },
                    new[] { greenFactor, greenFactor, greenFactor, 0, 0 },
                    new[] { blueFactor, blueFactor, blueFactor, 0, 0 },
                    new[] { 0f, 0f, 0f, 1f, 0f },
                    new[] { 0f, 0f, 0f, 0f, 1f },
                });

            using (var imageAttributes = new ImageAttributes())
            {
                using (var graphics = Graphics.FromImage(bmp))
                {
                    imageAttributes.SetColorMatrix(colorMatrix);
                    graphics.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }
        }

        /// <summary>
        /// Converts the image to grayscale without creating a copy.
        /// The original instance is therefore overwritten!
        /// Color distribution: R=20%, G=60%, B=20%
        /// </summary>
        public static void ToGrayScale(this Bitmap bmp)
        {
            bmp.ToGrayScale(20, 60, 20);
        }

        /// <summary>
        /// Returns a copy of the grayscale image.
        /// The original instance is not modified.
        /// </summary>
        /// <param name="bmp">Image to convert</param>
        /// <param name="redPart">Proportion of red (in %)</param>
        /// <param name="greenPart">Proportion of green (in %)</param>
        /// <param name="bluePart">Proportion of blue (in %)</param>
        public static Bitmap ToGrayScaleCopy(this Bitmap bmp, int redPart, int greenPart, int bluePart)
        {
            var copy = new Bitmap(bmp);

            copy.ToGrayScale(redPart, greenPart, bluePart);

            return copy;
        }

        /// <summary>
        /// Returns a copy of the grayscale image.
        /// The original instance is not modified.
        /// Color distribution: R=20%, G=60%, B=20%
        /// </summary>
        public static Bitmap ToGrayScaleCopy(this Bitmap bmp)
        {
            var copy = new Bitmap(bmp);

            copy.ToGrayScale();

            return copy;
        }
    }
}