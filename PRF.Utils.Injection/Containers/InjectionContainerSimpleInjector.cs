using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using PRF.Utils.Injection.Interception.Helpers;
using PRF.Utils.Injection.Interception.InterceptionExtensions;
using PRF.Utils.Injection.Utils;
using SimpleInjector;
using SimpleInjector.Diagnostics;

namespace PRF.Utils.Injection.Containers
{
    /// <inheritdoc cref="IInjectionContainer"/>
    public sealed class InjectionContainerSimpleInjector : IInjectionContainer
    {
        /// <summary>
        /// Dictionnaire de conversion entre un LifeTime externe et l'implémentation SimpleInjector
        /// </summary>
        private static readonly Dictionary<LifeTime, Lifestyle> _convertLifestyles = new Dictionary<LifeTime, Lifestyle>
        {
            {LifeTime.Singleton, Lifestyle.Singleton},
            {LifeTime.Scoped, Lifestyle.Scoped},
            {LifeTime.Transient, Lifestyle.Transient},
        };

        /// <summary>
        /// Le conteneur d'injection interne utilisé (dans notre cas, un conteneur SimpleInjector)
        /// </summary>
        private readonly Container _internalContainer;

        private readonly HashSet<Type> _collectionRegisteredByTypes = new HashSet<Type>();
        private readonly object _keyCollections = new object();

        /// <summary>
        /// Simple injector wrapper. It will register itself.
        /// </summary>
        public InjectionContainerSimpleInjector()
        {
            _internalContainer = new Container();
            
            // WARNING: Since V5, simple injector do auto-verifying of registered elements
            // while this beaviour could make sense in some environment, it is highly unwanted when talking
            // about windows registered as transient as ALL of them will be resolved at first resolution of any type 
            // and all of them will never be exited. As a result, the application could never be closed...
            // From my point of view, it is not to the container to decide this kind of behaviour. This point of
            // view is not negociable so i do not provide a way to tweak it.
            _internalContainer.Options.EnableAutoVerification = false;

            _internalContainer.ResolveUnregisteredType += RaiseResolveUnregisteredType;
            // s'enregistre soit mm dans le conteneur afin de pouvoir injecter éventuellement le conteneur
            _internalContainer.RegisterInstance<IInjectionContainer>(this);
        }

        /// <inheritdoc />
        public event Action<object, Type> ResolveUnregisteredType;

        private void RaiseResolveUnregisteredType(object sender, UnregisteredTypeEventArgs e)
        {
            // Simple injector does not allow optional injection of empty Ienumerable. This makes sense in 99.99% of usages but in context where plugins could not even exists and where it is normal, it could be a desirable behaviour
            if (typeof(IEnumerable).IsAssignableFrom(e.UnregisteredServiceType))
            {
                var genericArguments = e.UnregisteredServiceType.GetGenericArguments();

                // simple injector use the ResolveUnregisteredType event to resolve Collection, so we have to check if the collection is within registered one in order to call the empty one ONLY if none has been registered
                if (genericArguments.All(o => !_collectionRegisteredByTypes.Contains(o)))
                {
                    var validatorType = typeof(EmptyCollection<>).MakeGenericType(genericArguments);

                    // Register the instance as singleton.
                    e.Register(Lifestyle.Singleton.CreateRegistration(validatorType, _internalContainer));
                    return;
                }
            }

            var handler = ResolveUnregisteredType;
            handler?.Invoke(sender, e.UnregisteredServiceType);
        }

        /// <inheritdoc/>
        public void Register<TInterface, TClass>(LifeTime lifetime) where TClass : class, TInterface where TInterface : class
        {
            _internalContainer.Register<TInterface, TClass>(_convertLifestyles[lifetime]);
        }

        /// <inheritdoc/>
        public void RegisterWithInitializer<TInterface, TClass>(LifeTime lifetime, Action<TClass> initializer)
            where TClass : class, TInterface where TInterface : class
        {
            Register<TInterface, TClass>(lifetime);
            _internalContainer.RegisterInitializer(initializer);
        }

        /// <inheritdoc />
        public void RegisterInterceptor<T>(LifeTime lifetime) where T : class, IInterceptor
        {
            _internalContainer.Register<T>(_convertLifestyles[lifetime]);
        }

        /// <inheritdoc />
        public void RegisterInterceptor(PredefinedInterceptors predefinedInterceptor, LifeTime lifetime)
        {
            var interceptor = predefinedInterceptor.GetMatchingInterceptor();
            _internalContainer.Register(interceptor, interceptor, _convertLifestyles[lifetime]);
        }

        /// <inheritdoc />
        public IInterceptorFluentBindingDefinition Intercept<T>() where T : class
        {
            return new InterceptorFluentBindingDefinition(_internalContainer, typeof(T));
        }

