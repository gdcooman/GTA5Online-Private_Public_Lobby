using CodeSwine_Solo_Public_Lobby.Helpers;
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
                //Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(_path));
            }
        }

        public void Save() {
            File.WriteAllText(_path, JsonConvert.SerializeObject(Settings, _jsonSettings));
            //File.WriteAllText(_path, JsonConvert.SerializeObject(Settings));
        }

        private JsonSerializerSettings GenerateJsonSettings() {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPAddressConverter());
            settings.Formatting = Formatting.Indented;
            return settings;
        }
    }
}
