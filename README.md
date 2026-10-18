# G19ZVetro

A modern telemetry dashboard for the Logitech G19 LCD.

G19ZVetro displays real-time system statistics directly on the integrated LCD screen of a Logitech G19 keyboard, including FPS, CPU utilization, GPU utilization, temperatures, RAM usage, and VRAM usage.

The project includes custom USB communication, menu navigation, configurable settings, and persistent user preferences.

---

## Features

### Dashboard
- Real-time FPS display
- CPU utilization and temperature
- GPU utilization and temperature
- RAM usage
- VRAM usage
- Color-coded warnings for temperatures and memory usage

### Pages
- Dashboard
- Hardware
- Memory
- Performance
- Settings

### Navigation
- G19 softkey support
- Left / Right page navigation
- Main menu system
- Settings menu
- About screen

### Settings
- Adjustable refresh rate
  - 250 ms
  - 500 ms
  - 1000 ms
- Persistent settings storage using JSON
- Settings survive application restarts

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
- LibreHardwareMonitor
- LibUsbDotNet
- System.Drawing

---

## Project Structure

```text
G19ZVetro/
├── Models/
│   ├── AppSettings.cs
│   └── SystemStats.cs
│
├── Rendering/
│   └── DashboardRenderer.cs
│
├── Telemetry/
│   ├── HardwareMonitor.cs
│   └── RtssMonitor.cs
│
├── Program.cs
├── SettingsManager.cs
│
└── settings.json
```

---

## Requirements

### Hardware

- Logitech G19 Keyboard
- Logitech G19 LCD display

### Software

- Windows 10 / Windows 11
- .NET 10 SDK

---

## Building

Clone the repository:

```bash
git clone https://github.com/zvetro123/G19ZVetro.git
```

Open the solution in Visual Studio and build:

```bash
Build → Rebuild Solution
```

Run the application:

```bash
F5
```

---

## Settings Persistence

Application settings are stored in:

```text
settings.json
```

Example:

```json
{
  "RefreshRate": 500
}
```

---

## Roadmap

### Planned

- FPS history graph
- Temperature warning thresholds
- Additional performance metrics
- Custom dashboard layouts
- Multiple monitoring profiles

---

## Acknowledgements

- Logitech G19 community
- LibreHardwareMonitor
- LibUsbDotNet

---

## License

MIT License
