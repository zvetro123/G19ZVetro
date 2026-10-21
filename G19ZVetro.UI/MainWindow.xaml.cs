using G19ZVetro.Models;
using G19ZVetro.Services;
using HidSharp;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Forms = System.Windows.Forms;

namespace G19ZVetro.UI;

public partial class MainWindow : Window
{
    private const int WM_DEVICECHANGE = 0x0219;
    private G19Service service;
    private AppSettings settings;
    private Forms.NotifyIcon trayIcon;
    private readonly MacroService _macroService = new();
    private readonly G19InputService _inputService =
    new();
    
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

    private void UpdateMKeyLeds(bool recording = false)
    {
        byte mr = recording ? (byte)0x10 : (byte)0x00;

        switch (settings.ActiveProfile)
        {
            case "M1":
                service.SetMKeyLeds((byte)(0x80 | mr));
                break;

            case "M2":
                service.SetMKeyLeds((byte)(0x40 | mr));
                break;

            case "M3":
                service.SetMKeyLeds((byte)(0x20 | mr));
                break;
        }
    }

    protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!_macroService.IsRecording)
        {
            return;
        }

        var key = GetRealKey(e);

        if (key >= Key.F1 &&
            key <= Key.F24)
        {
            return;
        }

        _macroService.CaptureKey(key);

        Debug.WriteLine(
            $"Captured: {key}");
    }

    private void ActivateProfile(string profile)
    {
        ProfileManager.SetActiveProfile(profile);

        settings = SettingsManager.Load();

        var activeProfile =
            settings.Profiles[profile];

        service.SetBacklightColor(
            (byte)activeProfile.Red,
            (byte)activeProfile.Green,
            (byte)activeProfile.Blue);

        UpdateMKeyLeds(_macroService.IsRecording);

        if (Dispatcher.CheckAccess())
        {
            RefreshProfileData();
        }
        else
        {
            Dispatcher.Invoke(RefreshProfileData);
        }
    }

    public MainWindow()
    {
        InitializeComponent();

        _inputService.GKeyPressed += ExecuteGKey;

        _inputService.ProfileButtonPressed += button =>
        {
            switch (button)
            {
                case G19ProfileButton.M1:
                    ActivateProfile("M1");
                    break;

                case G19ProfileButton.M2:
                    ActivateProfile("M2");
                    break;

                case G19ProfileButton.M3:
                    ActivateProfile("M3");
                    break;

                case G19ProfileButton.MR:
                    HandleMacroButton();
                    break;
            }
        };

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

        var activeProfile =
            settings.Profiles[settings.ActiveProfile];

        service.SetBacklightColor(
            (byte)activeProfile.Red,
            (byte)activeProfile.Green,
            (byte)activeProfile.Blue);

        UpdateMKeyLeds(false);

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

        _inputService.Start();

        service.ConnectionChanged += connected =>
        {
            Dispatcher.Invoke(() =>
            {
                UpdateKeyboardStatus(connected);
            });
        };
    }

    private void UpdateActiveProfileDisplay()
    {
        var profile =
            settings.Profiles[
                settings.ActiveProfile];

        ActiveProfileText.Text =
            $"Current: {settings.ActiveProfile} - {profile.Name}";
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

        if (gKey.MacroKeys.Count > 0)
        {
            gKey.ActionType = "Recorded Macro";
            valueBox.Text = string.Join(",", gKey.MacroKeys);
        }
        else
        {
            valueBox.Text = gKey.Value;
        }

        foreach (ComboBoxItem item in actionBox.Items)
        {
            if (item.Content?.ToString() == gKey.ActionType)
            {
                actionBox.SelectedItem = item;
                break;
            }
        }
    }

    private void SaveGKey(
        string keyName,
        System.Windows.Controls.ComboBox actionBox,
        System.Windows.Controls.TextBox valueBox)
    {
        var gKey =
            settings.Profiles[settings.ActiveProfile]
                .GKeys[keyName];

        var selectedType =
            (actionBox.SelectedItem as ComboBoxItem)?
                .Content?.ToString();

        if (selectedType == "Recorded Macro")
        {
            return;
        }

        gKey.ActionType =
            selectedType ?? "None";

        gKey.Value = valueBox.Text;
    }

    private static Key GetRealKey(
        System.Windows.Input.KeyEventArgs e)
    {
        return e.Key == Key.System
            ? e.SystemKey
            : e.Key;
    }

    private void HandleMacroButton()
    {
        if (!_macroService.IsRecording)
        {
            _macroService.StartRecording();

            UpdateMKeyLeds(true);

            Debug.WriteLine(
                "Macro recording enabled");

            return;
        }

        // User pressed MR again before choosing target
        if (_macroService.RecordingTarget == null)
        {
            _macroService.EndRecording();

            UpdateMKeyLeds(false);

            Debug.WriteLine(
                "Macro recording cancelled");

            return;
        }

        settings = SettingsManager.Load();

        var gKey =
            settings.Profiles[
                settings.ActiveProfile]
                .GKeys[
                    _macroService.RecordingTarget];

        gKey.MacroKeys =
            _macroService.EndRecording();

        gKey.ActionType =
            "Recorded Macro";

        gKey.Value = "";

        SettingsManager.Save(settings);

        UpdateMKeyLeds(false);

        Debug.WriteLine(
            $"Saved {gKey.MacroKeys.Count} keys");
    }

    private void ExecuteGKey(string gKeyName)
    {
        Debug.WriteLine($"ExecuteGKey({gKeyName})");
        try
        {
            if (_macroService.IsRecording &&
                _macroService.RecordingTarget == null)
            {
                _macroService.SetTarget(gKeyName);

                Debug.WriteLine(
                    $"Recording macro for {gKeyName}");

                return;
            }

            if (_macroService.IsRecording)
            {
                return;
            }

            settings = SettingsManager.Load();

            var profile =
                settings.Profiles[settings.ActiveProfile];

            if (!profile.GKeys.TryGetValue(
                    gKeyName,
                    out var gKey))
            {
                return;
            }

            if (gKey.MacroKeys.Count > 0)
            {
                Debug.WriteLine(
                    $"Playing macro for {gKeyName}");

                _macroService.Play(gKey.MacroKeys);

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

            #if DEBUG
            File.AppendAllText(
                logPath,
                $"{DateTime.Now}: MSG={msg:X}{Environment.NewLine}");
            #endif

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

        UpdateMKeyLeds(_macroService.IsRecording);

        System.Windows.MessageBox.Show(
            "Settings saved.");



        RefreshProfileData();
    }
    private void ActivateM1_Click(
    object sender,
    RoutedEventArgs e)
    {
        ActivateProfile("M1");
    }

    private void ActivateM2_Click(
        object sender,
        RoutedEventArgs e)
    {
        ActivateProfile("M2");
    }

    private void ActivateM3_Click(
        object sender,
        RoutedEventArgs e)
    {
        ActivateProfile("M3");
    }

}