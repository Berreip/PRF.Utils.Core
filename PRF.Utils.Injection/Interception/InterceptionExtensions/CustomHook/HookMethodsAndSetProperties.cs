using System;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;

namespace PRF.Utils.Injection.Interception.InterceptionExtensions.CustomHook
{
    /// <inheritdoc />
    /// <summary>
    /// Intercepte les méthodes mais pas les propriétés
    /// </summary>
    internal sealed class HookMethodsAndSetProperties : IProxyGenerationHook
    {
        public void MethodsInspected(){}

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo){}

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            // toutes les méthodes et toutes les propriétés sauf les getter
            return type
                .GetProperties()
                .All(p => p.GetGetMethod() != methodInfo);
        }
    }
}
