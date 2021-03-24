using System.Threading.Tasks;
using PRF.Utils.Injection.Containers;

namespace PRF.Utils.Injection.BootStrappers
{
    /// <summary>
    /// Représente l'interface définissant un BootStrapper: il s'agit d'une classe qui va enregistrer dans le container d'injection 
    /// toute la configuration d'un module (la gestion d'erreur, un bloc de services, etc...). Ce mécanisme permet d'injecter ensuite les dépendances 
    /// dont un composant a besoin en n'utilisant que le strict nécessaire au moment où c'est strictement nécessaire (lazy loading).
    /// </summary>
    public interface IBootstrapperCore
    {
        /// <summary>
        /// La phase de pré-initialisation permet d'enregistrer dans la conteneur les classes du module. 
        /// Il ne faut pas en résoudre (voir RAPPEL Initialize). Tous les composants non négligeables doivent être enregistré
        ///  avec une politique de durée de vie.
        /// </summary> 
        ///<param name="container">le container d'injection (en mode enregistrement seulement)</param>
        /// <see cref="InitializeAsync"/>
        void Register(IInjectionContainerRegister container);

        /// <summary>
        /// Resout les modules qui doivent l'être explicitement. Inutile de le faire si la résolution est faite par dépendance. Par contre, 
        /// pour des objet indépendants (superviseur, timers, Assistants..) on peut les résoudre ici.
        /// 
        ///  RAPPEL: la résolution explicite (c'est à dire l'appel à une commande Resolve) est une opération exceptionnelle
        /// qui ne doit concerner QUE les composants totalements indépendants (un récepteur de communication WCF par exemple) ou la racine 
        /// du programme (qui doit déjà exister)
        /// </summary>
        /// <param name="container">le container d'injection</param>
        Task InitializeAsync(IInjectionContainer container);
    }

    /// <summary>
    /// Classe de base des bootstrapper. Donne une implémentation de base de toutes les méthodes sauf Register (qui est toujours utile)
    /// On peut également choisir de directement implémenter IBootstrapperCore si l'on ne souhaite pas ces implémentations de base
    /// </summary>
    /// <see cref="IBootstrapperCore"/>
    public abstract class BootStrapperCore : IBootstrapperCore
    {
        /// <inheritdoc/>
        public abstract void Register(IInjectionContainerRegister container);
        
        /// <inheritdoc />
        public virtual async Task InitializeAsync(IInjectionContainer container)
        {
            // vide si pas overiddé
            await Task.CompletedTask;
        }
    }
}
