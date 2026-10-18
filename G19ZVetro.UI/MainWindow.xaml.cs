using G19ZVetro;
using G19ZVetro.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Forms = System.Windows.Forms;

namespace G19ZVetro.UI;

public partial class MainWindow : Window
{
    private AppSettings settings;
    private Forms.NotifyIcon trayIcon;

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
    }

    protected override void OnClosing(
        System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;

        Hide();
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