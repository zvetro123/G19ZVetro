using System.Text.Json;
using G19ZVetro.Models;
using System.IO;

namespace G19ZVetro;

public static class SettingsManager
{
    private const string FileName = "settings.json";

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