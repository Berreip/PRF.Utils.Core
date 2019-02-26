using System.IO.Compression;
using System.Text;

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Méthodes d'extensions des archives .Net
    /// </summary>
    public static class ZipFileExtension
    {
        /// <summary>
        /// Créer un fichier directement au sein d'un zip à partir d'une chaîne de caractères
        /// </summary>
        /// <param name="zipArchive">l'archive concernée</param>
        /// <param name="relativePathInArchive">le chemin relatif du fichier (avec le nom et l'extension)</param>
        /// <param name="fileContent">le contenu du fichier à ajouter</param>
        /// <param name="compressionLevel">le niveau de compression (optimal par défaut)</param>
        /// <param name="encoding">l'encodage du fichier (UTF8 par défaut)</param>
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
        /// Créer un fichier directement au sein d'un zip à partir d'une chaîne de caractères
        /// </summary>
        /// <param name="zipArchive">l'archive concernée</param>
        /// <param name="relativePathInArchive">le chemin relatif du fichier (avec le nom et l'extension)</param>
        /// <param name="fileContent">le contenu du fichier à ajouter sous forme de bytes</param>
        /// <param name="compressionLevel">le niveau de compression (optimal par défaut)</param>
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
