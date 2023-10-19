using Newtonsoft.Json;

namespace PRF.Utils.CoreComponents.JSON
{
    /// <summary>
    /// Serialization and deserialization manager
    /// </summary>
    public static class JsonSerializerWrapper
    {
        /// <summary>
        /// Synchronously deserialize a json string into an object of type T
        /// </summary>
        /// <typeparam name="T">the type of object to return</typeparam>
        /// <param name="jsonString">the string to deserialize (the content of a json file for example)</param>
        /// <returns>The deserialized object</returns>
        public static T DeserializeFromJson<T>(this string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        /// Synchronously serialize an object into json
        /// </summary>
        /// <typeparam name="T">the type of object to serialize</typeparam>
        /// <param name="data">the object to serialize</param>
        /// <returns>the string representation of the serialized object</returns>
        public static string SerializeToJson<T>(this T data)
        {
            return JsonConvert.SerializeObject(data);
        }
    }
}