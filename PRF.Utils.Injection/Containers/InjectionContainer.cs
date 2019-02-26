using System;
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
    public sealed class InjectionContainer : IInjectionContainer
    {
        /// <summary>
        /// Dictionnaire de conversion entre un LifeTime externe et l'implémentation SimpleInjector
        /// </summary>
        private static readonly Dictionary<LifeTime, Lifestyle> _convertLifestyles = new Dictionary<LifeTime, Lifestyle>
        {
            {LifeTime.Singleton, Lifestyle.Singleton },
            {LifeTime.Transient, Lifestyle.Transient},
        };

        /// <summary>
        /// Le conteneur d'injection interne utilisé (dans notre cas, un conteneur SimpleInjector)
        /// </summary>
        private readonly Container _internalContainer;

        /// <summary>
        /// Constructeur du conteneur d'injection. Il va s'enregistrer lui même.
        /// </summary>
        public InjectionContainer()
        {
            _internalContainer = new Container();
            _internalContainer.ResolveUnregisteredType += RaiseResolveUnregisteredType;
            // s'enregistre soit mm dans le conteneur afin de pouvoir injecter éventuellement le conteneur
            _internalContainer.RegisterInstance<IInjectionContainer>(this);
        }

        /// <inheritdoc />
        public event Action<object, Type> ResolveUnregisteredType;

        private void RaiseResolveUnregisteredType(object sender, UnregisteredTypeEventArgs e)
        {
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
        public void RegisterCollection<T>(IEnumerable<Type> typesList) where T : class
        {
            _internalContainer.Collection.Register<T>(typesList);
        }

        /// <inheritdoc/>
        public void RegisterCollection<T>(IEnumerable<T> instanceList) where T : class
        {
            _internalContainer.Collection.Register(instanceList);
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
            return (T)_internalContainer.GetInstance(type);
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
                : base(container, predicate) { }
        }

        /// <inheritdoc />
        private sealed class InterceptorFluentBindingDefinition : InterceptorFluentBindingDefinitionBase
        {
            /// <inheritdoc />
            public InterceptorFluentBindingDefinition(Container container, Type typeToIntercept) : base(container, t => t == typeToIntercept)
            {
            }
        }
    }
}
