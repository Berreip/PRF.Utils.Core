using System;
using System.Collections.Generic;
using Castle.DynamicProxy;
using PRF.Utils.Injection.Utils;

namespace PRF.Utils.Injection.Containers
{
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
        /// Enregistre un intercepteur dans le container. Il est également possible d'utiliser directement un container.Register
        /// </summary>
        /// <typeparam name="T">le type de l'intercepteur</typeparam>
        /// <param name="lifetime">le comportement du container lors d'une demande d'un intercepteur (transient Vs Singleton)</param>
        void RegisterInterceptor<T>(LifeTime lifetime) where T : class, IInterceptor;

        /// <summary>
        /// Enregistre un intercepteur pré-défini dans le container
        /// </summary>
        /// <param name="predefinedInterceptor">le type de l'intercepteur prédéfini</param>
        /// <param name="lifetime">le comportement du container lors d'une demande d'un intercepteur (transient Vs Singleton)</param>
        void RegisterInterceptor(PredefinedInterceptors predefinedInterceptor, LifeTime lifetime);

        /// <summary>
        /// Intercepte le type demandé
        /// </summary>
        /// <typeparam name="T">le type à intercepter</typeparam>
        IInterceptorFluentBindingDefinition Intercept<T>() where T : class;

        /// <summary>
        /// Intercepte  en fonction d'une lambda
        /// </summary>
        /// <param name="predicate"> le prédicat pour permettre d'intercepter dans différentes situations</param>
        IInterceptorFluentBindingDefinition InterceptAny(Func<Type, bool> predicate);

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
        void RegisterType<TClass>(LifeTime lifetime) where TClass : class;
       
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
        /// Register a decorator (a class that will wrap the type from injection point of view)
        /// </summary>
        /// <typeparam name="TDecorator"> decorator type</typeparam>
        /// <typeparam name="TInterface"> decorated type</typeparam>
        /// <param name="lifetime">the decorator lifetime </param>
        void RegisterDecorator<TDecorator, TInterface>(LifeTime lifetime)
            where TDecorator : class, TInterface
            where TInterface : class;

        /// <summary>
        /// Allow to register or complete a collection of type that will be injected as collection (Ienumerable, IReadOnlyCollection...).
        /// </summary>
        void RegisterOrAppendCollection<T>(LifeTime lifetime, params Type[] typesList) where T : class;

        /// <summary>
        /// Allow to register or complete a collection of type that will be injected as Ienumerable with given instances (no lifetime definition here, it has to be singleton as its the given instances).
        /// </summary>
        void RegisterOrAppendCollectionInstances<T>(params T[] elements) where T : class;
        
        /// <summary>
        /// Renvoie le container d'enregistrement. RECUPERER LE CONTENEUR D'ENREGISTREMENT LORS DES REGISTER NE DOIT ETRE FAIT QUE
        /// DANS DES CAS BIEN SPECIFIQUE (enregistrement d'une factory par exemple)
        /// </summary>
        /// <returns>le container d'enregistrement</returns>
        IInjectionContainer GetRegistrableContainer();
    }

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
}