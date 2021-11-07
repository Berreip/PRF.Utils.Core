using System;
using System.Collections.Generic;

namespace PRF.Utils.CoreComponents.Enums
{
    /// <summary>
    /// A class that will convert from string (ignoring case by default) to enum values and fallback to a default given value if not possible 
    /// </summary>
    public sealed class EnumStringConverterWithDefault<T> where T : struct, Enum
    {
        /// <summary>
        /// The default fallback value
        /// </summary>
        public T DefaultValue { get; }
        
        private readonly Dictionary<string, T> _stringToEnum;

        /// <summary>
        /// Create an enum from string converter with the given fallback value and an optional stringComparter (OrdinalIgnoreCase by default)
        /// </summary>
        public EnumStringConverterWithDefault(T defaultValue, StringComparer comparer = null)
        {
            _stringToEnum = new Dictionary<string, T>(comparer ?? StringComparer.OrdinalIgnoreCase);
            DefaultValue = defaultValue;
            foreach (T enumValue in Enum.GetValues(typeof(T)))
            {
                _stringToEnum.Add(enumValue.ToString(), enumValue);
            }
        }
        
        /// <summary>
        /// Convert from string to the matching enum values and fallback to the default value if needed
        /// </summary>
        public T Convert(string stringValue)
        {
            return stringValue != null && _stringToEnum.TryGetValue(stringValue, out var convertedValue) 
                ? convertedValue 
                : DefaultValue;
        }
    }

    /// <summary>
    /// Utilities methods for converting enums
    /// </summary>
    public static class EnumUtils
    {
        /// <summary>
        /// Create an enum from string converter with the given fallback value and an optional stringComparter (OrdinalIgnoreCase by default)
        /// </summary>
        public static EnumStringConverterWithDefault<T> CreateConverterWithDefault<T>(T defaultValue, StringComparer comparer = null) where T : struct, Enum
        {
            return new EnumStringConverterWithDefault<T>(defaultValue, comparer);
        }
    }
}