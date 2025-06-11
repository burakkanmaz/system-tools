# SystemTools

A comprehensive Windows system utility collection that runs in the system tray, providing automated brightness control and display scaling management.

## Features

### 🌅 Automatic Brightness Control
- **Solar-based brightness adjustment**: Automatically adjusts screen brightness based on sunrise/sunset times
- **Dual display support**: Controls both internal laptop displays and external monitors
- **Manual override**: Temporarily override automatic settings until midnight
- **Configurable day/night brightness levels**: Set custom brightness percentages for day and night modes
- **Geographic location support**: Uses latitude/longitude coordinates for accurate solar calculations

### 🖥️ Display Scaling Management
- **Automatic DPI scaling**: Detects display configuration changes and applies appropriate scaling
- **Multi-monitor optimization**: Different scaling for internal (150%) and external displays (100%)
- **Hot-plug detection**: Automatically adjusts scaling when monitors are connected/disconnected
- **Manual rechecking**: Force recheck displays through system tray menu

### 🔧 System Integration
- **System tray application**: Runs quietly in the background
- **Windows startup integration**: Automatically starts with Windows
- **Comprehensive logging**: Detailed logs for troubleshooting and monitoring
- **JSON configuration**: Easy-to-edit settings file

## Installation

1. Download the latest release
2. Extract files to your preferred directory
3. Ensure `DPIScaler.exe` is in the same directory (required for display scaling)
4. Run `SystemTools.exe`
5. The application will automatically add itself to Windows startup

## Configuration

Edit the `settings.json` file to customize the application:

```json
{
  "Latitude": 41.0082,
  "Longitude": 28.9784,
  "AutoBrightness": true,
  "DayBrightness": 100,
  "NightBrightness": 30
}
```

### Configuration Options

| Setting | Description | Default |
|---------|-------------|---------|
| `Latitude` | Your geographic latitude for solar calculations | 41.0082 (Istanbul) |
| `Longitude` | Your geographic longitude for solar calculations | 28.9784 (Istanbul) |
| `AutoBrightness` | Enable/disable automatic brightness control | true |
| `DayBrightness` | Brightness percentage during daylight hours (0-100) | 100 |
| `NightBrightness` | Brightness percentage during night hours (0-100) | 30 |

## System Tray Menu

Right-click the system tray icon to access:

- **Adjust Brightness**: Open brightness control window
- **Reset to Auto Brightness**: Clear manual overrides and resume automatic control
- **Recheck Displays**: Force display scaling reconfiguration
- **Exit**: Close the application

## Requirements

- Windows 10/11
- .NET 6.0 or later
- DPIScaler.exe (included for display scaling functionality)
- Administrator privileges may be required for some display operations

## Dependencies

- **DPIScaler**: External utility for display scaling management
  - Repository: https://github.com/burakkanmaz/DPIScaler
- **Serilog**: Logging framework
- **System.Management**: WMI operations for brightness control

## How It Works

### Brightness Control
1. Uses WMI (Windows Management Instrumentation) to control internal display brightness
2. Utilizes DDC/CI protocol for external monitor brightness control
3. Calculates sunrise/sunset times based on geographic coordinates
4. Checks and adjusts brightness every minute
5. Manual overrides persist until midnight, then auto-resume

### Display Scaling
1. Monitors Windows display configuration changes
2. Identifies internal vs external displays
3. Applies optimal DPI scaling (150% internal, 100% external)
4. Uses DPIScaler utility for reliable scaling operations

## Logging

Application logs are stored in the `logs/` directory:
- File: `systemtools.log`
- Contains detailed information about brightness adjustments, display changes, and errors
- Useful for troubleshooting and monitoring application behavior

## Troubleshooting

### Brightness Control Issues
- Ensure your display supports DDC/CI for external monitors
- Check Windows permissions for WMI operations
- Verify monitor cables support DDC/CI communication

### Display Scaling Issues
- Ensure `DPIScaler.exe` is present in the application directory
- Check Windows display settings for conflicts
- Run as administrator if scaling operations fail

### General Issues
- Check `logs/systemtools.log` for detailed error information
- Verify `settings.json` format is valid JSON
- Ensure .NET 6.0 runtime is installed

## License

This project is developed for personal use. Feel free to fork and modify according to your needs.

## Author

Developed by Burak Kanmaz for personal system management needs.
