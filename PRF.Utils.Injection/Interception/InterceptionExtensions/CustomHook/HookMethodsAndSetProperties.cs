using System;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;

namespace PRF.Utils.Injection.Interception.InterceptionExtensions.CustomHook
{
    /// <inheritdoc />
    /// <summary>
    /// Intercept methods but not properties
    /// </summary>
    internal sealed class HookMethodsAndSetProperties : IProxyGenerationHook
    {
        public void MethodsInspected()
        {
        }

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
        {
        }

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            // all methods and properties except getters
            return type
                .GetProperties()
                .All(p => p.GetGetMethod() != methodInfo);
        }
    }
}