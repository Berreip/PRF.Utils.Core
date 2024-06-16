using System.IO;
using System.IO.Compression;
using PRF.Utils.CoreComponents.Extensions;

namespace PRF.Utils.CoreComponent.UnitTest.Extensions;

public sealed class ZipFileExtensionTests
{
    [Fact]
    public void CreateFileEntryFromString()
    {
        //Arrange
        using (var memoryStream = new MemoryStream())
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            //Act
            archive.CreateFileEntryFromString("foo.txt", "content");
        }

        //Assert

    }

}