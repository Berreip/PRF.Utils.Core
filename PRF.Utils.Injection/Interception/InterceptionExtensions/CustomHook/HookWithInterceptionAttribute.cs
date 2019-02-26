using System;
using System.Reflection;
using Castle.DynamicProxy;
using PRF.Utils.Injection.Utils;

namespace PRF.Utils.Injection.Interception.InterceptionExtensions.CustomHook
{
    /// <inheritdoc />
    /// <summary>
    /// Intercepte les méthodes et les propriétés possédant un attribut InterceptionAttribute
    /// </summary>
    /// <seealso cref="T:PRF.Utils.Injection.Utils.InterceptionAttribute" />
    internal sealed class HookWithInterceptionAttribute : IProxyGenerationHook
    {
        public void MethodsInspected() { }

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo) { }

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttributes(typeof(InterceptionAttribute), false).Length > 0;
        }
    }
}
