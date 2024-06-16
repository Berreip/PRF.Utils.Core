using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PRF.Utils.Tracer.Listener.Traces;

namespace PRF.Utils.Tracer.Listener.Helpers
{
    /// <summary>
    /// Utility method class for the Listener
    /// </summary>
    internal static class ListenerHelpers
    {
        private static readonly Dictionary<SourceLevels, TraceEventType> _converter
            = new Dictionary<SourceLevels, TraceEventType>
            {
                { SourceLevels.Off, TraceEventType.Critical }, // Off for TraceEventType so just the critical
                { SourceLevels.Critical, TraceEventType.Critical },
                { SourceLevels.Error, TraceEventType.Error },
                { SourceLevels.Warning, TraceEventType.Warning },
                { SourceLevels.Information, TraceEventType.Information },
                { SourceLevels.Verbose, TraceEventType.Verbose },
                { SourceLevels.All, TraceEventType.Verbose }, // All == verbose
            };

        private const string DEFAULT_NULL_STRING = "NULL_DATA";

        public static string ToStringOrNull(this object data)
        {
            return data?.ToString() ?? DEFAULT_NULL_STRING;
        }

        public static string ToStringOrNullList(this object[] data)
        {
            return data == null
                ? DEFAULT_NULL_STRING
                : string.Join(", ", data.Select(o => o ?? DEFAULT_NULL_STRING));
        }

        /// <summary>
        /// returns a page with only the error trace
        /// </summary>
        public static TraceData[] GetExceptionArray(string exceptionMessage)
        {
            return new[]
            {
                new TraceData(new TraceEventCache(), nameof(TraceListenerSync), TraceEventType.Error, -1, exceptionMessage),
            };
        }

        /// <summary>
        /// Checks the input values of a Listener
        /// </summary>
        /// <param name="timeForFlush">the maximum flush time between two pages</param>
        /// <param name="poolingPageSize">the maximum size of a page</param>
        public static void CheckEntryValuesAndThrowExceptionIfFailed(TimeSpan timeForFlush, int poolingPageSize)
        {
            if (poolingPageSize <= 0)
            {
                throw new ArgumentException($"The page size {poolingPageSize} should be a strictly positive value");
            }

            if (timeForFlush < TimeSpan.FromMilliseconds(50) || timeForFlush > TimeSpan.FromHours(1))
            {
                throw new ArgumentException(
                    $"The timeForFlush {timeForFlush.TotalMilliseconds} ms should be greater than the minimum allowed time 50 ms AND lower than the maximum allowed time (1 hour) in order to ensure correct operation");
            }
        }

        /// <summary>
        /// Convert a source level to traceEvent type
        /// </summary>
        public static TraceEventType ToTraceEventType(this SourceLevels level)
        {
            return _converter.TryGetValue(level, out var eventType) ? eventType : TraceEventType.Verbose;
        }
    }
}