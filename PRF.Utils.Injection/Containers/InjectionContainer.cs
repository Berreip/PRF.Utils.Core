using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PRF.Utils.Injection.Utils;
using SimpleInjector;
using SimpleInjector.Diagnostics;

namespace PRF.Utils.Injection.Containers
{
    /// <summary>
    /// Réprésente un conteneur d'injection qui encapsule et masque la technique d'injection utilisée.
    ///  Tout les composants principaux doivent être enregistrés dans ce conteneur.
    /// </summary>
    public interface IInjectionContainer : IInjectionContainerRegister
    {
        /// <summary>
        /// Renvoie une instance du type demandé (qui doit préalablement avoir été enregistré dans le conteneur).
        /// </summary>
        /// <typeparam name="TInterface">le type demandé </typeparam>
        /// <returns>une instance (unique ou non en fonction de l'enregistrement) du type demandé</returns>
        TInterface Resolve<TInterface>() where TInterface : class;

        /// <summary>
        /// Renvoie une instance du type demandé (qui doit préalablement avoir été enregistré dans le conteneur).
        /// </summary>
        /// <typeparam name="T">le type demandé </typeparam>
        /// <returns>une instance (unique ou non en fonction de l'enregistrement) du type demandé</returns>
        T Resolve<T>(Type type) where T : class;


        /// <summary>
        /// Renvoie une instance non typée du type demandé (qui doit préalablement avoir été enregistré dans le conteneur).
        /// A limiter que dans les cas exceptionnels (imposés par le framework ou des cas très particuliers), et lui préférer les signatures génériques
        /// </summary>
        /// <returns>une instance (unique ou non en fonction de l'enregistrement) du type demandé</returns>
        object Resolve(Type type);

        /// <summary>
        /// Résout tous les types enegistrés implementant l'interface fourni
        /// </summary>
        /// <typeparam name="T">Type de L'interface</typeparam>
        /// <returns>Liste des Instances concretes</returns>
        List<T> ResolveAll<T>() where T : class;
    }

    /// <summary>
    /// Représente un conteneur d'injection où l'on ne peut qu'enregistrer des commandes (utile pour la phase de pré-initialisation "Register")
    /// </summary>
    public interface IInjectionContainerRegister : IDisposable
    {
        /// <summary>
        /// Evènement levé lors d'une résolution de type non enregistré
        /// </summary>
        event Action<object, Type> ResolveUnregisteredType;

        /// <summary>
        /// Enregistre un type dans le conteneur d'injection. On utilisera très préférentiellement un interface plutôt qu'une classe pour
        /// masquer le type concret créé par le conteneur (implique beaucoup d'avantage dans l'architecture d'un projet,
        /// il faut essayer de s'y tenir). 
        /// </summary>
        /// <param name="lifetime">la durée de vie souhaité pour ce type</param>
        /// <typeparam name="TInterface">le type de l'interface (préférable) sous lequel sera enregistré le type concret</typeparam>
        /// <typeparam name="TClass">le type concret</typeparam>
        void Register<TInterface, TClass>(LifeTime lifetime)
            where TClass : class, TInterface
            where TInterface : class;


        /// <summary>
        /// Enregistre un type dans le conteneur d'injection avec une méthode d'initialisation
        /// </summary>
        void RegisterWithInitializer<TInterface, TClass>(LifeTime lifetime, Action<TClass> initializer)
            where TClass : class, TInterface where TInterface : class;

        /// <summary>
        /// Enregistre un type dans le conteneur d'injection avec une méthode d'initialisation
        /// </summary>
        void RegisterWithInitializer<TInterface1, TInterface2, TClass>(LifeTime lifetime,
            Action<TClass> initializer)
            where TInterface1 : class where TInterface2 : class where TClass : class, TInterface1, TInterface2;


        /// <summary>
        /// Enregistre un type dans le conteneur d'injection derrière plusieurs interfaces (la résolution de l'un ou de l'autre renvoie le même type). 
        /// </summary>
        /// <param name="lifetime">la durée de vie souhaité pour ce type</param>
        /// <typeparam name="TInterface1">le premier type de l'interface sous lequel sera enregistré le type concret</typeparam>
        /// <typeparam name="TInterface2">le type de l'interface (préférable) sous lequel sera enregistré le type concret</typeparam>
        /// <typeparam name="TClass">le type concret</typeparam>
        void Register<TInterface1, TInterface2, TClass>(LifeTime lifetime)
            where TClass : class, TInterface1, TInterface2
            where TInterface1 : class
            where TInterface2 : class;

