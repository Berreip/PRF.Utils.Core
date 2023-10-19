using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PRF.Utils.CoreComponents.Extensions;

namespace PRF.Utils.CoreComponents.IO
{
    /// <summary>
    /// Wrapper around a FileInfo to allow unit testing more easily
    /// </summary>
    public interface IDirectoryInfo : IFileSystemInfo
    {
        /// <summary>
        /// Delete if exists (same as default behaviour Except if the path does not exists)
        /// </summary>
        /// <returns>true if deleted</returns>
        bool DeleteIfExist(bool recursive = true);

        /// <summary>Gets the parent directory of a specified subdirectory.</summary>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <returns>The parent directory, or <see langword="null" /> if the path is null or if the file path denotes a root (such as \, C:\, or \\server\share).</returns>
        IDirectoryInfo Parent { get; }

        /// <summary>Gets the root portion of the directory.</summary>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <returns>An object that represents the root of the directory.</returns>
        IDirectoryInfo Root { get; }

        /// <summary>Creates a directory.</summary>
        /// <exception cref="T:System.IO.IOException">The directory cannot be created.</exception>
        IDirectoryInfo Create();

        /// <summary>Creates a directory.</summary>
        /// <exception cref="T:System.IO.IOException">The directory cannot be created.</exception>
        bool CreateIfNotExists();

        /// <summary>Creates a subdirectory or subdirectories on the specified path. The specified path can be relative to this instance of the <see cref="T:System.IO.DirectoryInfo" /> class.</summary>
        /// <param name="path">The specified path. This cannot be a different disk volume or Universal Naming Convention (UNC) name.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="path" /> does not specify a valid file path or contains invalid <see langword="DirectoryInfo" /> characters.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="path" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.IO.IOException">The subdirectory cannot be created.
        /// 
        /// -or-
        /// 
        /// A file already has the name specified by <paramref name="path" />.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have code access permission to create the directory.
        /// 
        /// -or-
        /// 
        /// The caller does not have code access permission to read the directory described by the returned <see cref="T:System.IO.DirectoryInfo" /> object.  This can occur when the <paramref name="path" /> parameter describes an existing directory.</exception>
        /// <exception cref="T:System.NotSupportedException">
        /// <paramref name="path" /> contains a colon character (:) that is not part of a drive label ("C:\").</exception>
        /// <returns>The last directory specified in <paramref name="path" />.</returns>
        IDirectoryInfo CreateSubdirectory(string path);

        /// <summary>Creates a subdirectory or subdirectories on the specified path. The specified path can be relative to this instance of the <see cref="T:System.IO.DirectoryInfo" /> class.</summary>
        /// <param name="path">The specified path. This cannot be a different disk volume or Universal Naming Convention (UNC) name.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="path" /> does not specify a valid file path or contains invalid <see langword="DirectoryInfo" /> characters.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="path" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.IO.IOException">The subdirectory cannot be created.
        /// 
        /// -or-
        /// 
        /// A file already has the name specified by <paramref name="path" />.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have code access permission to create the directory.
        /// 
        /// -or-
        /// 
        /// The caller does not have code access permission to read the directory described by the returned <see cref="T:System.IO.DirectoryInfo" /> object.  This can occur when the <paramref name="path" /> parameter describes an existing directory.</exception>
        /// <exception cref="T:System.NotSupportedException">
        /// <paramref name="path" /> contains a colon character (:) that is not part of a drive label ("C:\").</exception>
        /// <returns>The last directory specified in <paramref name="path" />.</returns>
        IDirectoryInfo CreateSubdirectoryIfNotExists(string path);

