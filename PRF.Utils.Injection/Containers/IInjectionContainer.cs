using System;
using System.Collections.Generic;
using Castle.DynamicProxy;
using PRF.Utils.Injection.Utils;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace PRF.Utils.Injection.Containers
{
    /// <summary>
    /// Represents an injection container where you can only register commands (useful for the "Register" pre-initialization phase)
    /// </summary>
    public interface IInjectionContainerRegister : IDisposable
    {
        /// <summary>
        /// Event raised during an unregistered type resolution
        /// </summary>
        // ReSharper disable once EventNeverSubscribedTo.Global
        event Action<object, Type> ResolveUnregisteredType;

        /// <summary>
        /// Registers a type in the injection container. We will very preferably use an interface rather than a class to
        /// hide the concrete type created by the container (implies a lot of advantage in the architecture of a project,
        /// you have to try to stick to it).
        /// </summary>
        /// <param name="lifetime">the desired lifespan for this type</param>
        /// <typeparam name="TInterface">the interface type (preferable) under which the concrete type will be registered</typeparam>
        /// <typeparam name="TClass">the concrete type</typeparam>
        void Register<TInterface, TClass>(LifeTime lifetime)
            where TClass : class, TInterface
            where TInterface : class;

        /// <summary>
        /// Register a type in the injection container with an initialization method
        /// </summary>
        void RegisterWithInitializer<TInterface, TClass>(LifeTime lifetime, Action<TClass> initializer)
            where TClass : class, TInterface where TInterface : class;

        /// <summary>
        /// Register a type in the injection container with an initialization method
        /// </summary>
        void RegisterWithInitializer<TInterface1, TInterface2, TClass>(LifeTime lifetime,
            Action<TClass> initializer)
            where TInterface1 : class where TInterface2 : class where TClass : class, TInterface1, TInterface2;

        /// <summary>
        /// Register an interceptor in the container. It is also possible to directly use a container.Register
        /// </summary>
        /// <typeparam name="T">the type of the interceptor</typeparam>
        /// <param name="lifetime">the behavior of the container when requesting an interceptor (transient Vs Singleton)</param>
        void RegisterInterceptor<T>(LifeTime lifetime) where T : class, IInterceptor;

        /// <summary>
        /// Registers a pre-defined interceptor in the container
        /// </summary>
        /// <param name="predefinedInterceptor">the type of the predefined interceptor</param>
        /// <param name="lifetime">the behavior of the container when requesting an interceptor (transient Vs Singleton)</param>
        void RegisterInterceptor(PredefinedInterceptors predefinedInterceptor, LifeTime lifetime);

        /// <summary>
        /// Intercept the requested type
        /// </summary>
        /// <typeparam name="T">the type to intercept</typeparam>
        IInterceptorFluentBindingDefinition Intercept<T>() where T : class;

        /// <summary>
        /// Intercept based on a lambda
        /// </summary>
        /// <param name="predicate"> the predicate to allow interception in different situations</param>
        IInterceptorFluentBindingDefinition InterceptAny(Func<Type, bool> predicate);

        /// <summary>
        /// Registers a type in the injection container behind multiple interfaces (resolving either returns the same type).
        /// </summary>
        /// <param name="lifetime">the desired lifespan for this type</param>
        /// <typeparam name="TInterface1">the first type of the interface under which the concrete type will be registered</typeparam>
        /// <typeparam name="TInterface2">the interface type (preferable) under which the concrete type will be registered</typeparam>
        /// <typeparam name="TClass">the concrete type</typeparam>
        void Register<TInterface1, TInterface2, TClass>(LifeTime lifetime)
            where TClass : class, TInterface1, TInterface2
            where TInterface1 : class
            where TInterface2 : class;

        /// <summary>
        /// Registers a type in the injection container.
        /// </summary>
        void RegisterType<TClass>(LifeTime lifetime) where TClass : class;

        /// <summary>
        /// Registers a type as a singleton in the injection container with a custom type creation function
        /// </summary>
        /// /// <param name="instanceCreator">Function returning the instance of the requested type</param>
        /// <typeparam name="TClass">the concrete type</typeparam>
        void RegisterSingleton<TClass>(Func<TClass> instanceCreator)
            where TClass : class;

        /// <summary>
        /// Registers an instance in the injection container as a Singleton.
        /// </summary>
        /// <typeparam name="TInterface">the interface of the desired type for recording</typeparam>
        /// <param name="instance">the instance to save</param>
        void RegisterInstance<TInterface>(TInterface instance)
            where TInterface : class;

        /// <summary>
        /// Register a decorator (a class that will wrap the type from injection point of view)
        /// </summary>
        /// <typeparam name="TDecorator"> decorator type</typeparam>
        /// <typeparam name="TInterface"> decorated type</typeparam>
        /// <param name="lifetime">the decorator lifetime </param>
        void RegisterDecorator<TDecorator, TInterface>(LifeTime lifetime)
            where TDecorator : class, TInterface
            where TInterface : class;

        /// <summary>
        /// Allow to register or complete a collection of type that will be injected as collection (IEnumerable, IReadOnlyCollection...).
        /// </summary>
        void RegisterOrAppendCollection<T>(LifeTime lifetime, params Type[] typesList) where T : class;

        /// <summary>
        /// Allow to register or complete a collection of type that will be injected as IEnumerable with given instances (no lifetime definition here, it has to be singleton as its the given instances).
        /// </summary>
        void RegisterOrAppendCollectionInstances<T>(params T[] elements) where T : class;

        /// <summary>
        /// Returns the recording container. RECOVERING THE REGISTRATION CONTAINER DURING REGISTER SHOULD ONLY BE DONE
        /// IN VERY SPECIFIC CASES (registration of a factory for example)
        /// </summary>
        /// <returns>the recording container</returns>
        IInjectionContainer GetRegistrableContainer();
    }

    /// <summary>
    /// Represents an injection container that encapsulates and hides the injection technique used.
    /// All main components must be registered in this container.
    /// </summary>
    public interface IInjectionContainer : IInjectionContainerRegister
    {
        /// <summary>
        /// Returns an instance of the requested type (which must have previously been registered in the container).
        /// </summary>
        /// <typeparam name="TInterface">the requested type </typeparam>
        /// <returns>an instance (unique or not depending on the record) of the requested type</returns>
        TInterface Resolve<TInterface>() where TInterface : class;

        /// <summary>
        /// Returns an instance of the requested type (which must have previously been registered in the container).
        /// </summary>
        /// <typeparam name="T">the requested type </typeparam>
        /// <returns>an instance (unique or not depending on the record) of the requested type</returns>
        T Resolve<T>(Type type) where T : class;

        /// <summary>
        /// Returns an untyped instance of the requested type (which must have previously been registered in the container).
        /// Limit only in exceptional cases (imposed by the framework or very specific cases), and prefer generic signatures
        /// </summary>
        /// <returns>an instance (unique or not depending on the record) of the requested type</returns>
        object Resolve(Type type);

        /// <summary>
        /// Resolves all registered types implementing the provided interface
        /// </summary>
        /// <typeparam name="T">Interface Type</typeparam>
        /// <returns>List of Concrete Instances</returns>
        List<T> ResolveAll<T>() where T : class;
    }
}