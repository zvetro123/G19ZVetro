using G19ZVetro.Models;
using System.Drawing;

namespace G19ZVetro.Rendering;

public class DashboardRenderer
{
    public Bitmap RenderDashboardPage(SystemStats stats)
    {
        Bitmap bitmap = new Bitmap(320, 240);

        using Graphics g = Graphics.FromImage(bitmap);

        g.Clear(Color.Black);

        using Font fpsFont = new("Arial", 36, FontStyle.Bold);
        using Font labelFont = new("Arial", 14, FontStyle.Bold);
        using Font valueFont = new("Arial", 14);

        // Dynamic colors

        Brush fpsBrush =
            stats.Fps >= 100 ? Brushes.Lime :
            stats.Fps >= 60 ? Brushes.Yellow :
            Brushes.Red;

        Brush cpuTempBrush =
            stats.CpuTemp < 75 ? Brushes.White :
            stats.CpuTemp < 85 ? Brushes.Yellow :
            Brushes.Red;

        Brush gpuTempBrush =
            stats.GpuTemp < 75 ? Brushes.White :
            stats.GpuTemp < 85 ? Brushes.Yellow :
            Brushes.Red;

        float vramPercent =
            stats.VramTotalGb > 0
                ? stats.VramUsedGb / stats.VramTotalGb
                : 0;

        Brush vramBrush =
            vramPercent < 0.80f ? Brushes.White :
            vramPercent < 0.90f ? Brushes.Yellow :
            Brushes.Red;

        float ramPercent =
            stats.RamTotalGb > 0
                ? (stats.RamUsedGb / stats.RamTotalGb) * 100f
                : 0;

        float vramPercentDisplay =
            stats.VramTotalGb > 0
                ? (stats.VramUsedGb / stats.VramTotalGb) * 100f
                : 0;

        // FPS Header
        g.DrawString(
            $"FPS {stats.Fps}",
            fpsFont,
            fpsBrush,
            10,
            5);

        // Divider
        g.DrawLine(
            Pens.DarkGray,
            10,
            55,
            310,
            55);

        // CPU Row
        g.DrawString(
            "CPU",
            labelFont,
            Brushes.Lime,
            10,
            70);

        g.DrawString(
            $"{stats.CpuUsage:F0}%",
            valueFont,
            Brushes.White,
            80,
            70);

        g.DrawString(
            $"{stats.CpuTemp:F0}C",
            valueFont,
            cpuTempBrush,
            160,
            70);

        DrawBar(
            g,
            230,
            73,
            75,
            12,
            stats.CpuUsage,
            Brushes.Lime);

        // GPU Row
        g.DrawString(
            "GPU",
            labelFont,
            Brushes.Cyan,
            10,
            100);

        g.DrawString(
            $"{stats.GpuUsage:F0}%",
            valueFont,
            Brushes.White,
            80,
            100);

        g.DrawString(
            $"{stats.GpuTemp:F0}C",
            valueFont,
            gpuTempBrush,
            160,
            100);

        DrawBar(
            g,
            230,
            103,
            75,
            12,
            stats.GpuUsage,
            Brushes.Cyan);

        // Divider
        g.DrawLine(
            Pens.DarkGray,
            10,
            135,
            310,
            135);

        // RAM Row
        g.DrawString(
            "RAM",
            labelFont,
            Brushes.Orange,
            10,
            150);

        g.DrawString(
            $"{stats.RamUsedGb:F1} / {stats.RamTotalGb:F0} GB",
            valueFont,
            Brushes.White,
            90,
            150);

        DrawBar(
            g,
            230,
            153,
            75,
            12,
            ramPercent,
            Brushes.Orange);

        // VRAM Row
        g.DrawString(
            "VRAM",
            labelFont,
            Brushes.OrangeRed,
            10,
            180);

        g.DrawString(
            $"{stats.VramUsedGb:F1} / {stats.VramTotalGb:F1} GB",
            valueFont,
            vramBrush,
            90,
            180);

        DrawBar(
            g,
            230,
            183,
            75,
            12,
            vramPercentDisplay,
            Brushes.OrangeRed);

        return bitmap;
    }

