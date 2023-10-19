using System;
using System.Reflection;
using Castle.DynamicProxy;
using PRF.Utils.Injection.Utils;

namespace PRF.Utils.Injection.Interception.InterceptionExtensions.CustomHook
{
    /// <inheritdoc />
    /// <summary>
    /// Intercepts methods and properties with an InterceptionAttribute attribute
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
