using System;

namespace PRF.Utils.Injection.Utils
{
    /// <inheritdoc />
    /// <summary>
    /// Attribut d'interception indiquant que l'on souhaite intercepter cette méthode ou propriété. 
    /// CET ATTRIBUT DOIT ETRE PLACE SUR L'INTERFACE
    /// </summary>
    /// <remarks>Pour l'interception de propriétés il faut mettre les attributs
    /// sur le get ET sur le set indépendamment en fonction des besoins</remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class InterceptionAttribute : Attribute
    {
    }
}