    private void DrawBar(
        Graphics g,
        int x,
        int y,
        int width,
        int height,
        float percent,
        Brush fillBrush)
    {
        percent = Math.Clamp(percent, 0, 100);

        g.DrawRectangle(
            Pens.Gray,
            x,
            y,
            width,
            height);

        int fillWidth = (int)((width - 1) * (percent / 100f));

        g.FillRectangle(
            fillBrush,
            x + 1,
            y + 1,
            fillWidth,
            height - 2);

    }

    public Bitmap RenderHardwarePage(SystemStats stats)
    {
        Bitmap bitmap = new Bitmap(320, 240);

        using Graphics g = Graphics.FromImage(bitmap);

        g.Clear(Color.Black);

        using Font titleFont = new("Arial", 20, FontStyle.Bold);
        using Font valueFont = new("Arial", 14);

        g.DrawString(
            "HARDWARE",
            titleFont,
            Brushes.Cyan,
            10,
            10);

        g.DrawLine(
            Pens.Gray,
            10,
            45,
            310,
            45);

        g.DrawString(
            $"CPU Usage: {stats.CpuUsage:F0}%",
            valueFont,
            Brushes.White,
            10,
            60);

        g.DrawString(
            $"CPU Temp: {stats.CpuTemp:F0}C",
            valueFont,
            Brushes.White,
            10,
            90);

        g.DrawString(
            $"GPU Usage: {stats.GpuUsage:F0}%",
            valueFont,
            Brushes.White,
            10,
            120);

        g.DrawString(
            $"GPU Temp: {stats.GpuTemp:F0}C",
            valueFont,
            Brushes.White,
            10,
            150);

        return bitmap;
    }

    public Bitmap RenderMemoryPage(SystemStats stats)
    {
        Bitmap bitmap = new Bitmap(320, 240);

        using Graphics g = Graphics.FromImage(bitmap);

        g.Clear(Color.Black);

        using Font titleFont = new("Arial", 20, FontStyle.Bold);
        using Font valueFont = new("Arial", 14);

        g.DrawString(
            "MEMORY",
            titleFont,
            Brushes.Orange,
            10,
            10);

        g.DrawLine(
            Pens.Gray,
            10,
            45,
            310,
            45);

        g.DrawString(
            $"RAM: {stats.RamUsedGb:F1} / {stats.RamTotalGb:F1}",
            valueFont,
            Brushes.White,
            10,
            70);

        g.DrawString(
            $"VRAM: {stats.VramUsedGb:F1} / {stats.VramTotalGb:F1}",
            valueFont,
            Brushes.White,
            10,
            100);

        return bitmap;
    }

    public Bitmap RenderMenu(
    string[] menuItems,
    int selectedIndex)
    {
        Bitmap bitmap = new Bitmap(320, 240);

        using Graphics g = Graphics.FromImage(bitmap);

        g.Clear(Color.Black);

        using Font titleFont = new("Arial", 20, FontStyle.Bold);
        using Font itemFont = new("Arial", 14);

        g.DrawString(
            "MENU",
            titleFont,
            Brushes.Yellow,
            10,
            10);

        for (int i = 0; i < menuItems.Length; i++)
        {
            Brush brush =
                i == selectedIndex
                    ? Brushes.Lime
                    : Brushes.White;

            string prefix =
                i == selectedIndex
                    ? "> "
                    : "  ";

            g.DrawString(
                prefix + menuItems[i],
                itemFont,
                brush,
                10,
                50 + (i * 30));
        }

        return bitmap;
    }

