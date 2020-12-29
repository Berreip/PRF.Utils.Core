using System;
using System.Threading.Tasks;

namespace PRF.Utils.CoreComponents.Async
{
    /// <summary>
    /// Class that start wrap a callback in a try catch. Provide also a finally which is itself wrapped.
    /// </summary>
    public static class WrapperCore
    {
        /// <summary>
        /// Invoke an async callback within a try catch with an optional finally
        /// </summary>
        public static async Task WrapAsync(this Func<Task> callback, Action<Exception> onErrorAction, Action onfinally = null)
        {
            try
            {
                await callback.Invoke().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                onErrorAction?.Invoke(e);
            }
            finally
            {
                InvokeFinally(onfinally, onErrorAction);
            }
        }

        /// <summary>
        /// Invoke an async callback within a try catch with an optional finally
        /// </summary>
        public static async Task<T> WrapAsync<T>(this Func<Task<T>> callback, Action<Exception> onErrorAction, Action onfinally = null)
        {
            try
            {
                return await callback.Invoke().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                onErrorAction?.Invoke(e);
                return default;
            }
            finally
            {
                InvokeFinally(onfinally, onErrorAction);
            }
        }

        internal static void InvokeFinally(Action onFinally, Action<Exception> onErrorAction)
        {
            try
            {
                onFinally?.Invoke();
            }
            catch (Exception e)
            {
                onErrorAction?.Invoke(e);
            }
        }
    }
}
