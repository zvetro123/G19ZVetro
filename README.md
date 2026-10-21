# G19ZVetro

A modern telemetry dashboard and customization suite for the Logitech G19 keyboard.

G19ZVetro brings the Logitech G19 into the modern era with real-time hardware monitoring, programmable G-keys, macro recording, profile management, RGB backlight control, LCD menu navigation, and persistent user settings.

The application communicates directly with the G19 LCD and keyboard hardware over USB without requiring Logitech Gaming Software.

---

## Features

### LCD Dashboard

Display real-time system information directly on the G19 LCD:

- FPS monitoring (RTSS)
- CPU utilization
- CPU temperature
- GPU utilization
- GPU temperature
- RAM usage
- VRAM usage
- Color-coded warning indicators
- Configurable refresh rate

### LCD Pages

- Dashboard
- Hardware
- Memory
- Performance
- Settings
- About

### LCD Navigation

Supports all built-in G19 LCD buttons:

- Menu
- Settings
- Back
- OK
- Up
- Down
- Left
- Right

Features include:

- Page navigation
- Settings menu
- Profile selection menu
- About screen

---

## Profile Management

Three hardware profiles are supported:

- M1
- M2
- M3

Each profile stores:

- Profile name
- RGB backlight color
- G-key assignments
- Recorded macros

Profiles can be switched using:

- Physical M1/M2/M3 buttons
- LCD profile menu
- Desktop configuration application

---

## RGB Backlight Control

Direct USB control of the G19 keyboard backlight.

Features:

- Per-profile RGB colors
- Instant profile switching
- Automatic color restoration after reconnect
- Persistent RGB configuration

Example:

| Profile | Color |
|----------|----------|
| M1 | Red |
| M2 | Green |
| M3 | Blue |

---

## G-Key Programming

Supports all programmable G-keys:

- G1-G12

Each key can be assigned to:

- Launch applications
- Open websites
- Open folders
- Play recorded macros

---

## Macro Recording

Built-in macro recording system:

1. Press MR
2. Press the target G-key
3. Record a sequence of keys
4. Press MR again to save

Features:

- Profile-specific macros
- Persistent storage
- Playback through assigned G-keys

---

## Hardware Monitoring

Information displayed on the LCD includes:

- FPS
- CPU usage
- CPU temperature
- GPU usage
- GPU temperature
- RAM usage
- VRAM usage

Powered by:

- LibreHardwareMonitor
- RTSS integration

---

## Device Recovery

Robust USB handling includes:

- Automatic keyboard detection
- Disconnect detection
- Automatic reconnect handling
- LCD recovery
- RGB restoration after reconnect

---

## Desktop Configuration Utility

WPF-based configuration application with:

- Profile management
- RGB color editing
- G-key configuration
- Macro assignment
- Refresh-rate settings
- Startup settings

---

## System Tray Integration

The application can run quietly in the background.

Features:

- Minimize to tray
- Restore on double-click
- Context menu support
- Quick application exit

---

## Startup Integration

Optional Windows startup support configurable within the application.

---

## Screens

### Dashboard

Displays an overview of all major telemetry information.

### Hardware

Displays CPU and GPU utilization and temperatures.

### Memory

Displays RAM and VRAM usage.

### Performance

Displays FPS and performance statistics.

### Settings

Configure dashboard behavior and application settings.

### About

Displays application information and version details.

---

## Technologies

- .NET 10
- C#
- WPF
- LibreHardwareMonitor
- RTSS
- LibUsbDotNet
- HidSharp
- System.Drawing

---

## Project Structure

```text
G19ZVetro/
├── Models/
│   ├── AppSettings.cs
│   ├── Profile.cs
│   ├── GKeyAction.cs
│   └── SystemStats.cs
│
├── Services/
│   ├── G19Service.cs
│   ├── G19InputService.cs
│   ├── MacroService.cs
│   ├── ProfileManager.cs
│   ├── StartupManager.cs
│   └── SettingsManager.cs
│
├── Rendering/
│   └── DashboardRenderer.cs
│
├── Telemetry/
│   ├── HardwareMonitor.cs
│   └── RtssMonitor.cs
│
├── UI/
│   └── MainWindow.xaml
│
├── G19LCD/
│
└── settings.json
```

---

## Requirements

### Hardware

- Logitech G19 Keyboard

### Software

- Windows 10 or Windows 11
- .NET 10 Runtime

### Driver

LCD functionality requires a libusb-compatible driver configured for the G19 LCD interface.

---

## Building

Clone the repository:

```bash
git clone https://github.com/zvetro123/G19ZVetro.git
```

Open the solution in Visual Studio and build:

```text
Build → Rebuild Solution
```

Run:

```text
F5
```

---

## Configuration

Settings are stored in:

```text
settings.json
```

Configuration includes:

- Refresh rate
- Active profile
- RGB colors
- G-key assignments
- Macros
- Startup options

---

## Roadmap

### Planned

- Custom LCD layouts
- Additional telemetry pages
- FPS history graphs
- User-selectable themes
- Import/export profiles
- Sensor customization

---

## Acknowledgements

G19ZVetro was originally built upon the excellent G19LCD project by Pontus Holmberg (endlessmind).

Original project:

https://github.com/endlessmind/G19LCD

The original G19LCD library provided low-level USB communication with the Logitech G19 LCD display.

G19ZVetro extends that foundation with:

- .NET 10 support
- Hardware telemetry
- RTSS FPS monitoring
- LCD menu navigation
- Profile management
- G-key programming
- Macro recording
- RGB backlight control
- Device recovery handling
- WPF configuration utility
- System tray integration
- Windows startup integration

Additional thanks to the G19 reverse-engineering community and to MagicMau for the G19USB project and RGB backlight protocol research.

---

## License

MIT License

---

**Current Release:** v1.0.0  
**Status:** Stable Release 🚀
