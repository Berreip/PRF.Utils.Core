using System;
using System.Collections.Generic;
using System.Reflection;

// ReSharper disable UnusedMember.Global

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// Extensions de 'Type'. Permet entre autre de simplifier certaines opération
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Renvoie la liste des propriétés publiques du type et des types parents du type demandé (utile dans le cas d'un interface pour 
        /// connaitres propriétés des interfaces parents)
        /// </summary>
        /// <param name="type">le type dont on souhaite conaitre la hiérarchie de propriété</param>
        /// <returns>la liste des propriétés publiques</returns>
        public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type)
        {
            if (!type.IsInterface)
            {
                // cas d'un type concret == pas besoin de parcourir la hiérarchie
                return type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
            }

            var propertyInfos = new HashSet<PropertyInfo>();
            var considered = new HashSet<Type> { type };
            var queue = new Queue<Type>();
            queue.Enqueue(type);
            while (queue.Count > 0)
            {
                var subType = queue.Dequeue();
                foreach (var subInterface in subType.GetInterfaces())
                {
                    if (!considered.Add(subInterface)) continue;
                    queue.Enqueue(subInterface);
                }

                var typeProperties = subType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in typeProperties)
                {
                    propertyInfos.Add(property);
                }
            }

            return propertyInfos;
        }

        /// <summary>
        /// Indique si le type est dérivé d'une classe générique de base (et uniquement d'une classe)
        /// </summary>
        /// <param name="currentType">le type source dont on souhaite savoir s'il est dérivable d'un type générique</param>
        /// <param name="genericType">le type générique cible</param>
        /// <returns>true si le type source dérive du type générique cible</returns>
        public static bool IsSubclassOfRawGeneric(this Type currentType, Type genericType)
        {
            if (!genericType.IsClass)
            {
                throw new ArgumentException($"ERROR: {nameof(IsSubclassOfRawGeneric)} method :type {genericType.Name} is not a class");
            }
            if (!genericType.IsGenericType)
            {
                throw new ArgumentException($"ERROR: {nameof(IsSubclassOfRawGeneric)} method :type {genericType.Name} is not a generic type");
            }

            while (currentType != null && currentType != typeof(object))
            {
                if (genericType == (currentType.IsGenericType ? currentType.GetGenericTypeDefinition() : currentType))
                {
                    return true;
                }
                currentType = currentType.BaseType;
            }
            return false;
        }
    }
}