        /// <summary>Deletes this instance of a <see cref="T:System.IO.DirectoryInfo" />, specifying whether to delete subdirectories and files.</summary>
        /// <param name="recursive">
        /// <see langword="true" /> to delete this directory, its subdirectories, and all files; otherwise, <see langword="false" />.</param>
        /// <exception cref="T:System.UnauthorizedAccessException">The directory contains a read-only file.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The directory described by this <see cref="T:System.IO.DirectoryInfo" /> object does not exist or could not be found.</exception>
        /// <exception cref="T:System.IO.IOException">The directory is read-only.
        /// 
        /// -or-
        /// 
        /// The directory contains one or more files or subdirectories and <paramref name="recursive" /> is <see langword="false" />.
        /// 
        /// -or-
        /// 
        /// The directory is the application's current working directory.
        /// 
        /// -or-
        /// 
        /// There is an open handle on the directory or on one of its files, and the operating system is Windows XP or earlier. This open handle can result from enumerating directories and files. For more information, see How to: Enumerate Directories and Files.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        void Delete(bool recursive = true);


        /// <summary>Returns an enumerable collection of directory information in the current directory.</summary>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path encapsulated in the <see cref="T:System.IO.DirectoryInfo" /> object is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <returns>An enumerable collection of directories in the current directory.</returns>
        IEnumerable<IDirectoryInfo> EnumerateDirectories();

        /// <summary>Returns an enumerable collection of directory information that matches a specified search pattern.</summary>
        /// <param name="searchPattern">The search string to match against the names of directories.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path encapsulated in the <see cref="T:System.IO.DirectoryInfo" /> object is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <returns>An enumerable collection of directories that matches <paramref name="searchPattern" />.</returns>
        IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern);

