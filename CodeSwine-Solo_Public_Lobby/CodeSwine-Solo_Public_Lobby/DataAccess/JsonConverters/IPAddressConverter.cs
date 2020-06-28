using Newtonsoft.Json;
using System;
using System.Net;

namespace CodeSwine_Solo_Public_Lobby.DataAccess.JsonConverters {
    public class IPAddressConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(IPAddress);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            return IPAddress.Parse((string)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WriteValue(value.ToString());
        }
    }
}
