using System.Text.Json;
using G19ZVetro.Models;
using System.IO;

namespace G19ZVetro;

public static class SettingsManager
{
    private static readonly string SettingsDirectory =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "G19ZVetro");

    private static readonly string FileName =
        Path.Combine(SettingsDirectory, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(FileName))
                return new AppSettings();

            string json = File.ReadAllText(FileName);

            return JsonSerializer.Deserialize<AppSettings>(json)
                   ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        Directory.CreateDirectory(SettingsDirectory);

        string json =
            JsonSerializer.Serialize(
                settings,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        File.WriteAllText(
            FileName,
            json);
    }
}