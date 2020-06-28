using CodeSwine_Solo_Public_Lobby.DataAccess.JsonConverters;
using CodeSwine_Solo_Public_Lobby.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace CodeSwine_Solo_Public_Lobby.DataAccess {
    public class SettingsLoader {
        private readonly string _path;
        private readonly JsonSerializerSettings _jsonSettings;
        public static Settings Settings { get; set; }

        public SettingsLoader() {
            _path = AppDomain.CurrentDomain.BaseDirectory + "settings.json";
            _jsonSettings = GenerateJsonSettings();
            Load();
        }

        private void Load() {
            if (!File.Exists(_path)) {
                Settings = new Settings();
            }
            else {
                Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(_path), _jsonSettings);
            }
        }

        public void Save() {
            File.WriteAllText(_path, JsonConvert.SerializeObject(Settings, _jsonSettings));
        }

        private JsonSerializerSettings GenerateJsonSettings() {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPAddressConverter());
            settings.Converters.Add(new NetworkInterfaceConverter());
            settings.Formatting = Formatting.Indented;
            return settings;
        }
    }
}
