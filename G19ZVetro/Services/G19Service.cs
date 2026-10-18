using G19LCD;
using G19ZVetro.Rendering;
using G19ZVetro.Telemetry;
using System.Drawing;
using System.Threading;
using System.Windows;

namespace G19ZVetro.Services;

public class G19Service
{
    public void Start()
    {
        Task.Run(() =>
        {
            try
            {
                var lcd = new G19LCD.LCD(1133, 49705);

                lcd.OpenDevice();
                lcd.StartButtonDebug();

                var renderer = new DashboardRenderer();
                var monitor = new HardwareMonitor();

                var settings = SettingsManager.Load();

                int currentPage = 0;
                const int pageCount = 4;

                bool inMenu = false;

                bool inSettingsMenu = false;
                bool inAboutScreen = false;

                string[] settingsItems =
                {
                "Refresh Rate",
                "About"
                };

                int settingsIndex = 0;

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

                            if (inAboutScreen)
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
                            if (inSettingsMenu)
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
                            if (inSettingsMenu)
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
                            if (inSettingsMenu)
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
                            if (!inMenu && !inSettingsMenu && !inAboutScreen)
                            {
                                currentPage--;

                                if (currentPage < 0)
                                    currentPage = pageCount - 1;
                            }
                            break;

                        case "BTN_RIGHT":
                            if (!inMenu && !inSettingsMenu && !inAboutScreen)
                            {
                                currentPage++;

                                if (currentPage >= pageCount)
                                    currentPage = 0;
                            }
                            break;
                    }
                };

                while (true)
                {
                    var stats = monitor.GetStats();

                    Bitmap dashboard;

                    if (inAboutScreen)
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "G19Service Error");
            }
        });
    }
}