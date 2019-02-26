using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Castle.DynamicProxy;
using PRF.Utils.Injection.Interception.InterceptionExtensions.CustomHook;
using PRF.Utils.Injection.Utils;
using SimpleInjector;

namespace PRF.Utils.Injection.Interception.InterceptionExtensions
{
    // Extension methods for interceptor registration
    // NOTE: These extension methods can only intercept interfaces, not abstract types.
    internal static class InterceptorExtensions
    {
        public static void InterceptWith<TInterceptor>(this Container container, Func<Type, bool> predicate, InterceptionHookOption hookOption)
            where TInterceptor : class, IInterceptor
        {
            var interceptWith = new InterceptionHelper(e => BuildInterceptorExpression<TInterceptor>(container), predicate, GetProxyGenerationHook(hookOption));
            container.ExpressionBuilt += interceptWith.OnExpressionBuilt;
        }

        private static ProxyGenerationOptions GetProxyGenerationHook(InterceptionHookOption hookOption)
        {
            switch (hookOption)
            {
                case InterceptionHookOption.MethodsAndSetProperties:
                    return new ProxyGenerationOptions(new HookMethodsAndSetProperties());
                case InterceptionHookOption.MethodsOnly:
                    return new ProxyGenerationOptions(new HookMethodsButNoProperties());
                case InterceptionHookOption.InterceptionAttributOnly:
                    return new ProxyGenerationOptions(new HookWithInterceptionAttribute());
                default:
                    throw new ArgumentOutOfRangeException(nameof(hookOption), hookOption, null);
            }
        }

        [DebuggerStepThrough]
        private static Expression BuildInterceptorExpression<TInterceptor>(Container container)
            where TInterceptor : class
        {
            var interceptorRegistration = container.GetRegistration(typeof(TInterceptor));

            if (interceptorRegistration == null)
            {
                throw new ActivationException($@"Interceptor [{typeof(TInterceptor).Name}] should be registered before using interception");
            }
            return interceptorRegistration.BuildExpression();
        }

        private sealed class InterceptionHelper
        {
            private static readonly MethodInfo _createProxyMethod = typeof(Interceptor).GetMethod(nameof(Interceptor.CreateProxy));
            private readonly Func<ExpressionBuiltEventArgs, Expression> _buildInterceptorExpression;
            private readonly Func<Type, bool> _predicate;
            private readonly ProxyGenerationOptions _proxyGenerationOptions;

            public InterceptionHelper(
                Func<ExpressionBuiltEventArgs, Expression> buildInterceptorExpression,
                Func<Type, bool> predicate,
                ProxyGenerationOptions proxyGenerationOptions)
            {
                _buildInterceptorExpression = buildInterceptorExpression;
                _predicate = predicate;
                _proxyGenerationOptions = proxyGenerationOptions;
            }

            [DebuggerStepThrough]
            public void OnExpressionBuilt(object sender, ExpressionBuiltEventArgs e)
            {
                if (!_predicate(e.RegisteredServiceType)) return;

                if (!e.RegisteredServiceType.IsInterface)
                {
                    // NOTE: We can only handle interfaces, because System.Runtime.Remoting.Proxies.RealProxy only supports interfaces.
                    throw new NotSupportedException($@"Can't intercept type {e.RegisteredServiceType.Name} because it is not an interface.");
                }
                e.Expression = BuildProxyExpression(e, _proxyGenerationOptions);
            }
            
            [DebuggerStepThrough]
            private Expression BuildProxyExpression(ExpressionBuiltEventArgs e, ProxyGenerationOptions proxyGenerationOptions)
            {
                var expr = _buildInterceptorExpression(e);

                // Create call to
                // (ServiceType)Interceptor.CreateProxy(Type, IInterceptor, object)
                var proxyExpression =
                    Expression.Convert(
                        Expression.Call(_createProxyMethod,
                            Expression.Constant(e.RegisteredServiceType, typeof(Type)),
                            expr,
                            e.Expression,
                            Expression.Constant(proxyGenerationOptions)),
                        e.RegisteredServiceType);

                if (e.Expression is ConstantExpression && expr is ConstantExpression)
                {
                    return Expression.Constant(CreateInstance(proxyExpression), e.RegisteredServiceType);
                }

                return proxyExpression;
            }

            [DebuggerStepThrough]
            private static object CreateInstance(Expression expression)
            {
                return Expression.Lambda<Func<object>>(expression).Compile()();
            }
        }
    }

    public static class Interceptor
    {
        private static readonly ProxyGenerator _generator = new ProxyGenerator();
        
        /// <summary>
        /// Méthode de création d'un proxy
        /// </summary>
        public static object CreateProxy(Type type, IInterceptor interceptor, object target, ProxyGenerationOptions proxyGenerationOptions)
        {
            return _generator.CreateInterfaceProxyWithTarget(type, target, proxyGenerationOptions, interceptor);
        }
    }
    
}