using G19ZVetro;
using G19ZVetro.Models;
using G19ZVetro.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Forms = System.Windows.Forms;
using System.Windows.Interop;
using System.IO;
using System.Windows.Threading;

namespace G19ZVetro.UI;

public partial class MainWindow : Window
{
    private const int WM_DEVICECHANGE = 0x0219;
    private const int DBT_DEVICEARRIVAL = 0x8000;
    private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
    private G19Service service;
    private AppSettings settings;
    private Forms.NotifyIcon trayIcon;
    private readonly DispatcherTimer profileRefreshTimer = new();
    private void UpdateKeyboardStatus(bool connected)
    {
        KeyboardStatusText.Text =
            connected
                ? "Keyboard Status: Connected"
                : "Keyboard Status: Not Detected";
    }

    public MainWindow()
    {
        InitializeComponent();

        trayIcon = new Forms.NotifyIcon();

        trayIcon.Text = "G19ZVetro";
        trayIcon.Icon = System.Drawing.SystemIcons.Application;

        var menu = new Forms.ContextMenuStrip();

        menu.Items.Add(
            "Open Settings",
            null,
            (_, _) =>
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            });

        menu.Items.Add(
            "Exit",
            null,
            (_, _) =>
            {
                trayIcon.Visible = false;
                System.Windows.Application.Current.Shutdown();
            });

        trayIcon.ContextMenuStrip = menu;

