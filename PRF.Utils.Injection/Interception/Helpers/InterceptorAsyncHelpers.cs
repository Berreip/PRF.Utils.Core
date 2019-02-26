using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace PRF.Utils.Injection.Interception.Helpers
{
    /// <summary>
    /// Classe d'aide à la manipulation de méthodes asynchrones dans les intercepteurs.
    /// Cette classe permet de créer des encapsulations génériques de taches
    /// </summary>
    internal static class InterceptorAsyncHelpers
    {
        private static readonly MethodInfo _createWrapperMethodInfo = typeof(InterceptorAsyncHelpers)
            .GetMethod(nameof(CreateWrapperTask), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly ConcurrentDictionary<Type, Func<Task, Action<object>, Task>> _taskWrapperCreators
            = new ConcurrentDictionary<Type, Func<Task, Action<object>, Task>>();
        
        /// <summary>
        /// Permet d'encaspuler une tache non générique OU générique
        /// et de rajouter une action post await pour chaque cas (comme une trace de fin par exemple)
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
        /// Permet d'encaspuler une tache non générique et de rajouter une action post await
        /// (comme une trace de fin par exemple)
        /// </summary>
        private static async Task InterceptAsync(this Task originalTask, Action postAwaitAction)
        {
            // await la tache originale
            await originalTask.ConfigureAwait(false);

            // appelle l'action de postAwait dans la foulée
            postAwaitAction.Invoke();
        }


        /// <summary>
        /// Permet d'encaspuler une tache non générique et de rajouter une action post await
        /// (comme une trace de fin par exemple)
        /// </summary>
        private static object InterceptWithResultAsync(this Task originalTask, object invocationReturnValue, Action<object> postAwaitAction)
        {
            /* Dans un cas de méthodes asynchrones, le plus pertinent est de wrapper la task dans une autre task qui après un
             * await va rajouter la trace de fin avec le résultat. Etant donné qu'il n'est pas possible de caster en Task<T>
             * sans connaitre explicitement le type de T (ou en tt cas que je n'ai pas réussi sans utiliser des horreurs comme
             * les dynamic ou la création de tache par reflexion), on génère à la volée un délégué de création que l'on met
             * en cache dans un ConcurrentDictionary et qui servira aux prochains appels également.
             * Cette approche est un compromis (pas tip top) */
            return GetTaskGenericWrapper(invocationReturnValue.GetType()).Invoke(originalTask, postAwaitAction);
        }

        /// <summary>
        /// Récupère ou crée s'il n'existe pas déjà un délégué wrappant une tache générique
        /// et rajoutant une action post await
        /// </summary>
        /// <param name="taskType">le type de la tache (Task[int] par exemple)</param>
        /// <returns>la fonction de création du wrapper</returns>
        private static Func<Task, Action<object>, Task> GetTaskGenericWrapper(Type taskType)
        {
            return _taskWrapperCreators.GetOrAdd(taskType,
                type => (Func<Task, Action<object>, Task>)_createWrapperMethodInfo
                    // assigne le type concret actuellement manipulé à la méthode de création générique
                    .MakeGenericMethod(type.GenericTypeArguments[0])
                    // puis créer le délégué
                    .CreateDelegate(typeof(Func<Task, Action<object>, Task>)));
        }

        private static Task CreateWrapperTask<T>(Task task, Action<object> postAwaitAction)
        {
            // sert seulement de passe plat pour récupérer une task typée (c'est le MakeGenericMethod
            // qui s'occupera d'en déterminer le type au runtime)
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
