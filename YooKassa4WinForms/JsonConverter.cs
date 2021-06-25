using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YooKassa4WinForms
{
    public static class JsonConverter
    {
        private static JsonSerializer JsonSerializer = new JsonSerializer();

        public static void SerializeJson(this Stream stream, object content)
        {
            using (StreamWriter streamWriter = new StreamWriter(stream))
            {
                JsonTextWriter jtw = new JsonTextWriter(streamWriter);
                JsonSerializer.Serialize(jtw, content);
            }
        }

        public static T DeserializeJson<T>(this Stream stream) where T : class
        {
            using (var streamReader = new StreamReader(stream))
            {
                JsonTextReader jtr = new JsonTextReader(streamReader);
                return JsonSerializer.Deserialize<T>(jtr);
            }
        }
    }
}
