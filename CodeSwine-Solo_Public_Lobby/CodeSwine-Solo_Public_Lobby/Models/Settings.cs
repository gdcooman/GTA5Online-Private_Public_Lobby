using System.Collections.Generic;
using System.Net;

namespace CodeSwine_Solo_Public_Lobby.Models {
    public class Settings {
        public List<IPAddress> Ips { get; set; }
        public string GTAVPath { get; set; }

        public Settings() {
            Ips = new List<IPAddress>();
            GTAVPath = "";
        }
    }
}
