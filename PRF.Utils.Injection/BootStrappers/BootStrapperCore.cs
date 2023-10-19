using System.Threading.Tasks;
using PRF.Utils.Injection.Containers;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedParameter.Global

namespace PRF.Utils.Injection.BootStrappers
{
    /// <summary>
    /// Represents the interface defining a BootStrapper: this is a class which will save in the injection container
    /// the entire configuration of a module (error management, a service block, etc...). This mechanism allows you to then inject the dependencies
    /// that a component needs by using only what is strictly necessary at the time when it is strictly necessary (lazy loading).
    /// </summary>
    public interface IBootstrapperCore
    {
        /// <summary>
        /// The pre-initialization phase allows you to save the module's classes in the container.
        /// It should not be resolved (see REMINDER Initialize). All non-negligible components must be recorded
        /// with a lifetime policy.
        /// </summary>
        ///<param name="container">the injection container (in recording mode only)</param>
        /// <see cref="InitializeAsync"/>
        void Register(IInjectionContainerRegister container);

        /// <summary>
        /// Resolves modules that must be resolved explicitly. No need to do this if the resolution is done by dependency. However,
        /// for independent objects (supervisor, timers, Assistants, etc.) we can resolve them here.
        ///
        /// REMINDER: explicit resolution (i.e. calling a Resolve command) is an exceptional operation
        /// which should ONLY concern totally independent components (a WCF communication receiver for example) or the root
        /// of the program (which must already exist)
        /// </summary>
        /// <param name="container">the injection container</param>
        Task InitializeAsync(IInjectionContainer container);
    }

    /// <summary>
    /// Bootstrapper base class. Gives a basic implementation of all methods except Register (which is still useful)
    /// You can also choose to directly implement IBootstrapperCore if you do not want these basic implementations
    /// </summary>
    /// <see cref="IBootstrapperCore"/>
    public abstract class BootStrapperCore : IBootstrapperCore
    {
        /// <inheritdoc/>
        public abstract void Register(IInjectionContainerRegister container);

        /// <inheritdoc />
        public virtual async Task InitializeAsync(IInjectionContainer container)
        {
            // empty if not overloaded
            await Task.CompletedTask;
        }
    }
}