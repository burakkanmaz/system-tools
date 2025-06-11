using Microsoft.Win32;

namespace SystemTools;

public class SystemTrayContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly DisplayScalingManager _scalingManager;
    private readonly BrightnessManager _brightnessManager;
    private readonly MessageWindow _messageWindow;

    public SystemTrayContext()
    {
        _scalingManager = new DisplayScalingManager();
        if (Program.Settings != null)
        {
            _brightnessManager = new BrightnessManager(Program.Settings.Latitude, Program.Settings.Longitude);

            _scalingManager.CheckAndApplyScaling();
            if (Program.Settings.AutoBrightness)
                _brightnessManager.StartAutoBrightness();
        }

        _trayIcon = new NotifyIcon()
        {
            Icon = new Icon("icon.ico"),
            ContextMenuStrip = BuildContextMenu(),
            Visible = true,
            Text = "SystemTools"
        };

        SystemEvents.DisplaySettingsChanged += (s, e) =>
        {
            _scalingManager.CheckAndApplyScaling();
        };

        _messageWindow = new MessageWindow(_scalingManager);
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Adjust Brightness", null, (s, e) => ShowBrightnessForm());
        menu.Items.Add("Reset to Auto Brightness", null, (s, e) => _brightnessManager.ResetAutoBrightness());
        menu.Items.Add("Recheck Displays", null, (s, e) => _scalingManager.CheckAndApplyScaling());
        menu.Items.Add("Exit", null, (s, e) => ExitApplication());
        return menu;
    }

    private void ShowBrightnessForm()
    {
        var form = new BrightnessForm(_brightnessManager);
        form.Show();
    }

    private void ExitApplication()
    {
        _trayIcon.Visible = false;
        _messageWindow?.DestroyHandle();
        Application.Exit();
    }
}