using LibreHardwareMonitor.Hardware;
using G19ZVetro.Models;

namespace G19ZVetro.Telemetry;

public class HardwareMonitor
{
    private readonly Computer _computer;
    private readonly RtssMonitor _rtss;
    public HardwareMonitor()
    {
        _rtss = new RtssMonitor();

        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true
        };

        _computer.Open();
    }

    public SystemStats GetStats()
    {
        var stats = new SystemStats();

        foreach (var hardware in _computer.Hardware)
        {
            hardware.Update();

            foreach (var subHardware in hardware.SubHardware)
                subHardware.Update();

            // CPU
            if (hardware.HardwareType == HardwareType.Cpu)
            {
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load &&
                        sensor.Name == "CPU Total")
                    {
                        stats.CpuUsage = sensor.Value ?? 0;
                    }

                    if (sensor.SensorType == SensorType.Temperature &&
                        sensor.Name == "CPU Package")
                    {
                        stats.CpuTemp = sensor.Value ?? 0;
                    }
                }
            }

            // GPU
            if (hardware.HardwareType == HardwareType.GpuNvidia)
            {
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load &&
                        sensor.Name == "GPU Core")
                    {
                        stats.GpuUsage = sensor.Value ?? 0;
                    }

                    if (sensor.SensorType == SensorType.Temperature &&
                        sensor.Name == "GPU Core")
                    {
                        stats.GpuTemp = sensor.Value ?? 0;
                    }

                    if (sensor.SensorType == SensorType.SmallData &&
                        sensor.Name == "GPU Memory Used")
                    {
                        stats.VramUsedGb = (sensor.Value ?? 0) / 1024f;
                    }

                    if (sensor.SensorType == SensorType.SmallData &&
                        sensor.Name == "GPU Memory Total")
                    {
                        stats.VramTotalGb = (sensor.Value ?? 0) / 1024f;
                    }
                }
            }

            // Memory
            if (hardware.HardwareType == HardwareType.Memory)
            {
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Data &&
                        sensor.Name == "Memory Used")
                    {
                        stats.RamUsedGb = sensor.Value ?? 0;
                    }
                }
            }
        }

        stats.RamTotalGb = 32;

        stats.Fps = _rtss.GetFps();

        return stats;
    }
}