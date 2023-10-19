using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace PRF.Utils.Injection.Interception.Helpers
{
    /// <summary>
    /// Helper class for handling asynchronous methods in interceptors.
    /// This class allows you to create generic encapsulations of tasks
    /// </summary>
    internal static class InterceptorAsyncHelpers
    {
        private static readonly MethodInfo _createWrapperMethodInfo = typeof(InterceptorAsyncHelpers)
            .GetMethod(nameof(CreateWrapperTask), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly ConcurrentDictionary<Type, Func<Task, Action<object>, Task>> _taskWrapperCreators
            = new ConcurrentDictionary<Type, Func<Task, Action<object>, Task>>();

        /// <summary>
        /// Allows you to encapsulate a non-generic OR generic task
        /// and add a post await action for each case (like an end trace for example)
        /// </summary>
        public static object InterceptAsync(this Task originalTask,
            IInvocation invocation,
            Action postActionWithoutResult,
            Action<object> postActionWithResult)
        {
            return !invocation.Method.ReturnType.IsGenericType
                ? originalTask.InterceptAsync(postActionWithoutResult)
                : originalTask.InterceptWithResultAsync(invocation.ReturnValue, postActionWithResult);
        }

        /// <summary>
        /// Allows you to encapsulate a non-generic task and add a post await action
        /// (like an end trace for example)
        /// </summary>
        private static async Task InterceptAsync(this Task originalTask, Action postAwaitAction)
        {
            // await the original task
            await originalTask.ConfigureAwait(false);

            // calls the postAwait action immediately
            postAwaitAction.Invoke();
        }


        /// <summary>
        /// Allows you to encapsulate a non-generic task and add a post await
        /// action (like an end trace for example)
        /// </summary>
        private static object InterceptWithResultAsync(this Task originalTask, object invocationReturnValue, Action<object> postAwaitAction)
        {
            /* In the case of asynchronous methods, the most relevant is to wrap the task in another task which after a
             * await will add the end trace with the result. Since it is not possible to cast in Task<T>
             * without explicitly knowing the type of T (or in any case I haven't succeeded without using horrors like
             * dynamic or task creation by reflection), we generate on the fly a creation delegate which we put
             * cached in a ConcurrentDictionary and which will be used for future calls as well.
             * This approach is a compromise (not the absolute best) */
            return GetTaskGenericWrapper(invocationReturnValue.GetType()).Invoke(originalTask, postAwaitAction);
        }

        /// <summary>
        /// Get or create if a delegate wrapping a generic task does not already exist
        /// and adding a post await action
        /// </summary>
        /// <param name="taskType">the type of the task (Task[int] for example)</param>
        /// <returns>the wrapper creation function</returns>
        private static Func<Task, Action<object>, Task> GetTaskGenericWrapper(Type taskType)
        {
            return _taskWrapperCreators.GetOrAdd(taskType,
                type => (Func<Task, Action<object>, Task>)_createWrapperMethodInfo
                    // assign the concrete type currently being handled to the generic creation method
                    .MakeGenericMethod(type.GenericTypeArguments[0])
                    // then create the delegate
                    .CreateDelegate(typeof(Func<Task, Action<object>, Task>)));
        }

        private static Task CreateWrapperTask<T>(Task task, Action<object> postAwaitAction)
        {
            // only serves as a pass-through to retrieve a typed task (this is the MakeGenericMethod
            // which will take care of determining the type at runtime)
            return CreateGenericWrapperTask((Task<T>)task, postAwaitAction);
        }

        private static async Task<T> CreateGenericWrapperTask<T>(Task<T> task, Action<object> postAwaitAction)
        {
            var value = await task.ConfigureAwait(false);
            postAwaitAction.Invoke(value);
            return value;
        }
    }
}