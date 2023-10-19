﻿using System.IO.Compression;
using System.Text;

// ReSharper disable MemberCanBePrivate.Global

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// .Net archive extension methods
    /// </summary>
    public static class ZipFileExtension
    {
        /// <summary>
        /// Create a file directly within a zip from a character string
        /// </summary>
        /// <param name="zipArchive">the archive concerned</param>
        /// <param name="relativePathInArchive">the relative path of the file (with name and extension)</param>
        /// <param name="fileContent">the contents of the file to add</param>
        /// <param name="compressionLevel">the compression level (optimal by default)</param>
        /// <param name="encoding">file encoding (UTF8 by default)</param>
        public static void CreateFileEntryFromString(this ZipArchive zipArchive,
            string relativePathInArchive,
            string fileContent,
            CompressionLevel compressionLevel = CompressionLevel.Optimal,
            Encoding encoding = null)
        {
            zipArchive.CreateFileEntryFromByteArray(relativePathInArchive,
                encoding?.GetBytes(fileContent) ?? Encoding.UTF8.GetBytes(fileContent),
                compressionLevel);
        }

        /// <summary>
        /// Create a file directly within a zip from a character string
        /// </summary>
        /// <param name="zipArchive">the archive concerned</param>
        /// <param name="relativePathInArchive">the relative path of the file (with name and extension)</param>
        /// <param name="fileContent">the content of the file to add in the form of bytes</param>
        /// <param name="compressionLevel">the compression level (optimal by default)</param>
        public static void CreateFileEntryFromByteArray(this ZipArchive zipArchive,
            string relativePathInArchive,
            byte[] fileContent,
            CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            using (var entry = zipArchive.CreateEntry(relativePathInArchive, compressionLevel).Open())
            {
                entry.Write(fileContent, 0, fileContent.Length);
            }
        }
    }
}