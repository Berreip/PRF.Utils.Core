using Newtonsoft.Json;

namespace PRF.Utils.CoreComponents.JSON
{
    /// <summary>
    /// Gestionnaire de sérialization et désérialization
    /// </summary>
    public static class JsonSerializerWrapper
    {
        /// <summary>
        /// Désérialise en synchrone un string json en objet de type T
        /// </summary>
        /// <typeparam name="T">le type d'objet à renvoyer</typeparam>
        /// <param name="jsonString">la string à désérialiser (le contenu d'un fichier json par exemple)</param>
        /// <returns>L'objet désérialisé</returns>
        public static T DeserializeFromJson<T>(this string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        /// Sérialise en synchrone un objet en json
        /// </summary>
        /// <typeparam name="T">le type d'objet à sérialiser</typeparam>
        /// <param name="data">l'objet à sérialiser</param>
        /// <returns>la représentation en string de l'objet sérialisé</returns>
        public static string SerializeToJson<T>(this T data)
        {
            return JsonConvert.SerializeObject(data);
        }
    }
}