        /// <summary>Returns an enumerable collection of directory information that matches a specified search pattern and search subdirectory option.</summary>
        /// <param name="searchPattern">The search string to match against the names of directories.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories. The default value is <see cref="F:System.IO.SearchOption.TopDirectoryOnly" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="searchOption" /> is not a valid <see cref="T:System.IO.SearchOption" /> value.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path encapsulated in the <see cref="T:System.IO.DirectoryInfo" /> object is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <returns>An enumerable collection of directories that matches <paramref name="searchPattern" /> and <paramref name="searchOption" />.</returns>
        IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption);

        /// <summary>Returns an enumerable collection of file information in the current directory.</summary>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path encapsulated in the <see cref="T:System.IO.DirectoryInfo" /> object is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <returns>An enumerable collection of the files in the current directory.</returns>
        IEnumerable<IFileInfo> EnumerateFiles();

        /// <summary>Returns an enumerable collection of file information that matches a search pattern.</summary>
        /// <param name="searchPattern">The search string to match against the names of files.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path encapsulated in the <see cref="T:System.IO.DirectoryInfo" /> object is invalid, (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <returns>An enumerable collection of files that matches <paramref name="searchPattern" />.</returns>
        IEnumerable<IFileInfo> EnumerateFiles(string searchPattern);

        /// <summary>Returns an enumerable collection of file information that matches a specified search pattern and search subdirectory option.</summary>
        /// <param name="searchPattern">The search string to match against the names of files.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories. The default value is <see cref="F:System.IO.SearchOption.TopDirectoryOnly" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="searchOption" /> is not a valid <see cref="T:System.IO.SearchOption" /> value.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path encapsulated in the <see cref="T:System.IO.DirectoryInfo" /> object is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <returns>An enumerable collection of files that matches <paramref name="searchPattern" /> and <paramref name="searchOption" />.</returns>
        IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption);

        /// <summary>Returns the subdirectories of the current directory.</summary>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path encapsulated in the <see cref="T:System.IO.DirectoryInfo" /> object is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <returns>An array of <see cref="T:System.IO.DirectoryInfo" /> objects.</returns>
        IDirectoryInfo[] GetDirectories();

        /// <summary>Returns an array of directories in the current <see cref="T:System.IO.DirectoryInfo" /> matching the given search criteria.</summary>
        /// <param name="searchPattern">The search string to match against the names of directories.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
        /// <exception cref="T:System.ArgumentException">.NET Framework and .NET Core versions older than 2.1: <paramref name="searchPattern" /> contains one or more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path encapsulated in the <see langword="DirectoryInfo" /> object is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <returns>An array of type <see langword="DirectoryInfo" /> matching <paramref name="searchPattern" />.</returns>
        IDirectoryInfo[] GetDirectories(string searchPattern);

        /// <summary>Returns an array of directories in the current <see cref="T:System.IO.DirectoryInfo" /> matching the given search criteria and using a value to determine whether to search subdirectories.</summary>
        /// <param name="searchPattern">The search string to match against the names of directories.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories.</param>
        /// <exception cref="T:System.ArgumentException">.NET Framework and .NET Core versions older than 2.1: <paramref name="searchPattern" /> contains one or more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="searchOption" /> is not a valid <see cref="T:System.IO.SearchOption" /> value.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path encapsulated in the <see langword="DirectoryInfo" /> object is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <returns>An array of type <see langword="DirectoryInfo" /> matching <paramref name="searchPattern" />.</returns>
        IDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption);

        /// <summary>Returns a file list from the current directory.</summary>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path is invalid, such as being on an unmapped drive.</exception>
        /// <returns>An array of type <see cref="T:System.IO.FileInfo" />.</returns>
        IFileInfo[] GetFiles();

        /// <summary>Returns a file list from the current directory matching the given search pattern.</summary>
        /// <param name="searchPattern">The search string to match against the names of files.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
        /// <exception cref="T:System.ArgumentException">.NET Framework and .NET Core versions older than 2.1: <paramref name="searchPattern" /> contains one or more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <returns>An array of type <see cref="T:System.IO.FileInfo" />.</returns>
        IFileInfo[] GetFiles(string searchPattern);

        /// <summary>Returns a file list from the current directory matching the given search pattern and using a value to determine whether to search subdirectories.</summary>
        /// <param name="searchPattern">The search string to match against the names of files.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or all subdirectories.</param>
        /// <exception cref="T:System.ArgumentException">.NET Framework and .NET Core versions older than 2.1: <paramref name="searchPattern" /> contains one or more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="searchOption" /> is not a valid <see cref="T:System.IO.SearchOption" /> value.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <returns>An array of type <see cref="T:System.IO.FileInfo" />.</returns>
        IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption);

        /// <summary>Moves a <see cref="T:System.IO.DirectoryInfo" /> instance and its contents to a new path.</summary>
        /// <param name="destDirName">The name and path to which to move this directory. The destination cannot be another disk volume or a directory with the identical name. It can be an existing directory to which you want to add this directory as a subdirectory.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="destDirName" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="destDirName" /> is an empty string (''").</exception>
        /// <exception cref="T:System.IO.IOException">An attempt was made to move a directory to a different volume.
        /// 
        /// -or-
        /// 
        /// <paramref name="destDirName" /> already exists.
        /// 
        /// -or-
        /// 
        /// You are not authorized to access this path.
        /// 
        /// -or-
        /// 
        /// The directory being moved and the destination directory have the same name.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The destination directory cannot be found.</exception>
        void MoveTo(string destDirName);

        /// <summary>
        /// Create the target file if it does not already exists
        /// </summary>
        /// <param name="name">the name of the file</param>
        /// <param name="autoRename">we return the corresponding FileInfo if autoRename = false VS we find an available name if autoRename = true</param>
        IFileInfo CreateFileIfNotExists(string name, bool autoRename = false);

        /// <summary>
        /// return a fileInfo in the given directory EVEN if the file does not exists
        /// </summary>
        IFileInfo GetFile(string filePath);

        /// <summary>
        /// return true and out the file info if any one is found in the given directory with the given name
        /// </summary>
        bool TryGetFile(string name, out IFileInfo file);

        /// <summary>
        /// Returns true if the folder is empty, i.e. contains no files or subfolders, even empty
        /// </summary>
        /// <returns>true if the folder is empty, false otherwise</returns>
        bool IsEmpty();

        /// <summary>
        /// Copy a Directory with all its sub files and folder
        /// </summary>
        /// <returns>the new folder</returns>
        /// <param name="path">the path of the target folder</param>
        /// <param name="autoRename">if autoRename = true et if the folder exists, we use it, otherwise, we create a new one with ...(2)</param>
        /// <returns></returns>
        IDirectoryInfo CopyTo(string path, bool autoRename = true);

        /// <summary>
        /// Clean a directory if it exists and create it if it does not
        /// </summary>
        IDirectoryInfo CleanDirectory();

        /// <summary>
        /// Returns an estimate of the size of all files in a folder (returns -1 on exception)
        /// </summary>
        /// <param name="searchOption">indicates whether sub-folders are included in this calculation</param>
        /// <returns>An estimate of the size of all files in a folder</returns>
        long EstimateSize(SearchOption searchOption);

        /// <summary>
        /// Return a sub directory EVEN if it does not exists
        /// </summary>
        IDirectoryInfo GetDirectory(string path);

        /// <summary>
        /// Try to get the matching sub directory and return true if it exists
        /// </summary>
        bool TryGetDirectory(string name, out IDirectoryInfo subdirectory);
    }

    /// <inheritdoc cref="PRF.Utils.CoreComponents.IO.IDirectoryInfo" />
    public sealed class DirectoryInfoWrapper : FileSystemInfoWrapper<DirectoryInfo>, IDirectoryInfo
    {
        /// <summary>
        /// Create a DirectoryInfo wrapper around the provided DirectoryInfo
        /// </summary>
        public DirectoryInfoWrapper(DirectoryInfo source) : base(source)
        {
        }

        /// <summary>
        /// Create a DirectoryInfo wrapper around the provided path
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public DirectoryInfoWrapper(string path) : this(new DirectoryInfo(path))
        {
        }

        /// <summary>
        /// Delete if exists (same as default behaviour Except if the path does not exists)
        /// </summary>
        /// <returns>true if deleted</returns>
        public bool DeleteIfExist(bool recursive = true)
        {
            if (!ExistsExplicit) return false;
            _source.Delete(recursive);
            return true;
        }

        /// <inheritdoc />
        public IDirectoryInfo Parent => _source.Parent != null ? new DirectoryInfoWrapper(_source.Parent) : null;

        /// <inheritdoc />
        public IDirectoryInfo Root => new DirectoryInfoWrapper(_source.Root);

        /// <inheritdoc />
        public IDirectoryInfo Create()
        {
            _source.Create();
            _source.Refresh();
            return this;
        }

        /// <inheritdoc />
        public bool CreateIfNotExists()
        {
            if (ExistsExplicit) return false;
            Create();
            return true;
        }

        /// <inheritdoc />
        public IDirectoryInfo CreateSubdirectory(string path)
        {
            var dir = _source.CreateSubdirectory(path);
            return new DirectoryInfoWrapper(dir);
        }

        /// <inheritdoc />
        public IDirectoryInfo CreateSubdirectoryIfNotExists(string path)
        {
            var dir = new DirectoryInfo(Path.Combine(_source.FullName, path));
            if (!dir.Exists)
            {
                dir.Create();
                dir.Refresh();
            }

            return new DirectoryInfoWrapper(dir);
        }

        /// <summary>
        /// Delete the Directory (recursively by default)
        /// </summary>
        public void Delete(bool recursive = true)
        {
            _source.Delete(recursive);
        }

        /// <inheritdoc />
        public IEnumerable<IDirectoryInfo> EnumerateDirectories()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var dir in _source.EnumerateDirectories())
            {
                yield return new DirectoryInfoWrapper(dir);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var dir in _source.EnumerateDirectories(searchPattern))
            {
                yield return new DirectoryInfoWrapper(dir);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var dir in _source.EnumerateDirectories(searchPattern, searchOption))
            {
                yield return new DirectoryInfoWrapper(dir);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IFileInfo> EnumerateFiles()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var file in _source.EnumerateFiles())
            {
                yield return new FileInfoWrapper(file);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var file in _source.EnumerateFiles(searchPattern))
            {
                yield return new FileInfoWrapper(file);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var file in _source.EnumerateFiles(searchPattern, searchOption))
            {
                yield return new FileInfoWrapper(file);
            }
        }

        /// <inheritdoc />
        public IDirectoryInfo[] GetDirectories()
        {
            return EnumerateDirectories().ToArray();
        }

        /// <inheritdoc />
        public IDirectoryInfo[] GetDirectories(string searchPattern)
        {
            return EnumerateDirectories(searchPattern).ToArray();
        }

        /// <inheritdoc />
        public IDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            return EnumerateDirectories(searchPattern, searchOption).ToArray();
        }

        /// <inheritdoc />
        public IFileInfo[] GetFiles()
        {
            return EnumerateFiles().ToArray();
        }

        /// <inheritdoc />
        public IFileInfo[] GetFiles(string searchPattern)
        {
            return EnumerateFiles(searchPattern).ToArray();
        }

        /// <inheritdoc />
        public IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
        {
            return EnumerateFiles(searchPattern, searchOption).ToArray();
        }

        /// <inheritdoc />
        public void MoveTo(string destDirName)
        {
            _source.MoveTo(destDirName);
        }

        /// <inheritdoc />
        public IFileInfo CreateFileIfNotExists(string fileName, bool autoRename = false)
        {
            var file = autoRename
                ? new FileInfo(FilesAndDirectoryInfoExtension.AutoRenameFileToAvoidDuplicate(_source.FullName, fileName))
                : new FileInfo(Path.Combine(_source.FullName, fileName));
            
            var f = new FileInfoWrapper(file);
            f.CreateIfNotExistAndClose();
            return f;
        }

        /// <inheritdoc />
        public bool TryGetFile(string name, out IFileInfo file)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    var fileFound = new FileInfoWrapper(Path.Combine(FullName, name));
                    if (fileFound.Exists)
                    {
                        file = fileFound;
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
            }

            file = null;
            return false;
        }

        /// <inheritdoc />
        public IFileInfo GetFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return null;
            return new FileInfoWrapper(Path.Combine(FullName, filePath));
        }

        /// <inheritdoc />
        public bool IsEmpty()
        {
            return !Directory.EnumerateFileSystemEntries(_source.FullName).Any();
        }

        /// <inheritdoc />
        public IDirectoryInfo CopyTo(string path, bool autoRename = true)
        {
            if (!ExistsExplicit) return null;

            // generates a new name if a file with the same name already exists (otherwise, just check the folder separator)
            path = autoRename
                ? FilesAndDirectoryInfoExtension.GetDirectoryNameAndAvoidDuplicate(path)
                : path.TrimEnd(Path.DirectorySeparatorChar);

            // Create the target folder if it does not exist
            Directory.CreateDirectory(path);

            //Create all folders
            foreach (var dirPath in _source.GetDirectories("*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.FullName.Replace(_source.FullName.TrimEnd(Path.DirectorySeparatorChar), path));

            //Then all files
            foreach (var newPath in _source.GetFiles("*", SearchOption.AllDirectories))
                newPath.CopyTo(newPath.FullName.Replace(_source.FullName.TrimEnd(Path.DirectorySeparatorChar), path), false);

            return new DirectoryInfoWrapper(path);
        }

        /// <inheritdoc />
        public IDirectoryInfo CleanDirectory()
        {
            if (!ExistsExplicit)
            {
                Create();
            }
            else
            {
                foreach (var f in EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    f.IsReadOnly = false;
                    f.Delete();
                }

                foreach (var subDir in EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    // only deletes subfolders and not the original folder
                    subDir.Delete();
                }
            }

            Refresh();
            return this;
        }

        /// <inheritdoc />
        public long EstimateSize(SearchOption searchOption)
        {
            try
            {
                return EnumerateFiles("*", searchOption)
                    .Select(o => o.Length)
                    .Aggregate((long)0, (i, size) => i + size);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <inheritdoc />
        public IDirectoryInfo GetDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            return new DirectoryInfoWrapper(Path.Combine(FullName, path));
        }

        /// <inheritdoc />
        public bool TryGetDirectory(string name, out IDirectoryInfo subdirectory)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    var subDir = new DirectoryInfo(Path.Combine(FullName, name));
                    if (subDir.Exists)
                    {
                        subdirectory = new DirectoryInfoWrapper(subDir);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
            }


            subdirectory = null;
            return false;
        }
    }
}