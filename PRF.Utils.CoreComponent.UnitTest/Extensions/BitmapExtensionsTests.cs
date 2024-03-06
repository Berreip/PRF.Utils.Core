using System;
using System.Drawing;
using NUnit.Framework;
using PRF.Utils.CoreComponents.Extensions;

namespace PRF.Utils.CoreComponent.UnitTest.Extensions;

#pragma warning disable CA1416
[TestFixture]
internal sealed class BitmapExtensionsTests
{
    private static Bitmap GetBitmapTest()
    {
        // Create a sample bitmap (e.g., 2x2 pixels)
        var sut = new Bitmap(2, 2);
        sut.SetPixel(0, 0, Color.FromArgb(255, 100, 50, 75));
        sut.SetPixel(1, 0, Color.FromArgb(255, 150, 75, 100));
        sut.SetPixel(0, 1, Color.FromArgb(255, 200, 100, 125));
        sut.SetPixel(1, 1, Color.FromArgb(255, 250, 125, 150));
        return sut;
    }

    [Test]
    public void ToGrayScale_ShouldConvertBitmapToGrayscale()
    {
        if (!UnitTestHelpers.IsWindows)
        {
            Assert.Pass("Only works on windows due to bitmap: see https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only ");
        }

        // Arrange
        const int redPart = 30;
        const int greenPart = 59;
        const int bluePart = 11;

        using (var sut = GetBitmapTest())
        {
            // Act
            sut.ToGrayScale(redPart, greenPart, bluePart);

            // Assert
            for (var x = 0; x < sut.Width; x++)
            {
                for (var y = 0; y < sut.Height; y++)
                {
                    var pixel = sut.GetPixel(x, y);
                    var expectedGrayValue = (redPart * pixel.R + greenPart * pixel.G + bluePart * pixel.B) / 100;
                    Assert.AreEqual(expectedGrayValue, pixel.R);
                    Assert.AreEqual(expectedGrayValue, pixel.G);
                    Assert.AreEqual(expectedGrayValue, pixel.B);
                }
            }
        }
    }

    [Test]
    public void ToGrayScale_WhenInvalidColorParts_ShouldThrowArgumentException()
    {
        if (!UnitTestHelpers.IsWindows)
        {
            Assert.Pass("Only works on windows due to bitmap: see https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only ");
        }

        using (var sut = GetBitmapTest())
        {
            // Arrange
            const int invalidRedPart = 10;
            const int invalidGreenPart = 20;
            const int invalidBluePart = 80;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => sut.ToGrayScale(invalidRedPart, invalidGreenPart, invalidBluePart));
        }
    }

    [Test]
    public void ToGrayScale_ShouldConvertBitmapToGrayscale_with_default_values()
    {
        if (!UnitTestHelpers.IsWindows)
        {
            Assert.Pass("Only works on windows due to bitmap: see https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only ");
        }

        using (var sut = GetBitmapTest())
        {
            // Arrange
            const int redPart = 20;
            const int greenPart = 60;
            const int bluePart = 20;

            // Act
            sut.ToGrayScale();

            // Assert
            for (var x = 0; x < sut.Width; x++)
            {
                for (var y = 0; y < sut.Height; y++)
                {
                    var pixel = sut.GetPixel(x, y);
                    var expectedGrayValue = (redPart * pixel.R + greenPart * pixel.G + bluePart * pixel.B) / 100;
                    Assert.AreEqual(expectedGrayValue, pixel.R);
                    Assert.AreEqual(expectedGrayValue, pixel.G);
                    Assert.AreEqual(expectedGrayValue, pixel.B);
                }
            }
        }
    }

    [Test]
    public void ToGrayScaleCopy_ShouldReturnCopyOfGrayscaleImage()
    {
        if (!UnitTestHelpers.IsWindows)
        {
            Assert.Pass("Only works on windows due to bitmap: see https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only ");
        }

        using (var sut = GetBitmapTest())
        {
            // Arrange
            const int redPart = 20;
            const int greenPart = 60;
            const int bluePart = 20;

            // Act
            var copy = sut.ToGrayScaleCopy(redPart, greenPart, bluePart);

            // Assert
            Assert.AreNotSame(sut, copy);
            Assert.AreEqual(sut.Width, copy.Width);
            Assert.AreEqual(sut.Height, copy.Height);
        }
    }

    [Test]
    public void ToGrayScaleCopy_WithoutParameters_ShouldReturnCopyWithDefaultColorDistribution()
    {
        if (!UnitTestHelpers.IsWindows)
        {
            Assert.Pass("Only works on windows due to bitmap: see https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only ");
        }

        using (var sut = GetBitmapTest())
        {
            // Act
            var copy = sut.ToGrayScaleCopy();

            // Assert
            Assert.AreNotSame(sut, copy);
            Assert.AreEqual(sut.Width, copy.Width);
            Assert.AreEqual(sut.Height, copy.Height);
        }
    }
}
#pragma warning restore CA1416