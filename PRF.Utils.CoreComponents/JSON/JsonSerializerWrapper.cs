using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PRF.Utils.CoreComponents.JSON
{
    /// <summary>
    /// Gestionnaire de sérialization et désérialization
    /// </summary>
    public static class JsonSerializerWrapper
    {
        private const int BUFFER_SIZE = 4096;

        /// <summary>
        /// Désérialise en asynchrone un fichier json
        /// </summary>
        /// <typeparam name="T">le type d'objet à renvoyer</typeparam>
        /// <param name="file">le fichier json à désérialiser</param>
        /// <returns>la Task générant l'objet désérialisé</returns>
        public static async Task<T> DeserializeAsync<T>(this FileInfo file)
        {
            // si le fichier est plus petit que le buffer, on fait une lecture synchrone
            if (file.Length < BUFFER_SIZE)
            {
                return DeserializeFromJson<T>(File.ReadAllText(file.FullName));
            }
            var sb = new StringBuilder();
            using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, true))
            {
                var buffer = new byte[BUFFER_SIZE];
                int numRead;
                while ((numRead = await fs.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) != 0)
                {
                    sb.Append(Encoding.Unicode.GetString(buffer, 0, numRead));
                }
                return DeserializeFromJson<T>(sb.ToString());
            }
        }

        /// <summary>
        /// Sérialise en asynchrone un objet en json
        /// </summary>
        /// <typeparam name="T">le type de l'objet à sérialiser</typeparam>
        /// <param name="file">le fichier json cible</param>
        /// <param name="data">l'objet à sérialiser</param>
        /// <returns>la Task représentant la fin de la tache</returns>
        public static async Task SerializeAsync<T>(this FileInfo file, T data)
        {
            var stringData = SerializeToJson(data);
            using (var fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 4096, true))
            {
                var encodedText = Encoding.Unicode.GetBytes(stringData);
                await fs.WriteAsync(encodedText, 0, encodedText.Length).ConfigureAwait(false);
            }
        }

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

        /// <summary>
        /// Désérialise en synchrone un string json en objet de type T
        /// </summary>
        /// <typeparam name="T">le type d'objet à renvoyer</typeparam>
        /// <param name="jsonString">la string à désérialiser (le contenu d'un fichier json par exemple)</param>
        /// <returns>L'objet désérialisé</returns>
        [Obsolete("NewtonSoft.Json est bcq plus performant")]
        public static T DeserializeFromJsonWithDataContractJsonSerializer<T>(this string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString)) return default(T);

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(ms);
            }
        }

        /// <summary>
        /// Sérialise en synchrone un objet en json
        /// </summary>
        /// <typeparam name="T">le type d'objet à sérialiser</typeparam>
        /// <param name="data">l'objet à sérialiser</param>
        /// <returns>la représentation en string de l'objet sérialisé</returns>
        [Obsolete("NewtonSoft.Json est bcq plus performant")]
        public static string SerializeToJsonWithDataContractJsonSerializer<T>(this T data) where T : class
        {
            var serializer = new DataContractJsonSerializer(typeof(T));

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, data);
                var array = stream.ToArray();
                return  Encoding.UTF8.GetString(array, 0, array.Length);
            }
        }
    }
}
