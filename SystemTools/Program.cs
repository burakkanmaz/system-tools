using System.Text.Json;
using Microsoft.Win32;
using Serilog;

namespace SystemTools
{
    public static class Program
    {
        public static AppSettings? Settings;

        [STAThread]
        static void Main()
        {
            try
            {
                LoadSettings();
                ConfigureLogging();
                ConfigureExceptionHandling();
                EnsureStartupEntry();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new SystemTrayContext());
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unhandled exception occurred in Main");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureExceptionHandling()
        {
            Application.ThreadException += (sender, args) =>
            {
                Log.Error(args.Exception, "Unhandled UI thread exception");
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                Log.Error(ex, "Unhandled non-UI thread exception");
            };
        }

        private static void LoadSettings()
        {
            try
            {
                var json = File.ReadAllText("settings.json");
                Settings = JsonSerializer.Deserialize<AppSettings>(json);
            }
            catch
            {
                Settings = new AppSettings();
            }
        }

        private static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/systemtools.log", rollingInterval: RollingInterval.Infinite)
                .CreateLogger();
        }

        private static void EnsureStartupEntry()
        {
            string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            using var key = Registry.CurrentUser.OpenSubKey(runKey, true);
            var exePath = Application.ExecutablePath;

            if (key != null && key.GetValue("SystemTools") == null)
            {
                key.SetValue("SystemTools", $"\"{exePath}\"");
                Log.Information("SystemTools added to Windows startup.");
            }
        }
    }
}