        /// <inheritdoc />
        public IInterceptorFluentBindingDefinition InterceptAny(Func<Type, bool> predicate)
        {
            return new InterceptorFluentBindingDefinitionLambda(_internalContainer, predicate);
        }

        /// <inheritdoc/>
        public void RegisterWithInitializer<TInterface1, TInterface2, TClass>(LifeTime lifetime, Action<TClass> initializer)
            where TInterface1 : class where TInterface2 : class where TClass : class, TInterface1, TInterface2
        {
            Register<TInterface1, TInterface2, TClass>(lifetime);
            _internalContainer.RegisterInitializer(initializer);
        }

        /// <inheritdoc/>
        public void Register<TInterface1, TInterface2, TClass>(LifeTime lifetime)
            where TInterface1 : class where TInterface2 : class where TClass : class, TInterface1, TInterface2
        {
            var registration = _convertLifestyles[lifetime].CreateRegistration<TClass>(_internalContainer);
            _internalContainer.AddRegistration(typeof(TInterface1), registration);
            _internalContainer.AddRegistration(typeof(TInterface2), registration);
        }

        /// <inheritdoc/>
        public void RegisterType<TClass>(LifeTime lifetime) where TClass : class
        {
            _internalContainer.Register<TClass>(_convertLifestyles[lifetime]);
        }
        
        /// <inheritdoc/>
        public void RegisterSingleton<TClass>(Func<TClass> instancecreator) where TClass : class
        {
            _internalContainer.RegisterSingleton(instancecreator);
        }

        /// <inheritdoc/>
        public void RegisterInstance<TInterface>(TInterface instance) where TInterface : class
        {
            _internalContainer.RegisterInstance(instance);
        }

        /// <inheritdoc/>
        public void RegisterSimulation<TDecorator, TInterface>(LifeTime lifetime) where TDecorator : class, TInterface where TInterface : class
        {
            _internalContainer.RegisterDecorator<TInterface, TDecorator>(_convertLifestyles[lifetime]);
        }

        /// <inheritdoc/>
        public void RegisterOrAppendCollection<T>(LifeTime lifetime, params Type[] typesList) where T : class
        {
            lock (_keyCollections)
            {
                if (_collectionRegisteredByTypes.Contains(typeof(T)))
                {
                    foreach (var type in typesList)
                    {
                        _internalContainer.Collection.Append(typeof(T), type, _convertLifestyles[lifetime]);
                    }
                }
                else
                {
                    _internalContainer.Collection.Register<T>(typesList, _convertLifestyles[lifetime]);
                    _collectionRegisteredByTypes.Add(typeof(T));
                }
            }
        }

        /// <inheritdoc/>
        public IInjectionContainer GetRegistrableContainer()
        {
            return this;
        }

        /// <inheritdoc/>
        public TInterface Resolve<TInterface>() where TInterface : class
        {
            return _internalContainer.GetInstance<TInterface>();
        }

        /// <inheritdoc/>
        public T Resolve<T>(Type type) where T : class
        {
            return (T) _internalContainer.GetInstance(type);
        }

        /// <inheritdoc/>
        public object Resolve(Type type)
        {
            return _internalContainer.GetInstance(type);
        }

        /// <inheritdoc/>
        public List<T> ResolveAll<T>() where T : class
        {
            return _internalContainer.GetAllInstances<T>().ToList();
        }

        /// <summary>
        /// La vérification consiste à résoudre tous les types enregistrés de façon à vérifier qu'il n'existe pas de références circulaires non détectées
        /// dans le container SimpleInjector.
        /// </summary>
        /// <remarks>Cette méthode n'est pas exposé depuis les interfaces car elle n'est pas proposé aux consommateurs (les bootstrappers)</remarks>
        /// <returns>Renvoie le diagnostique des Erreurs et Warning sil y en a</returns>
        public string Verify()
        {
            _internalContainer.Verify(VerificationOption.VerifyAndDiagnose);
            var results = Analyzer.Analyze(_internalContainer);

            return results.Any() ? $"{Environment.NewLine}{string.Join(Environment.NewLine, from result in results select result.Description)}" : null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _internalContainer?.Dispose();
        }

        /// <inheritdoc />
        private sealed class InterceptorFluentBindingDefinitionLambda : InterceptorFluentBindingDefinitionBase
        {
            internal InterceptorFluentBindingDefinitionLambda(Container container, Func<Type, bool> predicate)
                : base(container, predicate)
            {
            }
        }

        /// <inheritdoc />
        private sealed class InterceptorFluentBindingDefinition : InterceptorFluentBindingDefinitionBase
        {
            /// <inheritdoc />
            public InterceptorFluentBindingDefinition(Container container, Type typeToIntercept) : base(container, t => t == typeToIntercept)
            {
            }
        }

        private sealed class EmptyCollection<T> : IEnumerable<T>
        {
            private readonly IReadOnlyList<T> _backing = Array.Empty<T>();

            public IEnumerator<T> GetEnumerator()
            {
                return _backing.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}