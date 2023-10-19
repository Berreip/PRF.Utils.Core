using System;
using System.IO;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace PRF.Utils.CoreComponents.IO
{
    /// <summary>
    /// Wrapper around a FileSystemInfo to allow unit testing more easily
    /// </summary>
    public interface IFileSystemInfo
    {
        /// <summary>Gets the extension part of the file name, including the leading dot <c>.</c> even if it is the entire file name, or an empty string if no extension is present.</summary>
        /// <returns>A string containing the <see cref="T:System.IO.FileSystemInfo" /> extension.</returns>
        string Extension { get; }

        /// <summary>For files, gets the name of the file. For directories, gets the name of the last directory in the hierarchy if a hierarchy exists. Otherwise, the <see langword="Name" /> property gets the name of the directory.</summary>
        /// <returns>A string that is the name of the parent directory, the name of the last directory in the hierarchy, or the name of a file, including the file name extension.</returns>
        string Name { get; }

        /// <summary>Gets the full path of the directory or file.</summary>
        /// <exception cref="T:System.IO.PathTooLongException">The fully qualified path and file name exceed the system-defined maximum length.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <returns>A string containing the full path.</returns>
        string FullName { get; }

        /// <summary>Gets a value indicating whether the file or directory exists.</summary>
        /// <returns>
        /// <see langword="true" /> if the file or directory exists; otherwise, <see langword="false" />.</returns>
        bool Exists { get; }

        /// <summary>
        /// Returns true if it exist but do a Refresh before calling (otherwise, the exist state during ctor/last refresh is used by default FileSystemInfo behaviour)
        /// </summary>
        bool ExistsExplicit { get; }

        /// <summary>Gets or sets the creation time of the current file or directory.</summary>
        /// <exception cref="T:System.IO.IOException">
        /// <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot initialize the data.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid; for example, it is on an unmapped drive.</exception>
        /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The caller attempts to set an invalid creation time.</exception>
        /// <returns>The creation date and time of the current <see cref="T:System.IO.FileSystemInfo" /> object.</returns>
        DateTime CreationTime { get; }

        /// <summary>Gets or sets the creation time, in coordinated universal time (UTC), of the current file or directory.</summary>
        /// <exception cref="T:System.IO.IOException">
        /// <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot initialize the data.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid; for example, it is on an unmapped drive.</exception>
        /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The caller attempts to set an invalid creation time.</exception>
        /// <returns>The creation date and time in UTC format of the current <see cref="T:System.IO.FileSystemInfo" /> object.</returns>
        DateTime CreationTimeUtc { get; }

        /// <summary>Refreshes the state of the object.</summary>
        /// <exception cref="T:System.IO.IOException">A device such as a disk drive is not ready.</exception>
        void Refresh();

        /// <summary>Gets or sets the time the current file or directory was last accessed.</summary>
        /// <exception cref="T:System.IO.IOException">
        /// <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot initialize the data.</exception>
        /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The caller attempts to set an invalid access time</exception>
        /// <returns>The time that the current file or directory was last accessed.</returns>
        DateTime LastAccessTime { get; }

        /// <summary>Gets or sets the time, in coordinated universal time (UTC), that the current file or directory was last accessed.</summary>
        /// <exception cref="T:System.IO.IOException">
        /// <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot initialize the data.</exception>
        /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The caller attempts to set an invalid access time.</exception>
        /// <returns>The UTC time that the current file or directory was last accessed.</returns>
        DateTime LastAccessTimeUtc { get; }

        /// <summary>Gets or sets the time when the current file or directory was last written to.</summary>
        /// <exception cref="T:System.IO.IOException">
        /// <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot initialize the data.</exception>
        /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The caller attempts to set an invalid write time.</exception>
        /// <returns>The time the current file was last written.</returns>
        DateTime LastWriteTime { get; }

        /// <summary>Gets or sets the time, in coordinated universal time (UTC), when the current file or directory was last written to.</summary>
        /// <exception cref="T:System.IO.IOException">
        /// <see cref="M:System.IO.FileSystemInfo.Refresh" /> cannot initialize the data.</exception>
        /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The caller attempts to set an invalid write time.</exception>
        /// <returns>The UTC time when the current file was last written to.</returns>
        DateTime LastWriteTimeUtc { get; }
    }

    /// <inheritdoc />
    public abstract class FileSystemInfoWrapper<T> : IFileSystemInfo where T : FileSystemInfo
    {
        /// <summary>
        /// the underlying fileSystemInfo 
        /// </summary>
        protected readonly T _source;

        /// <summary>
        /// wrapper ctor
        /// </summary>
        protected FileSystemInfoWrapper(T source)
        {
            _source = source;
        }

        /// <inheritdoc />
        public string Extension => _source.Extension;

        /// <inheritdoc />
        public string Name => _source.Name;

        /// <inheritdoc />
        public string FullName => _source.FullName;

        /// <inheritdoc />
        public bool Exists => _source.Exists;

        /// <inheritdoc />
        public DateTime CreationTime => _source.CreationTime;

        /// <inheritdoc />
        public DateTime CreationTimeUtc => _source.CreationTimeUtc;

        /// <inheritdoc />
        public void Refresh() => _source.Refresh();

        /// <inheritdoc />
        public DateTime LastAccessTime => _source.LastAccessTime;

        /// <inheritdoc />
        public DateTime LastAccessTimeUtc => _source.LastAccessTimeUtc;

        /// <inheritdoc />
        public DateTime LastWriteTime => _source.LastWriteTime;

        /// <inheritdoc />
        public DateTime LastWriteTimeUtc => _source.LastWriteTimeUtc;

        /// <inheritdoc />
        public bool ExistsExplicit
        {
            get
            {
                _source.Refresh();
                return _source.Exists;
            }
        }

        /// <summary>Returns the original path. Use the <see cref="P:System.IO.FileSystemInfo.FullName" /> or <see cref="P:System.IO.FileSystemInfo.Name" /> properties for the full path or file/directory name.</summary>
        /// <returns>A string with the original path.</returns>
        public override string ToString() => _source.ToString();
    }
}