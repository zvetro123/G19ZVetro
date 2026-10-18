namespace G19ZVetro.Models;

public class SystemStats
{
    public float CpuUsage { get; set; }
    public float CpuTemp { get; set; }

    public float GpuUsage { get; set; }
    public float GpuTemp { get; set; }

    public float RamUsedGb { get; set; }
    public float RamTotalGb { get; set; }

    public float VramUsedGb { get; set; }
    public float VramTotalGb { get; set; }

    public int Fps { get; set; }
}