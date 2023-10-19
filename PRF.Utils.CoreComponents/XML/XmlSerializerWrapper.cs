using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PRF.Utils.CoreComponents.XML
{
    /// <summary>
    /// Serialization and deserialization manager
    /// </summary>
    public static class XmlSerializerWrapper
    {
        /// <summary>
        /// Synchronously deserialize an XML string into a T type object
        /// </summary>
        /// <typeparam name="T">the type of object to return</typeparam>
        /// <param name="xmlString">the string to deserialize (the content of an xml file for example)</param>
        /// <returns>The deserialized object</returns>
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
        /// Synchronously serialize an object into xml
        /// </summary>
        /// <typeparam name="T">the type of object to serialize</typeparam>
        /// <param name="data">the object to serialize</param>
        /// <returns>the string representation of the serialized object</returns>
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