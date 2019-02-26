using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Méthodes d'extensions pour les Bitmap
    /// </summary>
    public static class BitmapExtensions
    {
        /// <summary>
        /// Convertit l'image en niveaux de gris sans créer de copie.
        /// L'instance originale est donc écrasée !
        /// </summary>
        /// <param name="bmp">Image à convertir</param>
        /// <param name="redPart">Proportion de rouge (en %)</param>
        /// <param name="greenPart">Proportion de vert (en %)</param>
        /// <param name="bluePart">Proportion de bleu (en %)</param>
        public static void ToGrayScale(this Bitmap bmp, int redPart, int greenPart, int bluePart)
        {
            if(redPart + greenPart + bluePart != 100)
                throw new ArgumentException("The sum of red, green and blue percentages must be equal to 100.");

            var redFactor = (float)redPart / 100;
            var greenFactor = (float)greenPart / 100;
            var blueFactor = (float)bluePart / 100;

            var colorMatrix = new ColorMatrix(
                new[]
                {
                    new[] {redFactor, redFactor, redFactor, 0, 0},
                    new[] {greenFactor, greenFactor, greenFactor, 0, 0},
                    new[] {blueFactor, blueFactor, blueFactor, 0, 0},
                    new[] {0f, 0f, 0f, 1f, 0f},
                    new[] {0f, 0f, 0f, 0f, 1f}
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
        /// Convertit l'image en niveaux de gris sans créer de copie.
        /// L'instance originale est donc écrasée !
        /// Répartition des couleurs : R=20%, V=60%, B=20%
        /// </summary>
        public static void ToGrayScale(this Bitmap bmp)
        {
            bmp.ToGrayScale(20, 60, 20);
        }

        /// <summary>
        /// Retourne une copie de l'image en niveaux de gris.
        /// L'instance originale n'est pas modifiée.
        /// </summary>
        /// <param name="bmp">Image à convertir</param>
        /// <param name="redPart">Proportion de rouge (en %)</param>
        /// <param name="greenPart">Proportion de vert (en %)</param>
        /// <param name="bluePart">Proportion de bleu (en %)</param>
        public static Bitmap ToGrayScaleCopy(this Bitmap bmp, int redPart, int greenPart, int bluePart)
        {
            var copy = new Bitmap(bmp);

            copy.ToGrayScale(redPart, greenPart, bluePart);

            return copy;
        }

        /// <summary>
        /// Retourne une copie de l'image en niveaux de gris.
        /// L'instance originale n'est pas modifiée.
        /// Répartition des couleurs : R=20%, V=60%, B=20%
        /// </summary>
        public static Bitmap ToGrayScaleCopy(this Bitmap bmp)
        {
            var copy = new Bitmap(bmp);

            copy.ToGrayScale();

            return copy;
        }
    }
}
