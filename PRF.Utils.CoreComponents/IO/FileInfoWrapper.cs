using System.IO;
using System.Text;

// ReSharper disable ClassNeverInstantiated.Global

namespace PRF.Utils.CoreComponents.IO
{
    /// <summary>
    /// Wrapper around a FileInfo to allow unit testing more easily
    /// </summary>
    public interface IFileInfo : IFileSystemInfo
    {
        /// <summary>
        /// Delete if exists (same as default behaviour Except if the path does not exists)
        /// </summary>
        /// <returns>true if deleted</returns>
        bool DeleteIfExist();

        /// <summary>Gets an instance of the parent directory.</summary>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <returns>A <see cref="T:System.IO.DirectoryInfo" /> object representing the parent directory of this file.</returns>
        IDirectoryInfo Directory { get; }

        /// <summary>Gets a string representing the directory's full path.</summary>
        /// <exception cref="T:System.ArgumentNullException">
        /// <see langword="null" /> was passed in for the directory name.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The fully qualified path name exceeds the system-defined maximum length.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <returns>A string representing the directory's full path.</returns>
        string DirectoryName { get; }

        /// <summary>Permanently deletes a file.</summary>
        /// <exception cref="T:System.IO.IOException">The target file is open or memory-mapped on a computer running Microsoft Windows NT.
        /// 
        /// -or-
        /// 
        /// There is an open handle on the file, and the operating system is Windows XP or earlier. This open handle can result from enumerating directories and files. For more information, see How to: Enumerate Directories and Files.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The path is a directory.</exception>
        void Delete();

        /// <summary>
        /// Opens a text file, reads all the text in the file into a string, and then closes the file.
        /// </summary>
        string ReadAllText();

        /// <summary>Gets or sets a value that determines if the current file is read only.</summary>
        /// <exception cref="T:System.IO.FileNotFoundException">The file described by the current <see cref="T:System.IO.FileInfo" /> object could not be found.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">This operation is not supported on the current platform.
        /// 
        /// -or-
        /// 
        /// The caller does not have the required permission.</exception>
        /// <exception cref="T:System.ArgumentException">The user does not have write permission, but attempted to set this property to <see langword="false" />.</exception>
        /// <returns>
        /// <see langword="true" /> if the current file is read only; otherwise, <see langword="false" />.</returns>
        bool IsReadOnly { get; set; }

        /// <summary>Gets the size, in bytes, of the current file.</summary>
        /// <exception cref="T:System.IO.IOException">
        /// <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot update the state of the file or directory.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file does not exist.
        /// 
        /// -or-
        /// 
        /// The <see langword="Length" /> property is called for a directory.</exception>
        /// <returns>The size of the current file in bytes.</returns>
        long Length { get; }

        /// <summary>Creates a <see cref="T:System.IO.StreamWriter" /> that appends text to the file represented by this instance of the <see cref="T:System.IO.FileInfo" />.</summary>
        /// <returns>A new <see langword="StreamWriter" />.</returns>
        StreamWriter AppendText();

        /// <summary>Copies an existing file to a new file, disallowing the overwriting of an existing file.</summary>
        /// <param name="destFileName">The name of the new file to copy to.</param>
        /// <exception cref="T:System.ArgumentException">.NET Framework and .NET Core versions older than 2.1: <paramref name="destFileName" /> is empty, contains only white spaces, or contains invalid characters.</exception>
        /// <exception cref="T:System.IO.IOException">An error occurs, or the destination file already exists.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="destFileName" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">A directory path is passed in, or the file is being moved to a different drive.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The directory specified in <paramref name="destFileName" /> does not exist.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        /// <exception cref="T:System.NotSupportedException">
        /// <paramref name="destFileName" /> contains a colon (:) within the string but does not specify the volume.</exception>
        /// <returns>A new file with a fully qualified path.</returns>
        IFileInfo CopyTo(string destFileName);

        /// <summary>Copies an existing file to a new file, allowing the overwriting of an existing file.</summary>
        /// <param name="destFileName">The name of the new file to copy to.</param>
        /// <param name="overwrite">
        /// <see langword="true" /> to allow an existing file to be overwritten; otherwise, <see langword="false" />.</param>
        /// <exception cref="T:System.ArgumentException">.NET Framework and .NET Core versions older than 2.1: <paramref name="destFileName" /> is empty, contains only white spaces, or contains invalid characters.</exception>
        /// <exception cref="T:System.IO.IOException">An error occurs, or the destination file already exists and <paramref name="overwrite" /> is <see langword="false" />.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="destFileName" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The directory specified in <paramref name="destFileName" /> does not exist.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">A directory path is passed in, or the file is being moved to a different drive.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        /// <exception cref="T:System.NotSupportedException">
        /// <paramref name="destFileName" /> contains a colon (:) in the middle of the string.</exception>
        /// <returns>A new file, or an overwrite of an existing file if <paramref name="overwrite" /> is <see langword="true" />. If the file exists and <paramref name="overwrite" /> is <see langword="false" />, an <see cref="T:System.IO.IOException" /> is thrown.</returns>
        IFileInfo CopyTo(string destFileName, bool overwrite);

