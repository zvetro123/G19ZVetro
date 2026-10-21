using G19LCD;
using G19ZVetro.Rendering;
using G19ZVetro.Telemetry;
using HidSharp;
using System.Drawing;
using System.Threading;
using System.Windows;

namespace G19ZVetro.Services;

public class G19Service
{
    private bool _hasInitializedConnectionState;

    public bool IsConnected { get; private set; }

    private volatile bool _deviceChanged;

    public event Action<bool>? ConnectionChanged;

    private void SetConnectionState(bool connected)
    {
        if (_hasInitializedConnectionState &&
            IsConnected == connected)
            return;

        _hasInitializedConnectionState = true;

        IsConnected = connected;

        ConnectionChanged?.Invoke(connected);
    }

    private G19LCD.LCD? _lcd;

    public void SetMKeyLeds(byte mask)
    {
        _lcd?.SetMKeyLeds(mask);
    }

    public void SetBacklightColor(
    byte red,
    byte green,
    byte blue)
    {
        _lcd?.SetBacklightColor(
            red,
            green,
            blue);
    }

    public void OnDeviceChange()
    {
        _deviceChanged = true;

        SetConnectionState(false);
    }

    public void Start()
    {
        SetConnectionState(false);

        Task.Run(() =>
        {
            while (true)
            {
                _lcd = null;
                
                try
                {
                    _lcd = new G19LCD.LCD(1133, 49705);

                    _lcd.OpenDevice();

                    var settings = SettingsManager.Load();

                    var profile =
                        settings.Profiles[
                            settings.ActiveProfile];

                    SetBacklightColor(
                        (byte)profile.Red,
                        (byte)profile.Green,
                        (byte)profile.Blue);

                    switch (settings.ActiveProfile)
                    {
                        case "M1":
                            SetMKeyLeds(0x80);
                            break;

                        case "M2":
                            SetMKeyLeds(0x40);
                            break;

                        case "M3":
                            SetMKeyLeds(0x20);
                            break;
                    }

                    SetConnectionState(true);

                    RunDisplayLoop(_lcd);
                }
                catch (Exception)
                {
                    SetConnectionState(false);
                }
                finally
                {
                    try
                    {
                        _lcd?.CloseDevice();
                    }
                    catch
                    {
                    }
                }
                Thread.Sleep(5000);
            }
        });
    }

