namespace G19ZVetro.Models;

public class AppSettings
{
    public int RefreshRate { get; set; } = 250;

    public bool StartWithWindows { get; set; }

    public string ActiveProfile { get; set; } = "M1";

    public Dictionary<string, Profile> Profiles { get; set; } =
        new()
        {
            ["M1"] = new Profile
            {
                Name = "Development",
                Red = 0,
                Green = 255,
                Blue = 0
            },

            ["M2"] = new Profile
            {
                Name = "Gaming",
                Red = 255,
                Green = 0,
                Blue = 0
            },

            ["M3"] = new Profile
            {
                Name = "Streaming",
                Red = 128,
                Green = 0,
                Blue = 255
            }
        };
}