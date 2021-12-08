using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PRF.Utils.CoreComponents.Diagnostic
{
    /// <summary>
    /// Class that has a similar function thant debug.assert in a .net frk but with a registration for a debug windows
    /// </summary>
    public static class DebugCore
    {
        private static Func<AssertionFailedResult, AssertionResponse> _assertionFailedCallBack;

        /// <summary>
        /// Set a callback that will be called when an assert or a fail is triggered
        /// </summary>
        public static void SetAssertionFailedCallback(Func<AssertionFailedResult, AssertionResponse> assertionFailedCallBack)
        {
            _assertionFailedCallBack = assertionFailedCallBack;
        }

        private static void FailCore(string stackTrace, string message, string errorSource)
        {
            // if no callback has been given (like in unit test), we ignore it
            if (_assertionFailedCallBack == null) return;
            
            // use the call back to provide specific behaviour
            var response = _assertionFailedCallBack(new AssertionFailedResult(stackTrace, message, errorSource));
            switch (response)
            {
                case AssertionResponse.Ignore:
                    return;
                case AssertionResponse.Debug:
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                    else
                    {
                        Debugger.Launch();
                    }

                    return;
                default:
                // ReSharper disable once RedundantCaseLabel
                case AssertionResponse.TerminateProcess:
                    Environment.FailFast("Exiting from assertion terminate process");
                    return;
            }
        }

        /// <summary>
        /// Do a Debug.Fail equivalent by calling the registered callback method
        /// </summary>
        [Conditional("DEBUG")]
        public static void Fail(string message, [CallerMemberName] string methodSource = "")
        {
            FailCore(GetStackTrace(), message, methodSource);
        }

        /// <summary>
        /// Do a Debug.Assert equivalent by calling the registered callback method
        /// </summary>
        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message, [CallerMemberName] string methodSource = "")
        {
            if (!condition)
            {
                Fail(message, methodSource);
            }
        }

        private static string GetStackTrace()
        {
            try
            {
                return new StackTrace(0, true).ToString();
            }
            catch
            {
                return "";
            }
        }
    }

    /// <summary>
    /// The assertion failed information for the registered callback
    /// </summary>
    public sealed class AssertionFailedResult
    {
        /// <summary>
        /// The stack trace of the debug fail
        /// </summary>
        public string StackTrace { get; }
        
        /// <summary>
        /// The specific message given by the caller
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// The caller method name (use a callerMembername attribute
        /// </summary>
        public string SourceMethod { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AssertionFailedResult(string stackTrace, string message, string sourceMethod)
        {
            StackTrace = stackTrace;
            Message = message;
            SourceMethod = sourceMethod;
        }
    }

    /// <summary>
    /// The assertion failed response frome the registered callback
    /// </summary>
    public enum AssertionResponse
    {
        /// <summary>
        /// Ignore and continue execution as if no failure arise
        /// </summary>
        Ignore,
        
        /// <summary>
        /// Try to debug.break if attached
        /// </summary>
        Debug,
        
        /// <summary>
        /// Kill the process immediatly
        /// </summary>
        TerminateProcess
    }
}