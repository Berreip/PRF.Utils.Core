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
}