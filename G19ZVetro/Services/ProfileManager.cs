using G19ZVetro.Models;

namespace G19ZVetro.Services;

public static class ProfileManager
{
    public static Profile GetActiveProfile()
    {
        var settings = SettingsManager.Load();

        return settings.Profiles[
            settings.ActiveProfile];
    }

    public static string GetActiveProfileId()
    {
        var settings = SettingsManager.Load();

        return settings.ActiveProfile;
    }

    public static void SetActiveProfile(string profileId)
    {
        var settings = SettingsManager.Load();

        if (!settings.Profiles.ContainsKey(profileId))
            return;

        settings.ActiveProfile = profileId;

        SettingsManager.Save(settings);
    }
}