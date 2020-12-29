using System;
using System.Threading.Tasks;

namespace PRF.Utils.CoreComponents.Async
{
    /// <summary>
    /// Class that start a new task and wrap the result in a try catch. Provide also a finally which is itself wrapped.
    /// It is better to use an extension methods inmost case. This one is to be used n one of these extensions.
    /// When creating extensions methods of this class, you HAVE TO BE CARREFULL TO IMPLEMENT: 
    /// 1) Action
    /// 2) Func[T]
    /// 3) Func[Task]
    /// 4) Func[Task[T]]
    /// if you forget any one, your code may compile but you will do a fire and forget by casting an awaitable call to an action
    /// </summary>
    public static class DispatcherCore
    {
        /// <summary>
        /// Create a task and invoke a callback with a try catch
        /// </summary>
        public static async Task DispatchAndWrapAsyncBase(Action callback, Action<Exception> onErrorAction, Action onfinally = null)
        {
            await Task.Run(() =>
            {
                try
                {
                    callback.Invoke();
                }
                catch (Exception e)
                {
                    onErrorAction?.Invoke(e);
                }
                finally
                {
                    WrapperCore.InvokeFinally(onfinally, onErrorAction);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a task and invoke a callback with a try catch. 
        /// </summary>
        public static async Task<T> DispatchAndWrapAsyncBase<T>(Func<T> callback, Action<Exception> onErrorAction, Action onfinally = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return callback.Invoke();
                }
                catch (Exception e)
                {
                    onErrorAction?.Invoke(e);
                    return default;
                }
                finally
                {
                    WrapperCore.InvokeFinally(onfinally, onErrorAction);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a task and invoke a callback with a try catch
        /// </summary>
        public static async Task DispatchAndWrapAsyncBase(Func<Task> callback, Action<Exception> onErrorAction, Action onfinally = null)
        {
            await Task.Run(
                async () => await callback.WrapAsync(onErrorAction, onfinally).ConfigureAwait(false)).ConfigureAwait(false);
        }


        /// <summary>
        /// Create a task and invoke a callback with a try catch
        /// </summary>
        public static async Task<T> DispatchAndWrapAsyncBase<T>(Func<Task<T>> callback, Action<Exception> onErrorAction, Action onfinally = null)
        {
            return await Task.Run(
                async () => await callback.WrapAsync(onErrorAction, onfinally).ConfigureAwait(false)).ConfigureAwait(false);
        }
    }
}
