// Services/G19InputService.cs

using G19ZVetro.Models;

namespace G19ZVetro.Services;

public sealed class G19InputService
{
    public event Action<string>? GKeyPressed;

    public event Action<G19ProfileButton>? ProfileButtonPressed;
}