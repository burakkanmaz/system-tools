using System.Diagnostics;
using Serilog;

namespace SystemTools;

// DPIScaler can be found at https://github.com/burakkanmaz/DPIScaler
public class DisplayScalingManager
{
    public void CheckAndApplyScaling()
    {
        Log.Information("Display scaling check invoked.");

        try
        {
            var (internalId, externalId) = GetDisplayIds();
            if (internalId == -1)
            {
                Log.Warning("Internal display ID not detected. Scaling skipped.");
                return;
            }

            bool hasExternal = externalId != -1;

            ApplyScalingWithDPIScaler(internalId, externalId, hasExternal);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Display scaling check failed.");
        }
    }

    private void ApplyScalingWithDPIScaler(int internalId, int externalId, bool hasExternal)
    {
        if (hasExternal)
        {
            Process.Start("DPIScaler.exe", $"-SetDPIValue -DisplayAdapter={internalId} -DPIValue=150");
            Thread.Sleep(1000);
            Process.Start("DPIScaler.exe", $"-SetDPIValue -DisplayAdapter={externalId} -DPIValue=100");
            Log.Information("DPIScaler set: Internal=150%, External=100%");
        }
        else
        {
            Process.Start("DPIScaler.exe", $"-SetDPIValue -DisplayAdapter={internalId} -DPIValue=100");
            Log.Information("DPIScaler set: Internal=100%");
        }
    }

    private (int internalId, int externalId) GetDisplayIds()
    {
        int internalId = -1;
        int externalId = -1;

        var psi = new ProcessStartInfo
        {
            FileName = "DPIScaler.exe",
            Arguments = "-GetAdapterID",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
            return (-1, -1);

        using var reader = process.StandardOutput;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.Contains("internal display", StringComparison.OrdinalIgnoreCase))
            {
                var id = line.Split('.')[0];
                if (int.TryParse(id.Trim(), out var parsed)) internalId = parsed;
            }
            else if (line.Contains("DELL", StringComparison.OrdinalIgnoreCase) ||
                     line.Contains("display", StringComparison.OrdinalIgnoreCase))
            {
                var id = line.Split('.')[0];
                if (int.TryParse(id.Trim(), out var parsed)) externalId = parsed;
            }
        }

        return (internalId, externalId);
    }
}