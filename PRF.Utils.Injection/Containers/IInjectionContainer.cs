using System;
using System.Collections.Generic;

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
}