using CodeSwine_Solo_Public_Lobby.DataAccess;
using CodeSwine_Solo_Public_Lobby.Helpers;
using CodeSwine_Solo_Public_Lobby.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace CodeSwine_Solo_Public_Lobby {
    public partial class MainWindow : Window {
        private IPTool iPTool = new IPTool();
        private DaWhitelist whiteList = new DaWhitelist();
        private List<IPAddress> addresses = new List<IPAddress>();
        private MWhitelist mWhitelist = new MWhitelist();

        private bool lobbyRulesSet = false;
        private bool lobbyRulesActive = false;
        private bool internetRuleSet = false;
        private bool internetRuleActive = false;

        public MainWindow() {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            FirewallRule.lblAdmin = lblAdmin;
            Init();
        }

        void Init() {
            lblYourIPAddress.Content += " " + iPTool.IpAddress + ".";
            addresses = DaWhitelist.ReadIPsFromJSON();
            lsbAddresses.ItemsSource = addresses;
            foreach (IPAddress ip in addresses) {
                mWhitelist.Ips.Add(ip.ToString());
            }
            SetIpCount();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            if (IPTool.ValidateIP(txbIpToAdd.Text)) {
                if (!addresses.Contains(IPAddress.Parse(txbIpToAdd.Text))) {
                    addresses.Add(IPAddress.Parse(txbIpToAdd.Text));
                    lsbAddresses.Items.Refresh();
                    mWhitelist.Ips.Add(txbIpToAdd.Text);
                    DaWhitelist.SaveToJson(mWhitelist);
                    lobbyRulesSet = false; lobbyRulesActive = false;
                    internetRuleSet = false; internetRuleActive = false;
                    FirewallRule.DeleteRules();
                    SetIpCount();
                    UpdateNotActive();
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            if (lsbAddresses.SelectedIndex != -1) {
                mWhitelist.Ips.Remove(lsbAddresses.SelectedItem.ToString());
                addresses.Remove(IPAddress.Parse(lsbAddresses.SelectedItem.ToString()));
                lsbAddresses.Items.Refresh();
                DaWhitelist.SaveToJson(mWhitelist);
                lobbyRulesSet = false; lobbyRulesActive = false;
                internetRuleSet = false; internetRuleActive = false;
                FirewallRule.DeleteRules();
                SetIpCount();
                UpdateNotActive();
            }
        }

        private void SetIpCount() {
            lblAmountIPs.Content = addresses.Count() + " IPs whitelisted!";
        }

        private void btnEnableDisable_Click(object sender, RoutedEventArgs e) {
            SetRules();
        }

        private void btnEnableDisableInternet_Click(object sender, RoutedEventArgs e) {
            ToggleInternet();
        }

        void SetRules() {
            string remoteAddresses = RangeCalculator.GetRemoteAddresses(addresses);

            // If the firewall rules aren't set yet.
            if (!lobbyRulesSet) {
                FirewallRule.CreateInbound(remoteAddresses, true, false);
                FirewallRule.CreateOutbound(remoteAddresses, true, false);
                lobbyRulesActive = true;
                lobbyRulesSet = true;
                UpdateActive();
                return;
            }

            // If they are set but not enabled.
            if (lobbyRulesSet && !lobbyRulesActive) {
                FirewallRule.CreateInbound(remoteAddresses, true, true);
                FirewallRule.CreateOutbound(remoteAddresses, true, true);
                lobbyRulesActive = true;
                UpdateActive();
                return;
            }

            // If they are active and set.
            if (lobbyRulesActive && lobbyRulesSet) {
                FirewallRule.CreateInbound(remoteAddresses, false, true);
                FirewallRule.CreateOutbound(remoteAddresses, false, true);
                UpdateNotActive();
                lobbyRulesActive = false;
            }
        }

        void ToggleInternet() {
            if (!internetRuleSet) {
                FirewallRule.CreateInternetBlockRule(true, false);
                internetRuleActive = true;
                internetRuleSet = true;
                ToggleInternetBtnColor();
            }
            else {
                if (!internetRuleActive) {
                    FirewallRule.CreateInternetBlockRule(true, true);
                    internetRuleActive = true;
                    ToggleInternetBtnColor();
                }
                else {
                    FirewallRule.CreateInternetBlockRule(false, true);
                    internetRuleActive = false;
                    ToggleInternetBtnColor();
                }
            }

        }

        void UpdateNotActive() {
            btnEnableDisable.Background = ColorBrush.Red;
            image4.Source = new BitmapImage(new Uri("/CodeSwine-Solo_Public_Lobby;component/ImageResources/unlocked.png", UriKind.Relative));
            lblLock.Content = "Rules not active." + Environment.NewLine + "Click to activate!";
        }

        void UpdateActive() {
            btnEnableDisable.Background = ColorBrush.Green;
            image4.Source = new BitmapImage(new Uri("/CodeSwine-Solo_Public_Lobby;component/ImageResources/locked.png", UriKind.Relative));
            lblLock.Content = "Rules active." + Environment.NewLine + "Click to deactivate!";
        }

        void ToggleInternetBtnColor() {
            if (internetRuleActive) {
                btnEnableDisableInternet.Background = ColorBrush.Green;
                lblInternet.Content = "Internet disabled" + Environment.NewLine + "Click to activate!";
            }
            else {
                btnEnableDisableInternet.Background = ColorBrush.Red;
                lblInternet.Content = "Internet enabled" + Environment.NewLine + "Click to deactivate!";
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
