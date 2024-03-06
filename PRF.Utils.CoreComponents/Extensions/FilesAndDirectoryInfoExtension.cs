using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable OutParameterValueIsAlwaysDiscarded.Global

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// By default, for DirectoryInfo and FileInfo, the Exist property returns whether the file or directory existed at the time the instance was CREATED.
    /// So the following Test fails:
    ///     var file = new FileInfo(@"C:\ProgramData\...");
    ///     Assert.IsFalse(file.Exists); // = FALSE
    ///     file.Create();
    ///     Assert.IsTrue(file.Exists); // FAIL! => FALSE
    /// => You must do a prior Refresh
    /// </summary>
    public static class FilesAndDirectoryInfoExtension
    {
        /// <summary>
        /// Returns true if the folder or file really exists at the time of the call
        /// </summary>
        public static bool ExistsExplicit<T>(this T fileSystemInfo) where T : FileSystemInfo
        {
            fileSystemInfo.Refresh();
            return fileSystemInfo.Exists;
        }

        /// <summary>
        /// Delete if the folder exists (default behavior UNLESS the path itself does not exist)
        /// </summary>
        /// <returns>true if something has been deleted, false otherwise</returns>
        public static bool DeleteIfExist(this DirectoryInfo fileSystemInfo, bool recursive = true)
        {
            if (!fileSystemInfo.ExistsExplicit()) return false;
            fileSystemInfo.Delete(recursive);
            return true;
        }

        /// <summary>
        /// Try to delete the folder and cycle up to the confirmation that the file is deleted or the timeout is reached (use this methods only in unit tests)
        /// Note: the deletion is NOT sync even if Ms pretends it. so using this in unit test context may be useful
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
        /// Try to delete the file and cycle up to the confirmation that the file is deleted or the timeout is reached (use this methods only in unit tests)
        /// Note: the deletion is NOT sync even if Ms pretends it. so using this in unit test context may be useful
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
        /// Makes a copy of a directoryInfo by copying all sub-files and subfolders
        /// </summary>
        /// <returns>the new target folder</returns>
        /// <param name="fileSystemInfo">the source folder</param>
        /// <param name="path">the path of the target folder</param>
        /// <param name="autoRename">if autoRename = true and if the target folder exists, we use it. Otherwise, we create a new folder with a ...(2)</param>
        /// <returns></returns>
        public static DirectoryInfo CopyTo(this DirectoryInfo fileSystemInfo, string path, bool autoRename = true)
        {
            if (!fileSystemInfo.ExistsExplicit()) return null;

            // generates a new name if a file with the same name already exists (otherwise, just check the folder separator)
            path = autoRename
                ? GetDirectoryNameAndAvoidDuplicate(path)
                : path.TrimEnd(Path.DirectorySeparatorChar);

            //Create the target folder if it does not exist
            Directory.CreateDirectory(path);

            //Create all folders
            foreach (var dirPath in fileSystemInfo.GetDirectories("*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.FullName.Replace(fileSystemInfo.FullName.TrimEnd(Path.DirectorySeparatorChar), path));

            // Then all the files
            foreach (var newPath in fileSystemInfo.GetFiles("*", SearchOption.AllDirectories))
                newPath.CopyTo(newPath.FullName.Replace(fileSystemInfo.FullName.TrimEnd(Path.DirectorySeparatorChar), path), false);

            return new DirectoryInfo(path);
        }

        /// <summary>
        /// Makes a copy of a directoryInfo by copying all sub-files and subfolders checking the length of the target path
        /// before copying (in this case an error file is written to the root)
        /// </summary>
        /// <returns>the new target folder</returns>
        /// <param name="fileSystemInfo">the source folder</param>
        /// <param name="path">the path of the target folder</param>
        public static DirectoryInfo CopyToWithCheckLenght(this DirectoryInfo fileSystemInfo, string path)
        {
            if (!fileSystemInfo.ExistsExplicit()) return null;

            const string errorCopyTxt = "errorCopy.txt";

            // generates a new name if a file with the same name already exists
            path = GetDirectoryNameAndAvoidDuplicate(path);

            //first check of the size of the paths we ensure that we can create the file which traces the errors
            var wantedPath = Path.Combine(path, errorCopyTxt);
            if (wantedPath.Length >= PathExtension.MaxPathLenght)
            {
                throw new PathTooLongException($@"the path provided in CopyToWithCheckLenght was too long: 
limit = {PathExtension.MaxPathLenght}, path lenght = {wantedPath.Length}, path = {wantedPath}");
            }

            // Create the target folder if it does not exist
            Directory.CreateDirectory(path);

            //Create all folders
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

            //Then all files
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

        /// <summary>
        /// Returns a directory full path that avoid duplicate from the given original full path. If it exists, add (2), (3) and so on.
        /// </summary>
        public static string GetDirectoryNameAndAvoidDuplicate(string path)
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar);

            if (!Directory.Exists(path)) return path;

            // we start at (2) because it makes more sense if the first file does not have a parenthesis (1).
            var i = 2;
            while (Directory.Exists($"{path}({i})"))
            {
                i++;
            }

            return $"{path}({i})";
        }


        /// <summary>
        /// Renames a folder automatically if a folder with the same name already exists and returns this new name
        /// => THEN CREATE IT
        /// </summary>
        public static DirectoryInfo AutoRenameDirectoryToAvoidDuplicateWithCreateDirectory(params string[] pathParts)
        {
            var dir = AutoRenameDirectoryToAvoidDuplicate(pathParts);
            dir.Create();
            return dir;
        }

        /// <summary>
        /// Renames a folder automatically if a folder with the same name already exists and returns this new name
        /// => WARNING, DO NOT CREATE IT IF IT DOES NOT EXIST
        /// </summary>
        public static DirectoryInfo AutoRenameDirectoryToAvoidDuplicate(params string[] pathParts)
        {
            var path = Path.Combine(pathParts).TrimEnd(Path.DirectorySeparatorChar);

            if (!Directory.Exists(path)) return new DirectoryInfo(path);

            // we start at (2) because it is more logical if the first file does not have a parenthesis (1).
            var i = 2;
            while (Directory.Exists($"{path}({i})"))
            {
                i++;
            }

            return new DirectoryInfo($"{path}({i})");
        }

        /// <summary>
        /// Returns true if the string is valid for a DirectoryInfo and if the folder really exists
        /// </summary>
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
        /// Returns a new string stripped of all invalid characters it contained
        /// </summary>
        /// <param name="name">The name of the file or folder to be redacted</param>
        /// <returns>the string redacted</returns>
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
        public static string AutoRenameFileToAvoidDuplicate(params string[] pathParts)
        {
            var path = Path.Combine(pathParts).TrimEnd(Path.DirectorySeparatorChar);
            var file = new FileInfo(path);
            if (!file.Exists) return path;// if the file does not exist, we return the name of the file directly.


            // we start at (2) because it is more logical if the first file does not have a parenthesis (1).
            var i = 2;
            while (File.Exists($"{file.FullName.Replace(file.Extension, string.Empty)}({i}){file.Extension}"))
            {
                i++;
            }

            return $"{file.FullName.Replace(file.Extension, string.Empty)}({i}){file.Extension}";
        }

        /// <summary>
        /// Delete if the file exists (default behavior UNLESS the path itself does not exist)
        /// </summary>
        /// <returns>true if something has been deleted, false otherwise</returns>
        public static bool DeleteIfExist(this FileInfo fileSystemInfo)
        {
            if (!fileSystemInfo.ExistsExplicit()) return false;
            fileSystemInfo.Delete();
            fileSystemInfo.Refresh();
            return true;
        }

        /// <summary>
        /// Do a Create if the folder does not exist (default behavior but with a true/false return)
        /// </summary>
        /// <returns>true if we created something, false otherwise</returns>
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
                    // only delete subfolders and not the original folder
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
        /// Returns true if the file has a creation date such as "Window display behavior" (the oldest between the creation date and the modification date)
        /// older or equal to the minimum requested
        /// </summary>
        /// <param name="file">the file concerned</param>
        /// <param name="minimumPeriodBeforeTakingFiles">the minimum time requested</param>
        public static bool IsOldEnough(this FileInfo file, TimeSpan minimumPeriodBeforeTakingFiles)
        {
            // we take the oldest date between the creation date and the modification date == window behavior
            // Be careful to take the date correctly with the hours/minutes/seconds.
            return DateTime.Now - MathExt.Min(file.CreationTime, file.LastWriteTime) >= minimumPeriodBeforeTakingFiles;
        }

        /// <summary>
        /// Returns an estimate of the size of all files in a folder (returns -1 in case of exception)
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="searchOption">indicates whether subfolders are included in this calculation</param>
        /// <returns>An estimate of the size of all files in a folder</returns>
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
        /// Create the subfolder whose name is provided and return it. If it already exists, we simply return the corresponding DirectoryInfo
        /// (the CreateSubdirectory method throwing an IOException by default if the folder already exists)
        /// </summary>
        /// <param name="dir">the parent directoryInfo</param>
        /// <param name="name">the name of the desired subfolder</param>
        /// <returns>the subfolder (existing or newly created)</returns>
        public static DirectoryInfo CreateSubdirectoryIfNotExist(this DirectoryInfo dir, string name)
        {
            var subDir = new DirectoryInfo(Path.Combine(dir.FullName, name).TrimEnd(Path.DirectorySeparatorChar));
            return subDir.Exists ? subDir : dir.CreateSubdirectory(name.TrimEnd(Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Create the file whose name is provided in the parent folder then return it.
        /// If it already exists:
        /// we simply return the corresponding FileInfo if autoRename = false
        /// we find an available name, we create the file and we return it if autoRename = true
        /// </summary>
        /// <param name="dir">the parent directoryInfo</param>
        /// <param name="name">the name of the desired file</param>
        /// <param name="autoRename">we simply return the corresponding FileInfo if autoRename = false
        /// we find an available name, we create the file and we return it if autoRename = true</param>
        /// <returns>the file (existing or newly created)</returns>
        public static FileInfo CreateFileIfNotExist(this DirectoryInfo dir, string name, bool autoRename = true)
        {
            var file = autoRename
                ? new FileInfo(AutoRenameFileToAvoidDuplicate(dir.FullName, name))
                : new FileInfo(Path.Combine(dir.FullName, name));
            file.CreateIfNotExistAndClose();
            return file;
        }

        /// <summary>
        /// Get a relative path by removing the 'pathToRemove' part but keeping the following subfolders
        /// Works with or without the '\' at the end of beforeRelativePath
        /// </summary>
        /// <param name="file">the file</param>
        /// <param name="pathToRemove">the path to remove</param>
        /// <returns>the remaining relative path</returns>
        public static string GetRelativePath(this FileInfo file, string pathToRemove)
        {
            // clear the beforeRelativePath with and without '\'
            // ex: C:\Toto\tata\tati\file.txt && beforeRelativePath =  C:\Toto\tata ==> tati\file.txt (WITH '\')
            // ex: C:\Toto\tata\tati\file.txt && beforeRelativePath =  C:\Toto\tata\ ==> tati\file.txt (WITHOUT '\')
            return file.FullName.Replace(pathToRemove + Path.DirectorySeparatorChar, string.Empty).Replace(pathToRemove, string.Empty);
        }
        /// <summary>
        /// Returns true if the folder is empty, i.e. contains no files or subfolders, even empty
        /// </summary>
        /// <param name="dir">the folder to examine</param>
        /// <returns>true if the folder is empty, false otherwise</returns>
        public static bool IsEmpty(this DirectoryInfo dir)
        {
            return !Directory.EnumerateFileSystemEntries(dir.FullName).Any();
        }

        /// <summary>
        /// return a fileInfo in the given directory EVEN if the file does not exists
        /// </summary>
        public static FileInfo GetFile(this DirectoryInfo dir, string name)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
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
            // ReSharper disable once ConvertIfStatementToReturnStatement
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
        /// Adds the character string passed as a parameter at the end of the file followed by a newline (CR+LF).
        /// If the file does not exist it is created.
        /// </summary>
        /// <param name="file">FileInfo of target file</param>
        /// <param name="line">Line to add</param>
        public static void AppendTextLine(this FileInfo file, string line)
        {
            File.AppendAllLines(file.FullName, new[] { line });
        }
        
        /// <summary>
        /// Remove the ReadOnly attribute from the file
        /// </summary>
        /// <param name="file">FileInfo of target file</param>
        public static void RemoveReadonlyAttribute(this FileInfo file)
        {
            if (file.IsReadOnly)
            {
                file.Attributes &= ~FileAttributes.ReadOnly;
            }
        }

        /// <summary>
        /// Delete the file if it is not in use.
        /// </summary>
        /// <param name="file">File to delete</param>
        /// <param name="timeout">Duration during which retries must be made. If null: no retry.</param>
        /// <returns>True if the file was deleted before the timeout expired.</returns>
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
        /// Asynchronously copies a file. Unlike FileInfo's copy method,
        /// this method does not modify the FileInfo but returns the copied file as output as FileInfo.
        /// WARNING, if the destination file exists, this method overwrites the file
        /// </summary>
        /// <param name="file">the source file to copy</param>
        /// <param name="destinationPath">the destination (full name)</param>
        /// <returns> the output file</returns>
        public static async Task<FileInfo> CopyAsync(this FileInfo file, string destinationPath)
        {
            using (var source = File.OpenRead(file.FullName))
            using (var destination = File.Create(destinationPath))
            {
                await source.CopyToAsync(destination).ConfigureAwait(false);
                return new FileInfo(destinationPath);
            }
        }

        /// <summary>
        /// Asynchronously copies a file. Unlike FileInfo's copy method,
        /// this method does not modify the FileInfo but returns the copied file as output as FileInfo.
        /// WARNING, if the destination file exists, this method overwrites the file
        /// </summary>
        /// <param name="file">the source file to copy</param>
        /// <param name="destinationFile">the destination (full name)</param>
        /// <returns> the output file</returns>
        public static async Task<FileInfo> CopyAsync(this FileInfo file, FileInfo destinationFile)
        {
            using (var source = File.OpenRead(file.FullName))
            using (var destination = File.OpenWrite(destinationFile.FullName))
            {
                await source.CopyToAsync(destination).ConfigureAwait(false);
                // refresh the file so that the .Exist is up to date
                destinationFile.Refresh();
                return destinationFile;
            }
        }

        /// <summary>
        /// Delete the file with retries for 10s if it is locked
        /// </summary>
        public static void DeleteAndRetryIfLocked(this FileInfo file, TimeSpan? timeout = null)
        {
            if (!file.ExistsExplicit()) return;

            var sw = new Stopwatch();
            var timeoutComputed = timeout ?? TimeSpan.FromSeconds(10);
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
                    if (sw.Elapsed > timeoutComputed)
                    {
                        throw new TimeoutException($"Timeout occurred while trying to delete file: {file.FullName}", ex);
                    }
                }
            }
        }
    }
}