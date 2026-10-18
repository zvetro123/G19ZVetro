using G19LCD;
using G19ZVetro;
using G19ZVetro.Rendering;
using G19ZVetro.Telemetry;
using System.Drawing;
using System.Diagnostics;

var lcd = new LCD(1133, 49705);

lcd.OpenDevice();
lcd.StartButtonDebug();

var appSettings = SettingsManager.Load();

int refreshRate =
    appSettings.RefreshRate;

int currentPage = 0;
const int pageCount = 5;

bool inMenu = false;
bool inAboutScreen = false;

string[] menuItems =
{
    "Dashboard",
    "Hardware",
    "Memory",
    "Performance",
    "Settings"
};

bool inSettingsMenu = false;

string[] settingsItems =
{
    "Refresh Rate",
    "About"
};

int settingsIndex = 0;

int menuIndex = 0;

lcd.ButtonPressed += button =>
{
    switch (button)
    {
        case "BTN_LEFT":
            currentPage--;

            if (currentPage < 0)
                currentPage = pageCount - 1;

            Console.WriteLine("Page: " + currentPage);
            break;

        case "BTN_RIGHT":
            currentPage++;

            if (currentPage >= pageCount)
                currentPage = 0;

            Console.WriteLine("Page: " + currentPage);
            break;

        case "BTN_MENU":
            inMenu = true;
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
                    case 0: // Refresh Rate

                        refreshRate =
                            refreshRate == 250
                                ? 500
                                : refreshRate == 500
                                    ? 1000
                                    : 250;

                        appSettings.RefreshRate =
                            refreshRate;

                        SettingsManager.Save(
                            appSettings);

                        break;

                    case 1: // About

                        inAboutScreen = true;

                        break;
                }
            }
            else if (inMenu)
            {
                currentPage = menuIndex;
                inMenu = false;

                if (currentPage == 4)
                {
                    inSettingsMenu = true;
                    settingsIndex = 0;
                }
            }

            break;

        case "BTN_SETTINGS":
            currentPage = 4;
            inMenu = false;
            inSettingsMenu = true;
            settingsIndex = 0;
            break;
    }
};

var monitor = new HardwareMonitor();
var renderer = new DashboardRenderer();
var rtss = new RtssMonitor();

while (true)
{
    var stats = monitor.GetStats();

    stats.Fps = rtss.GetFps();

    Bitmap dashboard;

    if (inAboutScreen)
    {
        dashboard = renderer.RenderAboutPage();
    }
    else if (inSettingsMenu)
    {
        dashboard =
        renderer.RenderSettingsMenu(
            settingsItems,
            settingsIndex,
            refreshRate);
    }
    else if (inMenu)
    {
        dashboard =
            renderer.RenderMenu(
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

            case 4:
                dashboard = renderer.RenderSettingsPage();
                break;

            default:
                dashboard = renderer.RenderDashboardPage(stats);
                break;
        }
    }

    // dashboard.Save("dashboard.png");

    var sw = Stopwatch.StartNew();

    lcd.UpdateScreen(dashboard);

    sw.Stop();

    Thread.Sleep(refreshRate);
}