using CodeSwine_Solo_Public_Lobby.DataAccess;
using CodeSwine_Solo_Public_Lobby.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace CodeSwine_Solo_Public_Lobby {
    public partial class MainWindow : Window {
        private readonly IPTool _iPTool = new IPTool();
        private readonly SettingsLoader _settingsLoader = new SettingsLoader();
        private NetworkInterfaceItem _selectedNetworkInterface;
        private List<IPAddress> _addresses = new List<IPAddress>();
        private List<NetworkInterfaceItem> _nics;

        private bool _lobbyRulesSet = false;
        private bool _lobbyRulesActive = false;

        public MainWindow() {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            FirewallRule.lblAdmin = lblAdmin;
            Init();
        }

        private void Init() {
            lblYourIPAddress.Content += " " + _iPTool.IpAddress + ".";
            _addresses = SettingsLoader.Settings.Ips;
            lsbAddresses.ItemsSource = _addresses;

            _nics = NetworkInterface.GetAllNetworkInterfaces().Select(ni => new NetworkInterfaceItem(ni)).ToList();
            NicBox.ItemsSource = _nics;
            // Had to use First here because the object in the nics array is not the same object as the one in the settings.
            if (SettingsLoader.Settings.NetworkInterface != null)
                NicBox.SelectedItem = _nics.First(ni => ni.NetworkInterface.Id == SettingsLoader.Settings.NetworkInterface.Id);

            SetIpCount();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e) {
            if (IPTool.ValidateIP(txbIpToAdd.Text)) {
                IPAddress newIp = IPAddress.Parse(txbIpToAdd.Text);
                if (!_addresses.Contains(newIp)) {
                    _addresses.Add(newIp);
                    lsbAddresses.Items.Refresh();
                    _settingsLoader.Save();
                    _lobbyRulesSet = false; _lobbyRulesActive = false;
                    FirewallRule.DeleteRules();
                    SetIpCount();
                    UpdateNotActive();
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e) {
            if (lsbAddresses.SelectedIndex != -1) {
                IPAddress removedIp = (IPAddress)lsbAddresses.SelectedItem;

                SettingsLoader.Settings.Ips.Remove(removedIp);
                _addresses.Remove(removedIp);
                lsbAddresses.Items.Refresh();
                _settingsLoader.Save();

                _lobbyRulesSet = false; _lobbyRulesActive = false;
                FirewallRule.DeleteRules();
                SetIpCount();
                UpdateNotActive();
            }
        }

        private void SetIpCount() {
            lblAmountIPs.Content = _addresses.Count() + " IPs whitelisted!";
        }

        private void BtnEnableDisable_Click(object sender, RoutedEventArgs e) {
            SetRules();
        }

        private void BtnEnableDisableInternet_Click(object sender, RoutedEventArgs e) {
            ToggleInternet();
        }

        private void NicBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            _selectedNetworkInterface = (NetworkInterfaceItem)NicBox.SelectedItem;
            ToggleInternetBtnColor();
            SettingsLoader.Settings.NetworkInterface = _selectedNetworkInterface.NetworkInterface;
            _settingsLoader.Save();
        }

        private void SetRules() {
            string remoteAddresses = RangeCalculator.GetRemoteAddresses(_addresses);

            // If the firewall rules aren't set yet.
            if (!_lobbyRulesSet) {
                FirewallRule.CreateInbound(remoteAddresses, true, false);
                FirewallRule.CreateOutbound(remoteAddresses, true, false);
                _lobbyRulesActive = true;
                _lobbyRulesSet = true;
                UpdateActive();
                return;
            }

            // If they are set but not enabled.
            if (_lobbyRulesSet && !_lobbyRulesActive) {
                FirewallRule.CreateInbound(remoteAddresses, true, true);
                FirewallRule.CreateOutbound(remoteAddresses, true, true);
                _lobbyRulesActive = true;
                UpdateActive();
                return;
            }

            // If they are active and set.
            if (_lobbyRulesActive && _lobbyRulesSet) {
                FirewallRule.CreateInbound(remoteAddresses, false, true);
                FirewallRule.CreateOutbound(remoteAddresses, false, true);
                UpdateNotActive();
                _lobbyRulesActive = false;
            }
        }

        private void ToggleInternet() {
            if (_selectedNetworkInterface.IsEnabled) {
                _selectedNetworkInterface.Disable();
            }
            else {
                _selectedNetworkInterface.Enable();
            }
            ToggleInternetBtnColor();
        }

        private void UpdateNotActive() {
            btnEnableDisable.Background = ColorBrush.Red;
            btnEnableDisable.BorderBrush = ColorBrush.Red;
            image4.Source = new BitmapImage(new Uri("/CodeSwine-Solo_Public_Lobby;component/ImageResources/unlocked.png", UriKind.Relative));
            lblLock.Content = "Rules not active." + Environment.NewLine + "Click to activate!";
        }

        private void UpdateActive() {
            btnEnableDisable.Background = ColorBrush.Green;
            btnEnableDisable.BorderBrush = ColorBrush.Green;
            image4.Source = new BitmapImage(new Uri("/CodeSwine-Solo_Public_Lobby;component/ImageResources/locked.png", UriKind.Relative));
            lblLock.Content = "Rules active." + Environment.NewLine + "Click to deactivate!";
        }

        private void ToggleInternetBtnColor() {
            if (_selectedNetworkInterface.IsEnabled) {
                btnEnableDisableInternet.Background = ColorBrush.Red;
                btnEnableDisableInternet.BorderBrush = ColorBrush.Red;
                lblInternet.Text = "Internet enabled" + Environment.NewLine + "Click to deactivate!";
            }
            else {
                btnEnableDisableInternet.Background = ColorBrush.Green;
                btnEnableDisableInternet.BorderBrush = ColorBrush.Green;
                lblInternet.Text = "Internet disabled" + Environment.NewLine + "Click to activate!";
            }
        }

        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
        [In] IntPtr hWnd,
        [In] int id,
        [In] uint fsModifiers,
        [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private HwndSource _source;
        private const int HOTKEY_ID = 9000;

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey();
        }

        protected override void OnClosed(EventArgs e) {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
            FirewallRule.DeleteRules();
            foreach (NetworkInterfaceItem nic in _nics) {
                nic.Enable();
            }
            base.OnClosed(e);
        }

        private void RegisterHotKey() {
            var helper = new WindowInteropHelper(this);
            const uint VK_F12 = 0x7B;
            const uint MOD_CTRL = 0x0002;
            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CTRL, VK_F12)) {

            }
        }

        private void UnregisterHotKey() {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            const int WM_HOTKEY = 0x0312;
            switch (msg) {
                case WM_HOTKEY:
                    switch (wParam.ToInt32()) {
                        case HOTKEY_ID:
                            OnHotKeyPressed();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed() {
            ToggleInternet();
            System.Media.SystemSounds.Hand.Play();
        }
    }
}
