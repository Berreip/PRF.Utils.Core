using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Par défaut, pour les DirectoryInfo et FileInfo, la propriété Exist renvoie si le fichier ou directory existait au moment de la CREATION de l'instance. 
    /// Ainsi, le Test suivant échoue:
    ///         var file = new FileInfo(@"C:\ProgramData\...");
    ///         Assert.IsFalse(file.Exists); // = FALSE
    ///         file.Create();
    ///         Assert.IsTrue(file.Exists); // FAIL! => FALSE
    /// => Il faut faire un Refresh préalable
    /// </summary>
    public static class FilesAndDirectoryInfoExtension
    {
        /// <summary>
        /// Renvoie true si le dossier ou fichier existe vraiment à l'instant de l'appel
        /// </summary>
        public static bool ExistsExplicit<T>(this T fileSystemInfo) where T : FileSystemInfo
        {
            fileSystemInfo.Refresh();
            return fileSystemInfo.Exists;
        }

        /// <summary>
        /// Fait un delete si le dossier existe (comportement par défaut SAUF si le chemin n'existe pas lui même)
        /// </summary>
        /// <returns>true si on a effacé qqch, false sinon</returns>
        public static bool DeleteIfExist(this DirectoryInfo fileSystemInfo, bool recursive = true)
        {
            if (!fileSystemInfo.ExistsExplicit()) return false;
            fileSystemInfo.Delete(recursive);
            return true;
        }

        /// <summary>
        /// Try to delete the folder and cycle up to the confirmation taht the file is deleted or the timeout is reached (use this methods only in unit tests)
        /// Note: the deletion is NOT sync even if Ms pretends it. so using this in unit test context may be usefull
        /// Default timeout is 5 seconds
        /// </summary>
        public static bool DeleteIfExistAndWaitDeletion(this DirectoryInfo fileSystemInfo, TimeSpan? timeout = null, bool recursive = true)
        {
            if (!timeout.HasValue)
            {
                timeout = TimeSpan.FromSeconds(5);
            }

            if (timeout > TimeSpan.FromHours(1))
                throw new ArgumentException("Timeout cannot exceed 1 hour.");

            if (!fileSystemInfo.ExistsExplicit()) return true;
            fileSystemInfo.Delete(recursive);

            var sw = new Stopwatch();
            sw.Start();
            try
            {
                while (sw.Elapsed <= timeout)
                {
                    try
                    {
                        if (!fileSystemInfo.ExistsExplicit())
                        {
                            return true;
                        }

                        Thread.Sleep(50);
                    }
                    catch
                    {
                        Thread.Sleep(50);
                    }
                }
            }
            finally
            {
                sw.Stop();
            }

            return false;
        }

        /// <summary>
        /// Try to delete the file and cycle up to the confirmation taht the file is deleted or the timeout is reached (use this methods only in unit tests)
        /// Note: the deletion is NOT sync even if Ms pretends it. so using this in unit test context may be usefull
        /// Default timeout is 5 seconds
        /// </summary>
        public static bool DeleteIfExistAndWaitDeletion(this FileInfo fileSystemInfo, TimeSpan? timeout = null)
        {
            if (!timeout.HasValue)
            {
                timeout = TimeSpan.FromSeconds(5);
            }

            if (timeout > TimeSpan.FromHours(1))
                throw new ArgumentException("Timeout cannot exceed 1 hour.");

            if (!fileSystemInfo.ExistsExplicit()) return true;
            fileSystemInfo.Delete();

            var sw = new Stopwatch();
            sw.Start();
            try
            {
                while (sw.Elapsed <= timeout)
                {
                    try
                    {
                        if (!fileSystemInfo.ExistsExplicit())
                        {
                            return true;
                        }

                        Thread.Sleep(50);
                    }
                    catch
                    {
                        Thread.Sleep(50);
                    }
                }
            }
            finally
            {
                sw.Stop();
            }

            return false;
        }


        /// <summary>
        /// Opens a text file, reads all the text in the file into a string, and then closes the file.
        /// </summary>
        public static string ReadAllText(this FileInfo file)
        {
            return File.ReadAllText(file.FullName);
        }

        /// <summary>
        /// Fait une copie d'un directoryInfo en copiant tous les sous fichiers et sous dossiers
        /// </summary>
        /// <returns>le nouveau dossier cible</returns>
        /// <param name="fileSystemInfo">le dossier source</param>
        /// <param name="path">le chemin du dossier cible</param>
        /// <param name="autoRename">si autoRename = true et si le dossier cible existe, on l'utilise. Sinon, on créer un nouveau dossier avec un ...(2)</param>
        /// <returns></returns>
        public static DirectoryInfo CopyTo(this DirectoryInfo fileSystemInfo, string path, bool autoRename = true)
        {
            if (!fileSystemInfo.ExistsExplicit()) return null;

            // génère un nouveau nom si un fichier du mm nom existe déjà (sinon, check juste le séparateur de dossier)
            path = autoRename
                ? GetDirectoryNameAndAvoidDuplicate(path)
                : path.TrimEnd(Path.DirectorySeparatorChar);

            // Créer le dossier cible s'il n'existe pas
            Directory.CreateDirectory(path);

            //Créer tous les dossiers
            foreach (var dirPath in fileSystemInfo.GetDirectories("*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.FullName.Replace(fileSystemInfo.FullName.TrimEnd(Path.DirectorySeparatorChar), path));

            //Puis tous les fichiers
            foreach (var newPath in fileSystemInfo.GetFiles("*", SearchOption.AllDirectories))
                newPath.CopyTo(newPath.FullName.Replace(fileSystemInfo.FullName.TrimEnd(Path.DirectorySeparatorChar), path), false);

            return new DirectoryInfo(path);
        }

        /// <summary>
        /// Fait une copie d'un directoryInfo en copiant tous les sous fichiers et sous dossiers en vérifiant la longueur du chemin cible
        ///  avant la copie (dans ce cas, un fichier d'erreur est écrit à la racine)
        /// </summary>
        /// <returns>le nouveau dossier cible</returns>
        /// <param name="fileSystemInfo">le dossier source</param>
        /// <param name="path">le chemin du dossier cible</param>
        public static DirectoryInfo CopyToWithCheckLenght(this DirectoryInfo fileSystemInfo, string path)
        {
            if (!fileSystemInfo.ExistsExplicit()) return null;

            const string errorcopyTxt = "errorCopy.txt";

            // génère un nouveau nom si un fichier du mm nom existe déjà
            path = GetDirectoryNameAndAvoidDuplicate(path);

            //première vérification de la taille des chemins on s'assure que l'on peut créer le fichier qui trace les erreurs
            var wantedPath = Path.Combine(path, errorcopyTxt);
            if (wantedPath.Length >= PathExtension.MaxPathLenght)
            {
                throw new PathTooLongException($@"the path provided in CopyToWithCheckLenght was too long: 
limit = {PathExtension.MaxPathLenght}, path lenght = {wantedPath.Length}, path = {wantedPath}");
            }

            // Créer le dossier cible s'il n'existe pas
            Directory.CreateDirectory(path);

            //Créer tous les dossiers
            foreach (var dirPath in fileSystemInfo.GetDirectories("*", SearchOption.AllDirectories))
            {
                var newDirectoryName = dirPath.FullName.Replace(fileSystemInfo.FullName.TrimEnd(Path.DirectorySeparatorChar), path);
                if (newDirectoryName.Length >= PathExtension.MaxPathLenght)
                {
                    File.AppendAllText(wantedPath, $"PathTooLong error for directory ({newDirectoryName.Length} char | limit: {PathExtension.MaxPathLenght}): {newDirectoryName}{Environment.NewLine}");
                }
                else
                {
                    Directory.CreateDirectory(newDirectoryName);
                }
            }

            //Puis tous les fichiers
            foreach (var newPath in fileSystemInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                var destFileName = newPath.FullName.TrimEnd(Path.DirectorySeparatorChar).Replace(fileSystemInfo.FullName.TrimEnd(Path.DirectorySeparatorChar), path);
                if (destFileName.Length >= PathExtension.MaxPathLenght)
                {
                    File.AppendAllText(wantedPath, $"PathTooLong error for file ({destFileName.Length} char | limit: {PathExtension.MaxPathLenght}): {destFileName}{Environment.NewLine}");
                }
                else
                {
                    newPath.CopyTo(destFileName, false);
                }
            }

            return new DirectoryInfo(path);
        }

        public static string GetDirectoryNameAndAvoidDuplicate(string path)
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar);

            if (!Directory.Exists(path)) return path;

            // on commence à (2) car c'est plus logique si le premier fichier n'a pas de parenthèse (1).
            var i = 2;
            while (Directory.Exists($@"{path}({i})"))
            {
                i++;
            }

            return $@"{path}({i})";
        }


        /// <summary>
        /// Renomme un dossier automatiquement si un dossier du même nom existe déjà et renvoie ce nouveau nom
        /// => PUIS LE CREER
        /// </summary>
        public static DirectoryInfo AutoRenameDirectoryToAvoidDuplicateWithCreateDirectory(params string[] pathparts)
        {
            var dir = AutoRenameDirectoryToAvoidDuplicate(pathparts);
            dir.Create();
            return dir;
        }

        /// <summary>
        /// Renomme un dossier automatiquement si un dossier du même nom existe déjà et renvoie ce nouveau nom
        /// => ATTENTION, NE LE CREER PAS S'IL N'EXISTE PAS
        /// </summary>
        public static DirectoryInfo AutoRenameDirectoryToAvoidDuplicate(params string[] pathparts)
        {
            var path = Path.Combine(pathparts).TrimEnd(Path.DirectorySeparatorChar);

            if (!Directory.Exists(path)) return new DirectoryInfo(path);

            // on commence à (2) car c'est plus logique si le premier fichier n'a pas de parenthèse (1).
            var i = 2;
            while (Directory.Exists($@"{path}({i})"))
            {
                i++;
            }

            return new DirectoryInfo($@"{path}({i})");
        }

        /// <summary>
        /// renvoie true si le le string est valide pour un DirectoryInfo et si le dossier existe vraiment
        /// </summary>
        /// <returns></returns>
        public static bool IsValidDirectory(string path)
        {
            try
            {
                return !string.IsNullOrEmpty(path) && new DirectoryInfo(path).ExistsExplicit();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Renvoie une nouvelle string expurgée de tous les charactère invalide qu'elle contenait
        /// </summary>
        /// <param name="name">Le nom du fichier ou du dossier à expurger</param>
        /// <returns>la chaine expurgée</returns>
        public static string EscapeInvalidPathFromName(string name)
        {
            return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c.ToString(), ""));
        }

        /// <summary>
        /// Return true if the name contains invalid file name chars
        /// </summary>
        public static bool ContainsInvalidCharFromName(string name)
        {
            return Path.GetInvalidFileNameChars().Any(name.Contains);
        }

        /// <summary>
        /// get a file name with an automatic rename in a 'windows style' with (2), (3) if it already exists.
        /// => WARNING: THIS METHOD DOES NOT CREATE THE FILE
        /// </summary>
        public static string AutoRenameFileToAvoidDuplicate(params string[] pathparts)
        {
            var path = Path.Combine(pathparts).TrimEnd(Path.DirectorySeparatorChar);
            var file = new FileInfo(path);
            if (!file.Exists) return path; // si le fichier n'existe pas, on renvoie directement le nom de ce dernier.


            // on commence à (2) car c'est plus logique si le premier fichier n'a pas de parenthèse (1).
            var i = 2;
            while (File.Exists($@"{file.FullName.Replace(file.Extension, string.Empty)}({i}){file.Extension}"))
            {
                i++;
            }

            return $@"{file.FullName.Replace(file.Extension, string.Empty)}({i}){file.Extension}";
        }

        /// <summary>
        /// Fait un delete si le fichier existe (comportement par défaut SAUF si le chemin n'existe pas lui même)
        /// </summary>
        /// <returns>true si on a effacé qqch, false sinon</returns>
        public static bool DeleteIfExist(this FileInfo fileSystemInfo)
        {
            if (!fileSystemInfo.ExistsExplicit()) return false;
            fileSystemInfo.Delete();
            fileSystemInfo.Refresh();
            return true;
        }

        /// <summary>
        /// Fait un Create si le dossier n'existe pas (comportement par défaut mais avec un renvoie true/false)
        /// </summary>
        /// <returns>true si on a crée qqch, false sinon</returns>
        public static bool CreateIfNotExist(this DirectoryInfo dir)
        {
            if (dir.ExistsExplicit()) return false;
            dir.Create();
            dir.Refresh();
            return true;
        }

        /// <summary>
        /// Create the file and close the stream
        /// </summary>
        /// <returns>true if the file has been created, false otherwise</returns>
        public static bool CreateIfNotExistAndClose(this FileInfo file)
        {
            if (file.ExistsExplicit()) return false;
            file.Create().Close();
            file.Refresh();
            return true;
        }

        /// <summary>
        /// Clean a directory if it exists and create it if it does not
        /// </summary>
        public static DirectoryInfo CleanDirectory(this DirectoryInfo dir)
        {
            if (!dir.ExistsExplicit())
            {
                dir.Create();
            }
            else
            {
                foreach (var f in dir.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    f.IsReadOnly = false;
                    f.Delete();
                }

                foreach (var subDir in dir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    // n'efface que les sous dossier et pas le dossier d'origine
                    subDir.Delete(true);
                }
            }

            dir.Refresh();
            return dir;
        }

        /// <summary>
        /// Clean a directory if it exists and create it if it does not AND IGNORE EVERY ERRORS
        /// </summary>
        public static DirectoryInfo CreateDirectoryAndTryClean(this DirectoryInfo dir)
        {
            if (!dir.ExistsExplicit())
            {
                dir.Create();
            }
            else
            {
                try
                {
                    foreach (var file in dir.EnumerateFiles("*", SearchOption.TopDirectoryOnly).ToList())
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch
                        {
                            // ignore errors
                        }
                    }

                    foreach (var subDir in dir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).ToList())
                    {
                        try
                        {
                            subDir.Delete(true);
                        }
                        catch
                        {
                            // ignore errors
                        }
                    }
                }
                catch
                {
                    // ignore errors
                }
            }

            dir.Refresh();
            return dir;
        }

        /// <summary>
        /// Renvoie true si le fichier a une date de création type "comportement affichage Window" (la plus ancienne entre la date de création et celle de modification)
        /// plus ancienne ou égale au minimum demandé
        /// </summary>
        /// <param name="file">le fichier concerné</param>
        /// <param name="minimumPeriodBeforeTakingFiles">le minimum de temps demandé</param>
        public static bool IsOldEnought(this FileInfo file, TimeSpan minimumPeriodBeforeTakingFiles)
        {
            // on prends la date la plus ancienne entre la date de création et celle de modification == comportement window
            // Attention à bien prendre la date avec les heures/minutes/secondes.
            return DateTime.Now - MathExt.Min(file.CreationTime, file.LastWriteTime) >= minimumPeriodBeforeTakingFiles;
        }

        /// <summary>
        /// Renvoie une estimation de la taille de tous les fichiers d'un dossier (renvoie -1 en cas d'exception)
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="searchOption">indique si l'on inclus les sous dossier dans ce calcul</param>
        /// <returns>Une estimation de la taille de tous les fichiers d'un dossier</returns>
        public static long EstimateSize(this DirectoryInfo dir, SearchOption searchOption)
        {
            try
            {
                return dir.EnumerateFiles("*", searchOption)
                    .Select(o => o.Length)
                    .Aggregate((long)0, (i, size) => i + size);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Créer le sous dossier dont le nom est fourni et le renvoie. S'il existe déjà, on renvoie simplement le DirectoryInfo correspondant
        /// (la méthode CreateSubdirectory renvoyant par défaut une IOException si le dossier existe déjà)
        /// </summary>
        /// <param name="dir">le directoryInfo parent</param>
        /// <param name="name">le nom du sous dossier souhaité</param>
        /// <returns>le sous dossier (existant ou nouvellement crée)</returns>
        public static DirectoryInfo CreateSubdirectoryIfNotExist(this DirectoryInfo dir, string name)
        {
            var subDir = new DirectoryInfo(Path.Combine(dir.FullName, name).TrimEnd(Path.DirectorySeparatorChar));
            return subDir.Exists ? subDir : dir.CreateSubdirectory(name.TrimEnd(Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Créer le fichier dont le nom est fourni dans le dossier parent puis le renvoie.
        ///  S'il existe déjà:
        ///  on renvoie simplement le FileInfo correspondant si autoRename = false
        ///  on trouve un nom disponible, on créer le fichier et on le renvoie si autoRename = true
        /// </summary>
        /// <param name="dir">le directoryInfo parent</param>
        /// <param name="name">le nom du fichier souhaité</param>
        /// <param name="autoRename">on renvoie simplement le FileInfo correspondant si autoRename = false
        ///  on trouve un nom disponible, on créer le fichier et on le renvoie si autoRename = true</param>
        /// <returns>le fichier (existant ou nouvellement crée)</returns>
        public static FileInfo CreateFileIfNotExist(this DirectoryInfo dir, string name, bool autoRename = true)
        {
            var file = autoRename
                ? new FileInfo(AutoRenameFileToAvoidDuplicate(dir.FullName, name))
                : new FileInfo(Path.Combine(dir.FullName, name));
            file.CreateIfNotExistAndClose();
            return file;
        }

        /// <summary>
        /// Récupère un chemin relatif en supprimant la partie 'pathToRemove' mais en conservant les sous-dossiers qui suivent
        /// Marche avec ou sans le '\' à la fin de beforeRelativePath
        /// </summary>
        /// <param name="file">le fichier</param>
        /// <param name="pathToRemove">le chemin à supprimer</param>
        /// <returns>le chemin relatif restant</returns>
        public static string GetRelativePath(this FileInfo file, string pathToRemove)
        {
            // efface le beforeRelativePath avec et sans '\'
            // ex: C:\Toto\tata\tati\file.txt && beforeRelativePath =  C:\Toto\tata ==> tati\file.txt (SANS '\')
            // ex: C:\Toto\tata\tati\file.txt && beforeRelativePath =  C:\Toto\tata\ ==> tati\file.txt (AVEC '\')
            return file.FullName.Replace(pathToRemove + Path.DirectorySeparatorChar, string.Empty).Replace(pathToRemove, string.Empty);
        }

        /// <summary>
        /// Renvoie true si le dossier est vide, c'est à dire ne contient ni fichiers ni sous-dossiers, même vide
        /// </summary>
        /// <param name="dir">le dossier à examiner</param>
        /// <returns>true si le dossier est vide, false sinon</returns>
        public static bool IsEmpty(this DirectoryInfo dir)
        {
            return !Directory.EnumerateFileSystemEntries(dir.FullName).Any();
        }

        /// <summary>
        /// return a fileInfo in the given directory EVEN if the file does not exists
        /// </summary>
        public static FileInfo GetFile(this DirectoryInfo dir, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return new FileInfo(Path.Combine(dir.FullName, name));
        }

        /// <summary>
        /// return true and out the file info if any one is found in the given directory with the given name
        /// </summary>
        public static bool TryGetFile(this DirectoryInfo dir, string name, out FileInfo file)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var fileFound = new FileInfo(Path.Combine(dir.FullName, name));
                if (fileFound.Exists)
                {
                    file = fileFound;
                    return true;
                }
            }
            file = null;
            return false;
        }

        /// <summary>
        /// Return a sub directory EVEN if it does not exists
        /// </summary>
        public static DirectoryInfo GetDirectory(this DirectoryInfo dir, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return new DirectoryInfo(Path.Combine(dir.FullName, name));
        }

        /// <summary>
        /// Try to get the matching sub directory and return true if it exists
        /// </summary>
        public static bool TryGetDirectory(this DirectoryInfo dir, string name, out DirectoryInfo subdirectory)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var subDir = new DirectoryInfo(Path.Combine(dir.FullName, name));
                if (subDir.Exists)
                {
                    subdirectory = subDir;
                    return true;
                }
            }

            subdirectory = null;
            return false;
        }

        /// <summary>
        /// Ajoute la chaine de caractères passée en paramètre à la fin du fichier suivie d'un retour à la ligne (CR+LF).
        /// Si le fichier n'existe pas il est créé.
        /// </summary>
        /// <param name="file">FileInfo du fichier cible</param>
        /// <param name="line">Ligne à ajouter</param>
        public static void AppendTextLine(this FileInfo file, string line)
        {
            File.AppendAllLines(file.FullName, new[] { line });
        }

        /// <summary>
        /// Retire l'attribut ReadOnly du fichier
        /// </summary>
        /// <param name="file">FileInfo du fichier cible</param>
        public static void RemoveReadonlyAttribute(this FileInfo file)
        {
            if (file.IsReadOnly)
            {
                file.Attributes &= ~FileAttributes.ReadOnly;
            }
        }

        /// <summary>
        /// Supprime le fichier s'il n'est pas en cours d'utilisation.
        /// </summary>
        /// <param name="file">Fichier à supprimer</param>
        /// <param name="timeout">Durée pendant laquelle on doit effectuer des retentatives. Si null: pas de retentative.</param>
        /// <returns>True si le fichier a été supprimé avant l'expiration du timeout.</returns>
        public static bool TryDelete(this FileInfo file, TimeSpan? timeout = null)
        {
            if (timeout.HasValue && timeout.Value > TimeSpan.FromHours(1))
                throw new ArgumentException("Timeout cannot exceed 1 hour.");

            if (!file.ExistsExplicit()) return true;

            var sw = new Stopwatch();
            sw.Start();
            try
            {
                while (!timeout.HasValue || sw.Elapsed < timeout.Value)
                {
                    try
                    {
                        file.Delete();
                        return true;
                    }
                    catch
                    {
                        if (!timeout.HasValue)
                        {
                            return false;
                        }

                        Thread.Sleep(100);
                    }
                }
            }
            finally
            {
                sw.Stop();
            }

            return false;
        }

        /// <summary>
        /// Copie de façon asynchrone un fichier. Contrairement à la méthode copy de FileInfo,
        /// cette méthode ne modifie pas le FileInfo mais renvoie le fichier copié en sortie sous forme de FileInfo.
        /// ATTENTION, si le fichier de destination existe, cette méthode écrase le fichier
        /// </summary>
        /// <param name="file">le fichier source à copier</param>
        /// <param name="destinationPath">la destination (full Name)</param>
        /// <returns> le fichier de sortie</returns>
        public static async Task<FileInfo> CopyAsync(this FileInfo file, string destinationPath)
        {
            using (var source = File.OpenRead(file.FullName))
            using (var destination = File.Create(destinationPath))
            {
                await source.CopyToAsync(destination);
                return new FileInfo(destinationPath);
            }
        }

        /// <summary>
        /// Copie de façon asynchrone un fichier. Contrairement à la méthode copy de FileInfo,
        /// cette méthode ne modifie pas le FileInfo mais renvoie le fichier copié en sortie sous forme de FileInfo.
        /// ATTENTION, si le fichier de destination existe, cette méthode écrase le fichier
        /// </summary>
        /// <param name="file">le fichier source à copier</param>
        /// <param name="destinationFile">le fichier de destination</param>
        /// <returns> le fichier de sortie</returns>
        public static async Task<FileInfo> CopyAsync(this FileInfo file, FileInfo destinationFile)
        {
            using (var source = File.OpenRead(file.FullName))
            using (var destination = File.OpenWrite(destinationFile.FullName))
            {
                await source.CopyToAsync(destination);
                // refresh le fichier afin que le .Exist soit à jour
                destinationFile.Refresh();
                return destinationFile;
            }
        }

        /// <summary>
        /// Supprime le fichier avec des retentatives pendant 10s s'il est locké
        /// </summary>
        /// <param name="file"></param>
        public static void DeleteAndRetryIfLocked(this FileInfo file)
        {
            if (!file.ExistsExplicit()) return;

            var sw = new Stopwatch();
            var timeout = TimeSpan.FromSeconds(10);
            sw.Start();

            while (sw.IsRunning)
            {
                try
                {
                    file.Delete();
                    sw.Stop();
                }
                catch (Exception ex)
                {
                    if (sw.Elapsed > timeout)
                    {
                        throw new TimeoutException($"Timeout occurred while trying to delete file: {file.FullName}", ex);
                    }
                }
            }
        }
    }
}