using Newtonsoft.Json;
using System;
using System.Net.NetworkInformation;

namespace CodeSwine_Solo_Public_Lobby.DataAccess.JsonConverters {
    class NetworkInterfaceConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return (objectType.IsSubclassOf(typeof(NetworkInterface)) || objectType == typeof(NetworkInterface));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            NetworkInterface nic = null;
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces()) {
                if (ni.Name == (string)reader.Value) {
                    nic = ni;
                }
            }

            return nic;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            string name = ((NetworkInterface)value).Name;
            writer.WriteValue(name);
        }
    }
}