    private void RunDisplayLoop(G19LCD.LCD lcd)
    {
        var renderer = new DashboardRenderer();
        var monitor = new HardwareMonitor();

        var settings = SettingsManager.Load();

        int currentPage = 0;
        const int pageCount = 4;
        const int profileCount = 3;

        bool inMenu = false;
        bool inSettingsMenu = false;
        bool inAboutScreen = false;
        bool inProfilesMenu = false;

        string[] settingsItems =
        {
            "Refresh Rate",
            "Profiles",
            "About"
        };
        int settingsIndex = 0;

        string[] GetProfileItems()
        {
            settings = SettingsManager.Load();

            return new[]
            {
                $"M1 - {settings.Profiles["M1"].Name}",
                $"M2 - {settings.Profiles["M2"].Name}",
                $"M3 - {settings.Profiles["M3"].Name}"
            };
        }
        int profileIndex = 0;

        string[] menuItems =
        {
            "Dashboard",
            "Hardware",
            "Memory",
            "Performance",
            "Settings"
        };

        int menuIndex = 0;
        lcd.ButtonPressed += button =>
        {
            switch (button)
            {
                case "BTN_MENU":
                    inMenu = true;
                    break;

                case "BTN_SETTINGS":
                    inMenu = false;
                    inAboutScreen = false;
                    inSettingsMenu = true;
                    settingsIndex = 0;
                    break;

                case "BTN_BACK":

                    if (inProfilesMenu)
                    {
                        inProfilesMenu = false;
                        inSettingsMenu = true;
                    }
                    else if (inAboutScreen)
                    {
                        inAboutScreen = false;
                        inSettingsMenu = true;
                    }
                    else if (inSettingsMenu)
                    {
                        inSettingsMenu = false;
                        currentPage = 0;
                    }
                    else
                    {
                        inMenu = false;
                        currentPage = 0;
                    }

                    break;

                case "BTN_UP":

                    if (inProfilesMenu)
                    {
                        profileIndex--;

                        if (profileIndex < 0)
                            profileIndex = profileCount - 1;
                    }
                    else if (inSettingsMenu)
                    {
                        settingsIndex--;

                        if (settingsIndex < 0)
                            settingsIndex = settingsItems.Length - 1;
                    }
                    else if (inMenu)
                    {
                        menuIndex--;

                        if (menuIndex < 0)
                            menuIndex = menuItems.Length - 1;
                    }
                    break;

                case "BTN_DOWN":

                    if (inProfilesMenu)
                    {
                        profileIndex++;

                        if (profileIndex >= profileCount)
                            profileIndex = 0;
                    }
                    else if (inSettingsMenu)
                    {
                        settingsIndex++;

                        if (settingsIndex >= settingsItems.Length)
                            settingsIndex = 0;
                    }
                    else if (inMenu)
                    {
                        menuIndex++;

                        if (menuIndex >= menuItems.Length)
                            menuIndex = 0;
                    }
                    break;

                case "BTN_OK":
                    if (inProfilesMenu)
                    {
                        switch (profileIndex)
                        {
                            case 0:
                                ProfileManager.SetActiveProfile("M1");
                                break;

                            case 1:
                                ProfileManager.SetActiveProfile("M2");
                                break;

                            case 2:
                                ProfileManager.SetActiveProfile("M3");
                                break;
                        }
                        settings = SettingsManager.Load();

                        var profile =
                            settings.Profiles[
                                settings.ActiveProfile];

                        SetBacklightColor(
                            (byte)profile.Red,
                            (byte)profile.Green,
                            (byte)profile.Blue);

                        switch (settings.ActiveProfile)
                        {
                            case "M1":
                                SetMKeyLeds(0x80);
                                break;

                            case "M2":
                                SetMKeyLeds(0x40);
                                break;

                            case "M3":
                                SetMKeyLeds(0x20);
                                break;
                        }
                        settings = SettingsManager.Load();

                        profileIndex = settings.ActiveProfile switch
                        {
                            "M2" => 1,
                            "M3" => 2,
                            _ => 0
                        };
                    }
                    else if (inSettingsMenu)
                    {
                        switch (settingsIndex)
                        {
                            case 0:
                                settings.RefreshRate =
                                    settings.RefreshRate == 250
                                        ? 500
                                        : settings.RefreshRate == 500
                                            ? 1000
                                            : 250;

                                SettingsManager.Save(settings);
                                break;

                            case 1:
                                inSettingsMenu = false;
                                inProfilesMenu = true;

                                profileIndex = settings.ActiveProfile switch
                                {
                                    "M2" => 1,
                                    "M3" => 2,
                                    _ => 0
                                };

                                break;

                            case 2:
                                inSettingsMenu = false;
                                inAboutScreen = true;
                                break;
                        }
                    }
                    else if (inMenu)
                    {
                        if (menuIndex == 4)
                        {
                            inSettingsMenu = true;
                            settingsIndex = 0;
                        }
                        else
                        {
                            currentPage = menuIndex;
                        }

                        inMenu = false;
                    }
                    break;

                case "BTN_LEFT":
                    if (!inMenu &&
                        !inSettingsMenu &&
                        !inAboutScreen &&
                        !inProfilesMenu)
                    {
                        currentPage--;

                        if (currentPage < 0)
                            currentPage = pageCount - 1;
                    }
                    break;

                case "BTN_RIGHT":
                    if (!inMenu &&
                        !inSettingsMenu &&
                        !inAboutScreen &&
                        !inProfilesMenu)
                    {
                        currentPage++;

                        if (currentPage >= pageCount)
                            currentPage = 0;
                    }
                    break;
            }
        };

        while (!_deviceChanged)
        {
            var stats = monitor.GetStats();

            Bitmap dashboard;

            if (inProfilesMenu)
            {
                dashboard = renderer.RenderMenu(
                    GetProfileItems(),
                    profileIndex);
            }
            else if (inAboutScreen)
            {
                dashboard = renderer.RenderAboutPage();
            }
            else if (inSettingsMenu)
            {
                dashboard = renderer.RenderSettingsMenu(
                    settingsItems,
                    settingsIndex,
                    settings.RefreshRate);
            }
            else if (inMenu)
            {
                dashboard = renderer.RenderMenu(
                    menuItems,
                    menuIndex);
            }
            else
            {
                switch (currentPage)
                {
                    case 1:
                        dashboard = renderer.RenderHardwarePage(stats);
                        break;

                    case 2:
                        dashboard = renderer.RenderMemoryPage(stats);
                        break;

                    case 3:
                        dashboard = renderer.RenderPerformancePage(stats);
                        break;

                    default:
                        dashboard = renderer.RenderDashboardPage(stats);
                        break;
                }
            }

            lcd.UpdateScreen(dashboard);

            settings = SettingsManager.Load();

            Thread.Sleep(settings.RefreshRate);
        }

        throw new Exception("Device change detected");

    }
}