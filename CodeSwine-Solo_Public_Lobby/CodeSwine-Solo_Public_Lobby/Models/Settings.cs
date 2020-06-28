using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace CodeSwine_Solo_Public_Lobby.Models {
    public class Settings {
        public List<IPAddress> Ips { get; set; }
        public NetworkInterface NetworkInterface { get; set; }

        public Settings(List<IPAddress> ips, NetworkInterface ni) {
            Ips = ips;
            NetworkInterface = ni;
        }

        public Settings() : this(new List<IPAddress>(), null) { }
    }
}
