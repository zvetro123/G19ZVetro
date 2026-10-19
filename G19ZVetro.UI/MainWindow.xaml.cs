using G19ZVetro;
using G19ZVetro.Models;
using G19ZVetro.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Forms = System.Windows.Forms;
using System.Windows.Interop;
using System.IO;

namespace G19ZVetro.UI;

public partial class MainWindow : Window
{
    private const int WM_DEVICECHANGE = 0x0219;
    private const int DBT_DEVICEARRIVAL = 0x8000;
    private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
    private G19Service service;
    private AppSettings settings;
    private Forms.NotifyIcon trayIcon;

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

        foreach (ComboBoxItem item in RefreshRateBox.Items)
        {
            if (item.Content?.ToString() ==
                settings.RefreshRate.ToString())
            {
                RefreshRateBox.SelectedItem = item;
                break;
            }
        }

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
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        if (PresentationSource.FromVisual(this) is HwndSource source)
        {
            source.AddHook(WndProc);
        }
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

        SettingsManager.Save(settings);

        System.Windows.MessageBox.Show(
            "Settings saved.");
    }
}