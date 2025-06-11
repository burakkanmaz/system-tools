using System.Management;
using System.Runtime.InteropServices;
using Serilog;
using Timer = System.Timers.Timer;

namespace SystemTools;

public class BrightnessManager
{
    private Timer _timer;
    private readonly double _latitude;
    private readonly double _longitude;
    private int? _manualOverrideValue;

    public BrightnessManager(double latitude, double longitude)
    {
        _latitude = latitude;
        _longitude = longitude;
    }

    public void StartAutoBrightness()
    {
        _timer = new Timer(60000);
        _timer.Elapsed += (s, e) => AdjustBrightness();
        _timer.Start();
        AdjustBrightness();
    }

    private void AdjustBrightness()
    {
        var now = DateTime.Now;

        if (_manualOverrideValue.HasValue)
        {
            if (now is { Hour: 0, Minute: < 2 })
            {
                Log.Information("Midnight reached, reverting to auto brightness");
                _manualOverrideValue = null;
            }
            else
            {
                SetBrightnessAllDisplays(_manualOverrideValue.Value);
                Log.Information("Manual brightness override active: {Value}%", _manualOverrideValue.Value);
                return;
            }
        }

        var sunrise = SunCalc.GetSunrise(now, _latitude, _longitude);
        var sunset = SunCalc.GetSunset(now, _latitude, _longitude);
        if (Program.Settings != null)
        {
            int value = (now >= sunrise && now <= sunset) ? Program.Settings.DayBrightness : Program.Settings.NightBrightness;
            SetBrightnessAllDisplays(value);
            Log.Information("Auto brightness applied: {Value}%. Time: {Now}", value, now);
        }
    }

    public int? GetInternalBrightness()
    {
        try
        {
            var scope = new ManagementScope(@"\\.\ROOT\WMI");
            scope.Connect();

            var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM WmiMonitorBrightness"));
            foreach (ManagementObject obj in searcher.Get())
            {
                var value = (byte)obj["CurrentBrightness"];
                return value;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to read internal brightness");
        }

        return null;
    }

    public void OverrideBrightnessUntilMidnight(int value)
    {
        _manualOverrideValue = value;
        SetBrightnessAllDisplays(value);
        Log.Information("Manual brightness override set to {Value}% until midnight", value);
    }

    public void ResetAutoBrightness()
    {
        _manualOverrideValue = null;
        AdjustBrightness();
        Log.Information("Manual override reset. Auto brightness resumed.");
    }

    public void SetBrightnessAllDisplays(int value)
    {
        SetBrightnessInternalDisplay(value);
        SetBrightnessExternalDisplays(value);
    }

    public void SetBrightnessInternalDisplay(int value)
    {
        try
        {
            var mclass = new ManagementClass("WmiMonitorBrightnessMethods")
            {
                Scope = new ManagementScope(@"\\.\ROOT\WMI")
            };
            foreach (ManagementObject instance in mclass.GetInstances())
            {
                instance.InvokeMethod("WmiSetBrightness", new object[] { 1, value });
            }
            Log.Information("Set internal brightness to {Value}%", value);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to set internal brightness");
        }
    }

    public void SetBrightnessExternalDisplays(int value)
    {
        try
        {
            IntPtr hMon = MonitorFromPoint(new POINT { x = 0, y = 0 }, 2);
            uint count = 0;
            if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMon, ref count)) return;

            var monitors = new PHYSICAL_MONITOR[count];
            if (!GetPhysicalMonitorsFromHMONITOR(hMon, count, monitors)) return;

            for (int i = 0; i < monitors.Length; i++)
            {
                SetVCPFeature(monitors[i].hPhysicalMonitor, 0x10, (uint)value);
                DestroyPhysicalMonitor(monitors[i].hPhysicalMonitor);
            }
            Log.Information("Set external brightness to {Value}%", value);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to set external brightness");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT { public int x, y; }

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    [DllImport("Dxva2.dll")]
    public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

    [DllImport("Dxva2.dll")]
    public static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

    [DllImport("Dxva2.dll")]
    public static extern bool SetVCPFeature(IntPtr hMonitor, byte bVCPCode, uint dwNewValue);

    [DllImport("Dxva2.dll")]
    public static extern bool DestroyPhysicalMonitor(IntPtr hMonitor);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct PHYSICAL_MONITOR
    {
        public IntPtr hPhysicalMonitor;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szPhysicalMonitorDescription;
    }
}