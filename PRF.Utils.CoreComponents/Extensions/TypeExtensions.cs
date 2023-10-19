using System;
using System.Collections.Generic;
using System.Reflection;

// ReSharper disable UnusedMember.Global

namespace PRF.Utils.CoreComponents.Extensions
{
    /// <summary>
    /// 'Type' extensions. Allows, among other things, to simplify certain operations
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns the list of public properties of the type and parent types of the requested type (useful in the case of an interface for
        /// know properties of parent interfaces)
        /// </summary>
        /// <param name="type">the type whose property hierarchy we wish to know</param>
        /// <returns>the list of public properties</returns>
        public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type)
        {
            if (!type.IsInterface)
            {
                // case of a concrete type == no need to traverse the hierarchy
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
        /// Indicates whether the type is derived from a generic base class (and only from a class)
        /// </summary>
        /// <param name="currentType">the source type of which we want to know if it is derivable from a generic type</param>
        /// <param name="genericType">the target generic type</param>
        /// <returns>true if the source type derives from the target generic type</returns>
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