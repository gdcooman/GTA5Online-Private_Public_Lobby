using System.Net.NetworkInformation;

namespace CodeSwine_Solo_Public_Lobby.Helpers {
    public class NetworkInterfaceItem {
        public NetworkInterface NetworkInterface { get; set; }
        public bool IsEnabled { get; set; }

        public NetworkInterfaceItem(NetworkInterface nic) {
            NetworkInterface = nic;
            IsEnabled = true;
        }

        public void Enable() {
            if (!IsEnabled) {
                Execute($"interface set interface \"{NetworkInterface.Name}\" enable");
                IsEnabled = true;
            }
        }

        public void Disable() {
            if (IsEnabled) {
                Execute($"interface set interface \"{NetworkInterface.Name}\" disable");
                IsEnabled = false;
            }
        }

        private void Execute(string args) {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("netsh", args) {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            };
            System.Diagnostics.Process p = new System.Diagnostics.Process {
                StartInfo = psi
            };
            p.Start();
        }
    }
}