    public Bitmap RenderPerformancePage(SystemStats stats)
    {
        Bitmap bitmap = new Bitmap(320, 240);

        using Graphics g = Graphics.FromImage(bitmap);

        g.Clear(Color.Black);

        using Font titleFont = new("Arial", 20, FontStyle.Bold);
        using Font valueFont = new("Arial", 14);

        g.DrawString(
            "PERFORMANCE",
            titleFont,
            Brushes.Lime,
            10,
            10);

        g.DrawLine(
            Pens.Gray,
            10,
            45,
            310,
            45);

        g.DrawString(
            $"FPS: {stats.Fps}",
            valueFont,
            Brushes.Lime,
            10,
            70);

        g.DrawString(
            $"CPU Load: {stats.CpuUsage:F0}%",
            valueFont,
            Brushes.White,
            10,
            105);

        g.DrawString(
            $"GPU Load: {stats.GpuUsage:F0}%",
            valueFont,
            Brushes.White,
            10,
            140);

        g.DrawString(
            $"RAM Used: {stats.RamUsedGb:F1} GB",
            valueFont,
            Brushes.White,
            10,
            175);

        return bitmap;
    }

    public Bitmap RenderSettingsPage()
    {
        Bitmap bitmap = new Bitmap(320, 240);

        using Graphics g = Graphics.FromImage(bitmap);

        g.Clear(Color.Black);

        using Font titleFont = new("Arial", 20, FontStyle.Bold);
        using Font itemFont = new("Arial", 14);

        g.DrawString(
            "SETTINGS",
            titleFont,
            Brushes.Cyan,
            10,
            10);

        g.DrawLine(
            Pens.Gray,
            10,
            45,
            310,
            45);

        g.DrawString(
            "Refresh Rate",
            itemFont,
            Brushes.White,
            10,
            70);

        g.DrawString(
            "Theme",
            itemFont,
            Brushes.White,
            10,
            100);

        g.DrawString(
            "About",
            itemFont,
            Brushes.White,
            10,
            130);

        return bitmap;
    }

    public Bitmap RenderSettingsMenu(
     string[] settingsItems,
     int selectedIndex,
     int refreshRate)
    {
        Bitmap bitmap = new Bitmap(320, 240);

        using Graphics g = Graphics.FromImage(bitmap);

        g.Clear(Color.Black);

        using Font titleFont = new("Arial", 20, FontStyle.Bold);
        using Font itemFont = new("Arial", 14);

        g.DrawString(
            "SETTINGS",
            titleFont,
            Brushes.Cyan,
            10,
            10);

        for (int i = 0; i < settingsItems.Length; i++)
        {
            Brush brush =
                i == selectedIndex
                    ? Brushes.Lime
                    : Brushes.White;

            string prefix =
                i == selectedIndex
                    ? "> "
                    : "  ";

            string text = settingsItems[i];

            if (i == 0)
            {
                text = $"Refresh Rate: {refreshRate} ms";
            }
          
            g.DrawString(
                prefix + text,
                itemFont,
                brush,
                10,
                55 + (i * 30));
        }
            return bitmap;
    }

    public Bitmap RenderAboutPage()
    {
        Bitmap bitmap = new Bitmap(320, 240);

        using Graphics g = Graphics.FromImage(bitmap);

        g.Clear(Color.Black);

        using Font titleFont = new("Arial", 20, FontStyle.Bold);
        using Font valueFont = new("Arial", 14);

        g.DrawString(
            "ABOUT",
            titleFont,
            Brushes.Cyan,
            10,
            10);

        g.DrawLine(
            Pens.Gray,
            10,
            45,
            310,
            45);

        g.DrawString(
            "G19ZVetro",
            valueFont,
            Brushes.White,
            10,
            70);

        g.DrawString(
            "Version 1.0",
            valueFont,
            Brushes.White,
            10,
            100);

        g.DrawString(
            "Custom G19 Telemetry",
            valueFont,
            Brushes.White,
            10,
            130);

        return bitmap;
    }
}