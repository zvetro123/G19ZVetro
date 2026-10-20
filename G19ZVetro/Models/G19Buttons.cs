namespace G19ZVetro.Models;

public enum G19ProfileButton : byte
{
    M1 = 0x10,
    M2 = 0x20,
    M3 = 0x40,
    MR = 0x80
}

public enum G19LcdButton : byte
{
    Settings = 0x01,
    Back = 0x02,
    Menu = 0x04,
    Ok = 0x08,
    Right = 0x10,
    Left = 0x20,
    Down = 0x40,
    Up = 0x80
}