        /// <summary>Creates a file.</summary>
        /// <returns>A new file.</returns>
        FileStream Create();

        /// <summary>Creates a <see cref="T:System.IO.StreamWriter" /> that writes a new text file.</summary>
        /// <exception cref="T:System.UnauthorizedAccessException">The file name is a directory.</exception>
        /// <exception cref="T:System.IO.IOException">The disk is read-only.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <returns>A new <see langword="StreamWriter" />.</returns>
        StreamWriter CreateText();

        /// <summary>Moves a specified file to a new location, providing the option to specify a new file name.</summary>
        /// <param name="destFileName">The path to move the file to, which can specify a different file name.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs, such as the destination file already exists or the destination device is not ready.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="destFileName" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">.NET Framework and .NET Core versions older than 2.1: <paramref name="destFileName" /> is empty, contains only white spaces, or contains invalid characters.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        /// <paramref name="destFileName" /> is read-only or is a directory.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file is not found.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        /// <exception cref="T:System.NotSupportedException">
        /// <paramref name="destFileName" /> contains a colon (:) in the middle of the string.</exception>
        void MoveTo(string destFileName);

        /// <summary>Opens a file in the specified mode.</summary>
        /// <param name="mode">A <see cref="T:System.IO.FileMode" /> constant specifying the mode (for example, <see langword="Open" /> or <see langword="Append" />) in which to open the file.</param>
        /// <exception cref="T:System.IO.FileNotFoundException">The file is not found.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The file is read-only or is a directory.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.IO.IOException">The file is already open.</exception>
        /// <returns>A file opened in the specified mode, with read/write access and unshared.</returns>
        FileStream Open(FileMode mode);

        /// <summary>Opens a file in the specified mode with read, write, or read/write access.</summary>
        /// <param name="mode">A <see cref="T:System.IO.FileMode" /> constant specifying the mode (for example, <see langword="Open" /> or <see langword="Append" />) in which to open the file.</param>
        /// <param name="access">A <see cref="T:System.IO.FileAccess" /> constant specifying whether to open the file with <see langword="Read" />, <see langword="Write" />, or <see langword="ReadWrite" /> file access.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file is not found.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        /// <see cref="P:System.IO.FileInfo.Name" /> is read-only or is a directory.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.IO.IOException">The file is already open.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <see cref="P:System.IO.FileInfo.Name" /> is empty or contains only white spaces.</exception>
        /// <exception cref="T:System.ArgumentNullException">One or more arguments is null.</exception>
        /// <returns>A <see cref="T:System.IO.FileStream" /> object opened in the specified mode and access, and unshared.</returns>
        FileStream Open(FileMode mode, FileAccess access);

        /// <summary>Opens a file in the specified mode with read, write, or read/write access and the specified sharing option.</summary>
        /// <param name="mode">A <see cref="T:System.IO.FileMode" /> constant specifying the mode (for example, <see langword="Open" /> or <see langword="Append" />) in which to open the file.</param>
        /// <param name="access">A <see cref="T:System.IO.FileAccess" /> constant specifying whether to open the file with <see langword="Read" />, <see langword="Write" />, or <see langword="ReadWrite" /> file access.</param>
        /// <param name="share">A <see cref="T:System.IO.FileShare" /> constant specifying the type of access other <see langword="FileStream" /> objects have to this file.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file is not found.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        /// <see cref="P:System.IO.FileInfo.Name" /> is read-only or is a directory.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.IO.IOException">The file is already open.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <see cref="P:System.IO.FileInfo.Name" /> is empty or contains only white spaces.</exception>
        /// <exception cref="T:System.ArgumentNullException">One or more arguments is null.</exception>
        /// <returns>A <see cref="T:System.IO.FileStream" /> object opened with the specified mode, access, and sharing options.</returns>
        FileStream Open(FileMode mode, FileAccess access, FileShare share);

        /// <summary>Creates a read-only <see cref="T:System.IO.FileStream" />.</summary>
        /// <exception cref="T:System.UnauthorizedAccessException">
        /// <see cref="P:System.IO.FileInfo.Name" /> is read-only or is a directory.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.IO.IOException">The file is already open.</exception>
        /// <returns>A new read-only <see cref="T:System.IO.FileStream" /> object.</returns>
        FileStream OpenRead();