        /// <summary>
        /// Enregistre un type dans le conteneur d'injection.
        /// </summary>
        /// <param name="lifetime">la durée de vie souhaité pour ce type</param>
        /// <typeparam name="TClass">le type concret</typeparam>
        /// <remarks> On utilisera très préférentiellement un interface plutôt qu'une classe pour masquer le type concret créé 
        /// par le conteneur (implique beaucoup d'avantage dans l'architecture d'un projet, il faut essayer de s'y tenir)</remarks>
        void RegisterType<TClass>(LifeTime lifetime)
            where TClass : class;

        /// <summary>
        /// Enregistre un type comme singleton dans le conteneur d'injection avec une fonction de création du type personnalisée
        /// </summary>

        /// /// <param name="instancecreator">Fonction retournant l'instance du type demandé</param>
        /// <typeparam name="TClass">le type concret</typeparam>
        void RegisterSingleton<TClass>(Func<TClass> instancecreator)
            where TClass : class;

        /// <summary>
        /// Enregistre une instance dans le conteneur d'injection en tant que Singleton.
        /// </summary>
        /// <typeparam name="TInterface">l'interface du type souhaité pour l'enregistrement</typeparam>
        /// <param name="instance">l'instance à enregistrer</param>
        void RegisterInstance<TInterface>(TInterface instance)
            where TInterface : class;

        /// <summary>
        /// Enregistre le décorateur de simulation
        /// </summary>
        /// <typeparam name="TDecorator"> le type du décorateur</typeparam>
        /// <typeparam name="TInterface"> le type simulé par le décorateur</typeparam>
        /// <param name="lifetime">la durée de vie souhaité pour ce décorateur </param>
        void RegisterSimulation<TDecorator, TInterface>(LifeTime lifetime)
            where TDecorator : class, TInterface
            where TInterface : class;

        /// <summary>
        /// Permet l'enregistrement d'un collection de types implémentant tous le même Type de base
        /// </summary>
        /// <param name="typesList">Liste des types</param>
        /// <typeparam name="T">Type de base derrière lequel les autres types seront enregistrés</typeparam>
        void RegisterCollection<T>(IEnumerable<Type> typesList) where T : class;

        /// <summary>
        /// Permet l'enregistrement d'un collection de types implémentant tous le même Type de base
        /// </summary>
        /// <param name="instanceList">Liste des instances</param>
        /// <typeparam name="T">Type de base derrière lequel les autres types seront enregistrés</typeparam>
        void RegisterCollection<T>(IEnumerable<T> instanceList) where T : class;

        /// <summary>
        /// Renvoie le container d'enregistrement. RECUPERER LE CONTENEUR D'ENREGISTREMENT LORS DES REGISTER NE DOIT ETRE FAIT QUE
        /// DANS DES CAS BIEN SPECIFIQUE (enregistrement d'une factory par exemple)
        /// </summary>
        /// <returns>le container d'enregistrement</returns>
        IInjectionContainer GetRegistrableContainer();


    }

    /// <inheritdoc cref="IInjectionContainer"/>
    public class InjectionContainer : IInjectionContainer
    {
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
            _internalContainer.Register<TInterface, TClass>(GetLifeStyle(lifetime));
        }

        /// <inheritdoc/>
        public void RegisterWithInitializer<TInterface, TClass>(LifeTime lifetime, Action<TClass> initializer)
            where TClass : class, TInterface where TInterface : class
        {
            Register<TInterface, TClass>(lifetime);
            _internalContainer.RegisterInitializer(initializer);
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
            var registration = GetLifeStyle(lifetime).CreateRegistration<TClass>(_internalContainer);
            _internalContainer.AddRegistration(typeof(TInterface1), registration);
            _internalContainer.AddRegistration(typeof(TInterface2), registration);
        }

        /// <inheritdoc/>
        public void RegisterType<TClass>(LifeTime lifetime) where TClass : class
        {
            _internalContainer.Register<TClass>(GetLifeStyle(lifetime));
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
            _internalContainer.RegisterDecorator<TInterface, TDecorator>(GetLifeStyle(lifetime));
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

        public Registration CreateSingletonRegistration<T>() where T : class
        {
            return Lifestyle.Singleton.CreateRegistration<T>(_internalContainer);
        }

        public void AddRegistration<T>(Registration registration) where T : class
        {
            _internalContainer.AddRegistration<T>(registration);
        }

        public void RegisterCollection<T>(List<Registration> registrations) where T : class
        {
            _internalContainer.Collection.Register<T>(registrations);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Lifestyle GetLifeStyle(LifeTime lifetime)
        {
            return lifetime == LifeTime.Singleton ? Lifestyle.Singleton : Lifestyle.Transient;
        }

        public void Dispose()
        {
            _internalContainer?.Dispose();
        }
    }
}
