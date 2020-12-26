using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PRF.Utils.CoreComponents.Async
{
    /// <summary>
    /// Class that start a new task and wrap the result in a try catch. Provide also a finally which is itself wrapped.
    /// It is better to use an extension methods inmost case. This one is to be used n one of these extensions.
    /// When creating extensions methods of this class, you have to be carrefull to implement Task and Func[Task]    /// 
    /// </summary>
    public static class WrapperCore
    {
        /// <summary>
        /// Create a task and dispatch a callwith a try catch. 
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
                    InvokeFinally(onfinally, onErrorAction);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a task and dispatch a callwith a try catch.
        /// </summary>
        public static async Task DispatchAndWrapAsyncBase(Func<Task> callback, Action<Exception> onErrorAction, Action onfinally = null)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await callback.Invoke();
                }
                catch (Exception e)
                {
                    onErrorAction?.Invoke(e);
                }
                finally
                {
                    InvokeFinally(onfinally, onErrorAction);
                }
            }).ConfigureAwait(false);
        }

        private static void InvokeFinally(Action onFinally, Action<Exception> onErrorAction)
        {
            try
            {
                onFinally?.Invoke();
            }
            catch (Exception e)
            {
                onErrorAction(e);
            }
        }
    }
}
