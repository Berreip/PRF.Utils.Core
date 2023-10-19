using System;
using Castle.DynamicProxy;
using PRF.Utils.Injection.Interception.Interceptors;
using PRF.Utils.Injection.Utils;
using SimpleInjector;

namespace PRF.Utils.Injection.Interception.InterceptionExtensions
{
    /// <inheritdoc />
    internal abstract class InterceptorFluentBindingDefinitionBase : IInterceptorFluentBindingDefinition
    {
        private readonly Container _container;
        private readonly Func<Type, bool> _predicate;

        /// <summary>
        /// Constructor with predicate
        /// </summary>
        protected InterceptorFluentBindingDefinitionBase(Container container, Func<Type, bool> predicate)
        {
            _container = container;
            _predicate = predicate;
        }

        /// <inheritdoc />
        public void With(PredefinedInterceptors predefinedInterceptor, InterceptionHookOption hookOption = InterceptionHookOption.MethodsAndSetProperties)
        {
            switch (predefinedInterceptor)
            {
                case PredefinedInterceptors.MethodTraceInterceptor:
                    _container.InterceptWith<MethodTraceInterceptor>(_predicate, hookOption);
                    break;
                case PredefinedInterceptors.TimeWatchInterceptor:
                    _container.InterceptWith<TimeWatchInterceptor>(_predicate, hookOption);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(predefinedInterceptor), predefinedInterceptor, null);
            }
        }

        /// <inheritdoc />
        public void With<TInterceptor>(InterceptionHookOption hookOption = InterceptionHookOption.MethodsAndSetProperties) where TInterceptor : class, IInterceptor
        {
            _container.InterceptWith<TInterceptor>(_predicate, hookOption);
        }
    }
}