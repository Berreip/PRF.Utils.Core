using Castle.DynamicProxy;

namespace PRF.Utils.Injection.Utils
{
    /// <summary>
    /// Allows you to link a type to intercept with an interceptor
    /// </summary>
    public interface IInterceptorFluentBindingDefinition
    {
        /// <summary>
        /// The interceptor to apply to the intercept type
        /// </summary>
        void With(PredefinedInterceptors predefinedInterceptor, InterceptionHookOption hookOption = InterceptionHookOption.MethodsAndSetProperties);

        /// <summary>
        /// The interceptor to apply to the intercept type
        /// </summary>
        void With<TInterceptor>(InterceptionHookOption hookOption = InterceptionHookOption.MethodsAndSetProperties) where TInterceptor : class, IInterceptor;
    }
}