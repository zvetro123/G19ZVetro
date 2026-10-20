using G19ZVetro.Models;
using HidSharp;
using System.Diagnostics;

namespace G19ZVetro.Services;

public sealed class G19InputService
{
    public event Action<G19ProfileButton>? ProfileButtonPressed;
    public event Action<string>? GKeyPressed;


    private void ProcessProfileButton(byte value)
    {
        if (!Enum.IsDefined(
            typeof(G19ProfileButton),
            (G19ProfileButton)value))
        {
            return;
        }

        ProfileButtonPressed?.Invoke(
            (G19ProfileButton)value);
    }

    private void ProcessExtendedGKey(byte value)
    {
        switch (value)
        {
            case 0x01:
                GKeyPressed?.Invoke("G9");
                break;

            case 0x02:
                GKeyPressed?.Invoke("G10");
                break;

            case 0x04:
                GKeyPressed?.Invoke("G11");
                break;

            case 0x08:
                GKeyPressed?.Invoke("G12");
                break;
        }
    }
    public void Start()
    {
        MonitorG19Devices();
    }

    private void MonitorG19Devices()
    {
        foreach (var device in DeviceList.Local.GetHidDevices())
        {
            if (device.VendorID != 0x046D ||
                device.ProductID != 0xC229 ||
                !device.DevicePath.Contains(
                    "col02",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!device.TryOpen(out var stream))
            {
                Debug.WriteLine(
                    $"FAILED OPEN PID={device.ProductID:X4}");
                continue;
            }

            Debug.WriteLine(
                $"OPENED PID={device.ProductID:X4}");

            Task.Run(() =>
            {
                try
                {
                    byte[] buffer =
                        new byte[
                            device.GetMaxInputReportLength()];

                    while (true)
                    {
                        try
                        {
                            int count = stream.Read(buffer);

                            if (count > 0)
                            {
#if DEBUG
                                Debug.WriteLine(
                                    $"PID={device.ProductID:X4} " +
                                    $"COUNT={count} " +
                                    BitConverter.ToString(buffer));
#endif

                                if (count < 3)
                                {
                                    continue;
                                }

                                byte gKeyValue = buffer[1];

                                if (gKeyValue != 0)
                                {
                                    ProcessGKeyPacket(gKeyValue);
                                }

                                byte value = buffer[2];

                                switch (value)
                                {
                                    case 0x01:
                                    case 0x02:
                                    case 0x04:
                                    case 0x08:
                                        ProcessExtendedGKey(value);
                                        continue;
                                }

                                if (value == 0)
                                {
                                    continue;
                                }

                                ProcessProfileButton(value);
                            }
                        }
                        catch (TimeoutException)
                        {
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"PID={device.ProductID:X4} ERROR {ex}");
                }
            });
        }
    }
    private void ProcessGKeyPacket(byte value)
    {
        switch (value)
        {
            case 0x01:
                GKeyPressed?.Invoke("G1");
                break;

            case 0x02:
                GKeyPressed?.Invoke("G2");
                break;

            case 0x04:
                GKeyPressed?.Invoke("G3");
                break;

            case 0x08:
                GKeyPressed?.Invoke("G4");
                break;

            case 0x10:
                GKeyPressed?.Invoke("G5");
                break;

            case 0x20:
                GKeyPressed?.Invoke("G6");
                break;

            case 0x40:
                GKeyPressed?.Invoke("G7");
                break;

            case 0x80:
                GKeyPressed?.Invoke("G8");
                break;
        }
    }
}