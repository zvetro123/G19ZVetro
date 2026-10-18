using Microsoft.Win32;

namespace G19ZVetro.UI;

public static class StartupManager
{
    private const string AppName = "G19ZVetro";

    public static void SetStartup(bool enabled)
    {
        using RegistryKey? key =
            Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run",
                true);

        if (key == null)
            return;

        if (enabled)
        {
            if (Environment.ProcessPath is string path)
            {
                key.SetValue(
                    AppName,
                    path);
            }
        }
        else
        {
            key.DeleteValue(
                AppName,
                false);
        }
    }
}