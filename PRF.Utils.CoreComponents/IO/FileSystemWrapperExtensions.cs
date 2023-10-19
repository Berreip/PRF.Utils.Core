using System;
using System.Diagnostics;
using System.Threading;
// ReSharper disable UnusedMethodReturnValue.Global

namespace PRF.Utils.CoreComponents.IO
{
    /// <summary>
    /// Extensions around IFileSystemInfo wrappers
    /// </summary>
    public static class FileSystemWrapperExtensions
    {
        /// <summary>
        /// Try to delete the folder and cycle up to the confirmation that the file is deleted or the timeout is reached (use this methods only in unit tests)
        /// Note: the deletion is NOT sync even if Ms pretends it. so using this in unit test context may be useful
        /// Default timeout is 5 seconds
        /// </summary>
        public static bool DeleteIfExistAndWaitDeletion(this IFileSystemInfo fileSystemInfo, TimeSpan? timeout = null, bool recursive = true)
        {
            if (!timeout.HasValue)
            {
                timeout = TimeSpan.FromSeconds(5);
            }

            if (timeout > TimeSpan.FromHours(1))
                throw new ArgumentException("Timeout cannot exceed 1 hour.");

            if (!fileSystemInfo.ExistsExplicit) return true;
            switch (fileSystemInfo)
            {
                case FileInfoWrapper fileInfoWrapper:
                    fileInfoWrapper.Delete();
                    break;
                case DirectoryInfoWrapper directoryInfoWrapper:
                    directoryInfoWrapper.Delete(recursive);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileSystemInfo));
            }

            var sw = new Stopwatch();
            sw.Start();
            try
            {
                while (sw.Elapsed <= timeout)
                {
                    try
                    {
                        if (!fileSystemInfo.ExistsExplicit)
                        {
                            return true;
                        }

                        Thread.Sleep(100);
                    }
                    catch
                    {
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
    }
}