        /// <summary>Creates a <see cref="T:System.IO.StreamReader" /> with UTF8 encoding that reads from an existing text file.</summary>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file is not found.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        /// <see cref="P:System.IO.FileInfo.Name" /> is read-only or is a directory.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <returns>A new <see langword="StreamReader" /> with UTF8 encoding.</returns>
        StreamReader OpenText();

        /// <summary>Creates a write-only <see cref="T:System.IO.FileStream" />.</summary>
        /// <exception cref="T:System.UnauthorizedAccessException">The path specified when creating an instance of the <see cref="T:System.IO.FileInfo" /> object is read-only or is a directory.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The path specified when creating an instance of the <see cref="T:System.IO.FileInfo" /> object is invalid, such as being on an unmapped drive.</exception>
        /// <returns>A write-only unshared <see cref="T:System.IO.FileStream" /> object for a new or existing file.</returns>
        FileStream OpenWrite();

        /// <summary>Replaces the contents of a specified file with the file described by the current <see cref="T:System.IO.FileInfo" /> object, deleting the original file, and creating a backup of the replaced file.</summary>
        /// <param name="destinationFileName">The name of a file to replace with the current file.</param>
        /// <param name="destinationBackupFileName">The name of a file with which to create a backup of the file described by the destFileName parameter.</param>
        /// <exception cref="T:System.ArgumentException">The path described by the destFileName parameter was not of a legal form.
        /// 
        /// -or-
        /// 
        /// The path described by the destBackupFileName parameter was not of a legal form.</exception>
        /// <exception cref="T:System.ArgumentNullException">The destFileName parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file described by the current <see cref="T:System.IO.FileInfo" /> object could not be found.
        /// 
        /// -or-
        /// 
        /// The file described by the <paramref name="destinationFileName" /> parameter could not be found.</exception>
        /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Microsoft Windows NT or later.</exception>
        /// <returns>A <see cref="T:System.IO.FileInfo" /> object that encapsulates information about the file described by the destFileName parameter.</returns>
        IFileInfo Replace(string destinationFileName, string destinationBackupFileName);

        /// <summary>
        /// Create the file and close the stream
        /// </summary>
        void CreateAndClose();

        /// <summary>
        /// Create the file and close the stream
        /// </summary>
        /// <returns>true if the file has been created, false otherwise</returns>
        bool CreateIfNotExistAndClose();

        /// <summary>
        /// Write the text into the file and close it
        /// </summary>
        void WriteAllText(string stringContent);

        /// <summary>
        /// Write the text into the file and close it
        /// </summary>
        void WriteAllText(string stringContent, Encoding encoding);

        /// <summary>
        /// Read all file content bytes
        /// </summary>
        byte[] ReadAllBytes();

        /// <summary>
        /// Write the bytes into the file and close it
        /// </summary>
        void WriteAllBytes(byte[] bytes);

        /// <summary>
        /// Read all file content as string lines
        /// </summary>
        string[] ReadAllLines();
        
        /// <summary>
        /// Read all file content as string lines
        /// </summary>
        string[] ReadAllLines(Encoding encoding);
        
        /// <summary>
        /// Append the text (after existing data) into the file and close it
        /// </summary>
        void AppendAllText(string contents);
        
        /// <summary>
        /// Append the text (after existing data) into the file and close it
        /// </summary>
        void AppendAllText(string contents, Encoding encoding);

        /// <summary>
        /// Get a relative path by removing the 'pathToRemove' part but keeping the subfolders that follow
        /// Works with or without the '\' at the end of beforeRelativePath
        /// </summary>
        /// <param name="pathToRemove">the path to delete</param>
        /// <returns>the remaining relative path</returns>
        string GetRelativePath(string pathToRemove);

        /// <summary>
        /// Remove the ReadOnly attribute from the file
        /// </summary>
        void RemoveReadonlyAttribute();
    }

    /// <inheritdoc cref="PRF.Utils.CoreComponents.IO.IFileInfo" />
    public sealed class FileInfoWrapper : FileSystemInfoWrapper<FileInfo>, IFileInfo
    {
        /// <summary>
        /// Create a FileInfo wrapper around the provided FileInfo
        /// </summary>
        public FileInfoWrapper(FileInfo source) : base(source)
        {
        }

        /// <summary>
        /// Create a FileInfo wrapper around the provided path
        /// </summary>
        public FileInfoWrapper(string path) : this(new FileInfo(path))
        {
        }

        /// <summary>
        /// Delete if exists (same as default behaviour Except if the path does not exists)
        /// </summary>
        /// <returns>true if deleted</returns>
        public bool DeleteIfExist()
        {
            if (!ExistsExplicit) return false;
            _source.Delete();
            return true;
        }

        /// <inheritdoc />
        public FileStream Create() => _source.Create();