        trayIcon.DoubleClick += (_, _) =>
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        };

        trayIcon.Visible = true;

        service = ((App)System.Windows.Application.Current).Service;

        settings = SettingsManager.Load();

        M1NameBox.Text =
            settings.Profiles["M1"].Name;
        M1RedBox.Text =
            settings.Profiles["M1"].Red.ToString();

        M1GreenBox.Text =
            settings.Profiles["M1"].Green.ToString();

        M1BlueBox.Text =
            settings.Profiles["M1"].Blue.ToString();

        M2NameBox.Text =
            settings.Profiles["M2"].Name;

        M2RedBox.Text =
            settings.Profiles["M2"].Red.ToString();

        M2GreenBox.Text =
            settings.Profiles["M2"].Green.ToString();

        M2BlueBox.Text =
            settings.Profiles["M2"].Blue.ToString();

        M3NameBox.Text =
            settings.Profiles["M3"].Name;

        M3RedBox.Text =
            settings.Profiles["M3"].Red.ToString();

        M3GreenBox.Text =
            settings.Profiles["M3"].Green.ToString();

        M3BlueBox.Text =
            settings.Profiles["M3"].Blue.ToString();

        UpdateActiveProfileDisplay();

        var g1 = settings
            .Profiles[settings.ActiveProfile]
            .GKeys["G1"];

        foreach (ComboBoxItem item in G1ActionTypeBox.Items)
        {
            if (item.Content?.ToString() == g1.ActionType)
            {
                G1ActionTypeBox.SelectedItem = item;
                break;
            }
        }

        G1ValueBox.Text = g1.Value;
        StartupBox.IsChecked =
            settings.StartWithWindows;
        UpdateKeyboardStatus(service.IsConnected);

        service.ConnectionChanged += connected =>
        {
            Dispatcher.Invoke(() =>
            {
                UpdateKeyboardStatus(connected);
            });
        };
        profileRefreshTimer.Interval = TimeSpan.FromSeconds(1);

        profileRefreshTimer.Tick += (_, _) =>
        {
            UpdateActiveProfileDisplay();
        };

        profileRefreshTimer.Start();
    }

    private void UpdateActiveProfileDisplay()
    {
        var currentSettings = SettingsManager.Load();

        var profile =
            currentSettings.Profiles[
                currentSettings.ActiveProfile];

        ActiveProfileText.Text =
            $"Current: {currentSettings.ActiveProfile} - {profile.Name}";
    }

    protected override void OnPreviewKeyDown(
    System.Windows.Input.KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        System.Diagnostics.Debug.WriteLine(
            $"PREVIEW KEY DOWN: {e.Key}");
    }

    protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        base.OnKeyDown(e);

        System.Diagnostics.Debug.WriteLine(
            $"KEY DOWN: {e.Key}");
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        if (PresentationSource.FromVisual(this) is HwndSource source)
        {
            source.AddHook(WndProc);

            RegisterForRawInput(source.Handle);
        }
    }

    [System.Runtime.InteropServices.StructLayout(
    System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct RAWINPUTDEVICE
    {
        public ushort usUsagePage;
        public ushort usUsage;
        public int dwFlags;
        public IntPtr hwndTarget;
    }

    [System.Runtime.InteropServices.DllImport("User32.dll")]
    private static extern bool RegisterRawInputDevices(
        RAWINPUTDEVICE[] pRawInputDevices,
        uint uiNumDevices,
        uint cbSize);

    private void RegisterForRawInput(IntPtr hwnd)
    {
        RAWINPUTDEVICE[] devices =
        {
        new RAWINPUTDEVICE
        {
            usUsagePage = 0x01,
            usUsage = 0x06, // Keyboard
            dwFlags = 0,
            hwndTarget = hwnd
        }
    };

        RegisterRawInputDevices(
            devices,
            (uint)devices.Length,
            (uint)System.Runtime.InteropServices.Marshal
                .SizeOf<RAWINPUTDEVICE>());
    }

    private void RefreshProfileData()
    {
        settings = SettingsManager.Load();

        UpdateActiveProfileDisplay();

        M1NameBox.Text = settings.Profiles["M1"].Name;
        M2NameBox.Text = settings.Profiles["M2"].Name;
        M3NameBox.Text = settings.Profiles["M3"].Name;

        var g1 = settings
            .Profiles[settings.ActiveProfile]
            .GKeys["G1"];

        foreach (ComboBoxItem item in G1ActionTypeBox.Items)
        {
            if (item.Content?.ToString() == g1.ActionType)
            {
                G1ActionTypeBox.SelectedItem = item;
                break;
            }
        }

        G1ValueBox.Text = g1.Value;
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        RefreshProfileData();
    }

    protected override void OnClosing(
        System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;

        Hide();
    }

    private IntPtr WndProc(
        IntPtr hwnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (msg == 0x00FF) // WM_INPUT
        {
            System.Diagnostics.Debug.WriteLine(
                $"WM_INPUT wParam={wParam}");
        }

        try
        {
            string logPath =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "G19ZVetro",
                    "device.log");

            Directory.CreateDirectory(
                Path.GetDirectoryName(logPath)!);

            File.AppendAllText(
                logPath,
                $"{DateTime.Now}: MSG={msg:X}{Environment.NewLine}");

            if (msg == WM_DEVICECHANGE)
            {
                int eventType = wParam.ToInt32();

                if (eventType == 7)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        service.OnDeviceChange();
                    });
                }
            }
        }
        catch
        {
        }

        return IntPtr.Zero;
    }

    private void Save_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (RefreshRateBox.SelectedItem is ComboBoxItem item)
        {
            settings.RefreshRate =
                int.Parse(item.Content.ToString()!);
        }

        settings.StartWithWindows =
            StartupBox.IsChecked ?? false;

        StartupManager.SetStartup(
            settings.StartWithWindows);

        settings.Profiles["M1"].Name =
            M1NameBox.Text;

        settings.Profiles["M2"].Name =
            M2NameBox.Text;

        settings.Profiles["M3"].Name =
            M3NameBox.Text;

        int.TryParse(M1RedBox.Text, out var m1r);
        int.TryParse(M1GreenBox.Text, out var m1g);
        int.TryParse(M1BlueBox.Text, out var m1b);

        settings.Profiles["M1"].Red =
            Math.Clamp(m1r, 0, 255);
        settings.Profiles["M1"].Green =
            Math.Clamp(m1g, 0, 255);
        settings.Profiles["M1"].Blue =
            Math.Clamp(m1b, 0, 255);

        int.TryParse(M2RedBox.Text, out var m2r);
        int.TryParse(M2GreenBox.Text, out var m2g);
        int.TryParse(M2BlueBox.Text, out var m2b);

        settings.Profiles["M2"].Red =
            Math.Clamp(m2r, 0, 255);
        settings.Profiles["M2"].Green =
            Math.Clamp(m2g, 0, 255);
        settings.Profiles["M2"].Blue =
            Math.Clamp(m2b, 0, 255);

        int.TryParse(M3RedBox.Text, out var m3r);
        int.TryParse(M3GreenBox.Text, out var m3g);
        int.TryParse(M3BlueBox.Text, out var m3b);

        settings.Profiles["M3"].Red =
            Math.Clamp(m3r, 0, 255);
        settings.Profiles["M3"].Green =
            Math.Clamp(m3g, 0, 255);
        settings.Profiles["M3"].Blue =
            Math.Clamp(m3b, 0, 255);

        M1RedBox.Text = settings.Profiles["M1"].Red.ToString();
        M1GreenBox.Text = settings.Profiles["M1"].Green.ToString();
        M1BlueBox.Text = settings.Profiles["M1"].Blue.ToString();

        M2RedBox.Text = settings.Profiles["M2"].Red.ToString();
        M2GreenBox.Text = settings.Profiles["M2"].Green.ToString();
        M2BlueBox.Text = settings.Profiles["M2"].Blue.ToString();

        M3RedBox.Text = settings.Profiles["M3"].Red.ToString();
        M3GreenBox.Text = settings.Profiles["M3"].Green.ToString();
        M3BlueBox.Text = settings.Profiles["M3"].Blue.ToString();

        var activeProfile =
            settings.Profiles[settings.ActiveProfile];

        activeProfile.GKeys["G1"].ActionType =
            (G1ActionTypeBox.SelectedItem as ComboBoxItem)?
                .Content?.ToString()
            ?? "None";

        activeProfile.GKeys["G1"].Value =
            G1ValueBox.Text;

        SettingsManager.Save(settings);

        System.Windows.MessageBox.Show(
            "Settings saved.");

        RefreshProfileData();
    }
    private void ActivateM1_Click(
    object sender,
    RoutedEventArgs e)
    {
        ProfileManager.SetActiveProfile("M1");
        RefreshProfileData();
    }

    private void ActivateM2_Click(
        object sender,
        RoutedEventArgs e)
    {
        ProfileManager.SetActiveProfile("M2");
        RefreshProfileData();
    }

    private void ActivateM3_Click(
        object sender,
        RoutedEventArgs e)
    {
        ProfileManager.SetActiveProfile("M3");
        RefreshProfileData();
    }
}