using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace PRF.Utils.CoreComponents.XML
{
    /// <summary>
    /// Gestionnaire de sérialization et désérialization
    /// </summary>
    public static class JsonSerializerWrapper
    {
        private const int BUFFER_SIZE = 4096;

        /// <summary>
        /// Désérialise en asynchrone un fichier xml
        /// </summary>
        /// <typeparam name="T">le type d'objet à renvoyer</typeparam>
        /// <param name="file">le fichier xml à désérialiser</param>
        /// <returns>la Task générant l'objet désérialisé</returns>
        public static async Task<T> DeserializeAsync<T>(this FileInfo file) where T : new()
        {
            // si le fichier est plus petit que le buffer, on fait une lecture synchrone
            if (file.Length < BUFFER_SIZE)
            {
                return DeserializeFromXml<T>(File.ReadAllText(file.FullName));
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
                return DeserializeFromXml<T>(sb.ToString());
            }
        }

        /// <summary>
        /// Sérialise en asynchrone un objet en xml
        /// </summary>
        /// <typeparam name="T">le type de l'objet à sérialiser</typeparam>
        /// <param name="file">le fichier xml cible</param>
        /// <param name="data">l'objet à sérialiser</param>
        /// <returns>la Task représentant la fin de la tache</returns>
        public static async Task SerializeAsync<T>(this FileInfo file, T data)
        {
            var stringData = SerializeToXml(data);
            using (var fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 4096, true))
            {
                var encodedText = Encoding.Unicode.GetBytes(stringData);
                await fs.WriteAsync(encodedText, 0, encodedText.Length).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Désérialise en synchrone un string XML en objet de type T
        /// </summary>
        /// <typeparam name="T">le type d'objet à renvoyer</typeparam>
        /// <param name="xmlString">la string à désérialiser (le contenu d'un fichier xml par exemple)</param>
        /// <returns>L'objet désérialisé</returns>
        public static T DeserializeFromXml<T>(this string xmlString) where T : new()
        {
            if (string.IsNullOrEmpty(xmlString)) return default;

            var deserializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xmlString))
            {
                return (T)deserializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Sérialise en synchrone un objet en xml
        /// </summary>
        /// <typeparam name="T">le type d'objet à sérialiser</typeparam>
        /// <param name="data">l'objet à sérialiser</param>
        /// <returns>la représentation en string de l'objet sérialisé</returns>
        public static string SerializeToXml<T>(this T data)
        {
            var serializerNamespace = new XmlSerializerNamespaces();
            serializerNamespace.Add(string.Empty, string.Empty);

            var serializer = new XmlSerializer(typeof(T));
            var str = new StringBuilder();

            using (var writer = new StringWriter(str))
            using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true }))
            {
                serializer.Serialize(xmlWriter, data, serializerNamespace);
            }
            return str.ToString();
        }
    }
}
