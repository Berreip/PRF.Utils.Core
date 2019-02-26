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
    internal sealed class HookMethodsButNoProperties : IProxyGenerationHook
    {
        public void MethodsInspected(){}

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo){}

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            return type
                .GetProperties()
                .All(p => 
                    p.GetSetMethod() != methodInfo && p.GetGetMethod() != methodInfo);
        }
    }
}
