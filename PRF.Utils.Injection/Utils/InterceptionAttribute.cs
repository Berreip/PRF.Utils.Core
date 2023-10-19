using System;

namespace PRF.Utils.Injection.Utils
{
    /// <inheritdoc />
    /// <summary>
    /// Interception attribute indicating that we wish to intercept this method or property.
    /// THIS ATTRIBUTE MUST BE PLACED ON THE INTERFACE
    /// </summary>
    /// <remarks>For the interception of properties you must put the attributes
    /// on the get AND on the set independently depending on the needs</remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class InterceptionAttribute : Attribute
    {
    }
}