        /// <inheritdoc />
        public void CreateAndClose()
        {
            // create the parent if needed
            Directory?.CreateIfNotExists();
            _source.Create().Close();
        }

        /// <inheritdoc />
        public bool CreateIfNotExistAndClose()
        {
            if (ExistsExplicit) return false;
            CreateAndClose();
            _source.Refresh();
            return true;
        }

        /// <inheritdoc />
        public void WriteAllText(string stringContent) => File.WriteAllText(_source.FullName, stringContent);

        /// <inheritdoc />
        public void WriteAllText(string stringContent, Encoding encoding) => File.WriteAllText(_source.FullName, stringContent, encoding);

        /// <inheritdoc />
        public byte[] ReadAllBytes() => File.ReadAllBytes(_source.FullName);

        /// <inheritdoc />
        public void WriteAllBytes(byte[] bytes) => File.WriteAllBytes(_source.FullName, bytes);

        /// <inheritdoc />
        public string[] ReadAllLines() => File.ReadAllLines(_source.FullName);

        /// <inheritdoc />
        public string[] ReadAllLines(Encoding encoding) => File.ReadAllLines(_source.FullName, encoding);

        /// <inheritdoc />
        public void AppendAllText(string contents) => File.AppendAllText(_source.FullName, contents);

        /// <inheritdoc />
        public void AppendAllText(string contents, Encoding encoding) => File.AppendAllText(_source.FullName, contents, encoding);

        /// <inheritdoc />
        public IDirectoryInfo Directory => new DirectoryInfoWrapper(_source.Directory);

        /// <inheritdoc />
        public string DirectoryName => _source.DirectoryName;

        /// <summary>
        /// Delete the File
        /// </summary>
        public void Delete()
        {
            _source.Delete();
        }

        /// <summary>
        /// Opens a text file, reads all the text in the file into a string, and then closes the file.
        /// </summary>
        public string ReadAllText()
        {
            return File.ReadAllText(_source.FullName);
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get => _source.IsReadOnly;
            set => _source.IsReadOnly = value;
        }

        /// <inheritdoc />
        public long Length => _source.Length;

        /// <inheritdoc />
        public StreamWriter AppendText()
        {
            return _source.AppendText();
        }

        /// <summary>
        /// Get a relative path by removing the 'pathToRemove' part but keeping the subfolders that follow
        /// Works with or without the '\' at the end of beforeRelativePath
        /// </summary>
        /// <param name="pathToRemove">the path to delete</param>
        /// <returns>the remaining relative path</returns>
        public string GetRelativePath(string pathToRemove)
        {
            // clear the beforeRelativePath with and without '\'
            // ex: C:\Toto\tata\tati\file.txt && beforeRelativePath =  C:\Toto\tata ==> tati\file.txt (WITHOUT '\')
            // ex: C:\Toto\tata\tati\file.txt && beforeRelativePath =  C:\Toto\tata\ ==> tati\file.txt (WITH '\')
            return FullName.Replace(pathToRemove + Path.DirectorySeparatorChar, string.Empty).Replace(pathToRemove, string.Empty);
        }

        /// <summary>
        /// Remove the ReadOnly attribute from the file
        /// </summary>
        public void RemoveReadonlyAttribute()
        {
            if (IsReadOnly)
            {
                _source.Attributes &= ~FileAttributes.ReadOnly;
            }
        }

        /// <inheritdoc />
        public IFileInfo CopyTo(string destFileName) => new FileInfoWrapper(_source.CopyTo(destFileName));

        /// <inheritdoc />
        public IFileInfo CopyTo(string destFileName, bool overwrite) => new FileInfoWrapper(_source.CopyTo(destFileName, overwrite));

        /// <inheritdoc />
        public StreamWriter CreateText() => _source.CreateText();

        /// <inheritdoc />
        public void MoveTo(string destFileName) => _source.MoveTo(destFileName);


        /// <inheritdoc />
        public FileStream Open(FileMode mode) => _source.Open(mode);

        /// <inheritdoc />
        public FileStream Open(FileMode mode, FileAccess access) => _source.Open(mode, access);

        /// <inheritdoc />
        public FileStream Open(FileMode mode, FileAccess access, FileShare share) => _source.Open(mode, access, share);

        /// <inheritdoc />
        public FileStream OpenRead() => _source.OpenRead();

        /// <inheritdoc />
        public StreamReader OpenText() => _source.OpenText();

        /// <inheritdoc />
        public FileStream OpenWrite() => _source.OpenWrite();

        /// <inheritdoc />
        public IFileInfo Replace(string destinationFileName, string destinationBackupFileName) => new FileInfoWrapper(_source.Replace(destinationFileName, destinationBackupFileName));
    }
}