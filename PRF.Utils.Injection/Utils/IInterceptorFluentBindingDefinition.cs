using Castle.DynamicProxy;

namespace PRF.Utils.Injection.Utils
{
    /// <summary>
    /// Permet de lier un type à intercepter avec un intercepteur
    /// </summary>
    public interface IInterceptorFluentBindingDefinition
    {
        /// <summary>
        /// L'intercepteur à appliquer au type intercepter
        /// </summary>
        void With(PredefinedInterceptors predefinedInterceptor, InterceptionHookOption hookOption = InterceptionHookOption.MethodsAndSetProperties);

        /// <summary>
        /// L'intercepteur à appliquer au type intercepter
        /// </summary>
        void With<TInterceptor>(InterceptionHookOption hookOption = InterceptionHookOption.MethodsAndSetProperties) where TInterceptor : class, IInterceptor;
    }
}