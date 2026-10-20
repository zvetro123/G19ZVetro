using G19ZVetro.Models;

namespace G19ZVetro.Services;

public sealed class G19InputService
{
    public event Action<string>? GKeyPressed;

    public void ProcessGKeyPacket(byte value)
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