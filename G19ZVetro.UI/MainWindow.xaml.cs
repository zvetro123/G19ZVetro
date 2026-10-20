using G19ZVetro;
using G19ZVetro.Models;
using G19ZVetro.Services;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

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

    private void LoadRefreshRate()
    {
        foreach (ComboBoxItem item in RefreshRateBox.Items)
        {
            if (item.Content?.ToString() ==
                settings.RefreshRate.ToString())
            {
                RefreshRateBox.SelectedItem = item;
                break;
            }
        }
    }
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

        KeyDown += MainWindow_KeyDown;

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

        StartupBox.IsChecked =
            settings.StartWithWindows;

        UpdateKeyboardStatus(service.IsConnected);

        RefreshProfileData();

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

    private string GetSelectedActionType(string gKeyName)
    {
        System.Windows.Controls.ComboBox? comboBox =
            FindName($"{gKeyName}ActionTypeBox")
                as System.Windows.Controls.ComboBox;

        return
            (comboBox?.SelectedItem as ComboBoxItem)?
                .Content?.ToString()
            ?? "None";
    }

    private void BrowseValue_Click(
       object sender,
       RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button button)
            return;

        if (button.Tag is not string textBoxName)
            return;

        if (FindName(textBoxName) is not System.Windows.Controls.TextBox textBox)
            return;

        string gKeyName =
            GetGKeyNumber(textBoxName);

        string actionType =
            GetSelectedActionType(gKeyName);

        switch (actionType)
        {
            case "Program":
                {
                    var dialog =
                        new Microsoft.Win32.OpenFileDialog();

                    dialog.Filter =
                        "Programs (*.exe)|*.exe|All Files (*.*)|*.*";

                    if (dialog.ShowDialog() == true)
                    {
                        textBox.Text = dialog.FileName;
                    }

                    break;
                }

            case "Folder":
                {
                    using var dialog =
                        new Forms.FolderBrowserDialog();

                    if (dialog.ShowDialog() ==
                        Forms.DialogResult.OK)
                    {
                        textBox.Text = dialog.SelectedPath;
                    }

                    break;
                }

            default:
                {
                    System.Windows.MessageBox.Show(
                        "Select Program or Folder first.");

                    break;
                }
        }
    }

    private static string GetGKeyNumber(string textBoxName)
    {
        return textBoxName.Replace("ValueBox", "");
    }

    private GKeyAction GetGKey(string key)
    {
        return settings
            .Profiles[settings.ActiveProfile]
            .GKeys[key];
    }

    private void LoadGKey(
        string keyName,
        System.Windows.Controls.ComboBox actionBox,
        System.Windows.Controls.TextBox valueBox)
    {
        var gKey = GetGKey(keyName);

        foreach (ComboBoxItem item in actionBox.Items)
        {
            if (item.Content?.ToString() == gKey.ActionType)
            {
                actionBox.SelectedItem = item;
                break;
            }
        }

        valueBox.Text = gKey.Value;
    }

    private void SaveGKey(
        string keyName,
        System.Windows.Controls.ComboBox actionBox,
        System.Windows.Controls.TextBox valueBox)
    {
        var gKey =
            settings.Profiles[settings.ActiveProfile]
                .GKeys[keyName];

        gKey.ActionType =
            (actionBox.SelectedItem as ComboBoxItem)?
                .Content?.ToString()
            ?? "None";

        gKey.Value = valueBox.Text;
    }

    private static Key GetRealKey(
        System.Windows.Input.KeyEventArgs e)
    {
        return e.Key == Key.System
            ? e.SystemKey
            : e.Key;
    }
    private void ExecuteGKey(string gKeyName)
    {
        try
        {
            settings = SettingsManager.Load();

            var profile =
                settings.Profiles[settings.ActiveProfile];

            if (!profile.GKeys.TryGetValue(
                    gKeyName,
                    out var gKey))
            {
                return;
            }

            switch (gKey.ActionType)
            {
                case "Program":
                    if (string.IsNullOrWhiteSpace(gKey.Value))
                        return;

                    System.Diagnostics.Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = gKey.Value,
                            UseShellExecute = true
                        });
                    break;

                case "Website":
                    Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = gKey.Value,
                            UseShellExecute = true
                        });
                    break;

                case "Folder":
                    Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = gKey.Value,
                            UseShellExecute = true
                        });
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"ExecuteGKey error: {ex}");
        }
    }

    private void MainWindow_KeyDown(
        object sender,
        System.Windows.Input.KeyEventArgs e)
    {
        var key = GetRealKey(e);

        switch (key)
        {
            case Key.F1:
                ExecuteGKey("G1");
                break;

            case Key.F2:
                ExecuteGKey("G2");
                break;

            case Key.F3:
                ExecuteGKey("G3");
                break;

            case Key.F4:
                ExecuteGKey("G4");
                break;

            case Key.F5:
                ExecuteGKey("G5");
                break;

            case Key.F6:
                ExecuteGKey("G6");
                break;

            case Key.F7:
                ExecuteGKey("G7");
                break;

            case Key.F8:
                ExecuteGKey("G8");
                break;

            case Key.F9:
                ExecuteGKey("G9");
                break;

            case Key.F10:
                ExecuteGKey("G10");
                break;

            case Key.F11:
                ExecuteGKey("G11");
                break;

            case Key.F12:
                ExecuteGKey("G12");
                break;
        }
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
        LoadRefreshRate();

        UpdateActiveProfileDisplay();

        M1NameBox.Text = settings.Profiles["M1"].Name;
        M2NameBox.Text = settings.Profiles["M2"].Name;
        M3NameBox.Text = settings.Profiles["M3"].Name;

        LoadGKey("G1", G1ActionTypeBox, G1ValueBox);
        LoadGKey("G2", G2ActionTypeBox, G2ValueBox);
        LoadGKey("G3", G3ActionTypeBox, G3ValueBox);
        LoadGKey("G4", G4ActionTypeBox, G4ValueBox);
        LoadGKey("G5", G5ActionTypeBox, G5ValueBox);
        LoadGKey("G6", G6ActionTypeBox, G6ValueBox);
        LoadGKey("G7", G7ActionTypeBox, G7ValueBox);
        LoadGKey("G8", G8ActionTypeBox, G8ValueBox);
        LoadGKey("G9", G9ActionTypeBox, G9ValueBox);
        LoadGKey("G10", G10ActionTypeBox, G10ValueBox);
        LoadGKey("G11", G11ActionTypeBox, G11ValueBox);
        LoadGKey("G12", G12ActionTypeBox, G12ValueBox);
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

        SaveGKey("G1", G1ActionTypeBox, G1ValueBox);
        SaveGKey("G2", G2ActionTypeBox, G2ValueBox);
        SaveGKey("G3", G3ActionTypeBox, G3ValueBox);
        SaveGKey("G4", G4ActionTypeBox, G4ValueBox);
        SaveGKey("G5", G5ActionTypeBox, G5ValueBox);
        SaveGKey("G6", G6ActionTypeBox, G6ValueBox);
        SaveGKey("G7", G7ActionTypeBox, G7ValueBox);
        SaveGKey("G8", G8ActionTypeBox, G8ValueBox);
        SaveGKey("G9", G9ActionTypeBox, G9ValueBox);
        SaveGKey("G10", G10ActionTypeBox, G10ValueBox);
        SaveGKey("G11", G11ActionTypeBox, G11ValueBox);
        SaveGKey("G12", G12ActionTypeBox, G12ValueBox);

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