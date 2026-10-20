using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using G19ZVetro.Services;
using System.Management;
using System.Diagnostics;

namespace G19ZVetro.UI
{
    public partial class App : System.Windows.Application
    {
        private readonly G19Service _service = new();

        public G19Service Service => _service;

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;

                System.Diagnostics.Debug.WriteLine(
                    $"UNHANDLED EXCEPTION: {ex}");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine(
                    $"UNOBSERVED TASK EXCEPTION: {e.Exception}");

                e.SetObserved();
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += (s, ex) =>
            {
                System.Windows.MessageBox.Show(
                    ex.Exception.ToString(),
                    "DISPATCHER EXCEPTION");

                ex.Handled = true;
            };

            try
            {
                base.OnStartup(e);

                _service.Start();

                foreach (ManagementObject obj in
                         new ManagementObjectSearcher(
                             "SELECT * FROM Win32_PnPEntity")
                         .Get())
                {
                    var name = obj["Name"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(name) &&
                        name.Contains("Logitech",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine(
                            $"LOGITECH DEVICE: {name}");
                    }
                }

                foreach (ManagementObject obj in
                         new ManagementObjectSearcher(
                             "SELECT * FROM Win32_Keyboard")
                         .Get())
                {
                    foreach (PropertyData prop in obj.Properties)
                    {
                        Debug.WriteLine(
                            $"KEYBOARD {prop.Name} = {prop.Value}");
                    }

                    Debug.WriteLine(
                        "--------------------------------");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "APP STARTUP ERROR");
            }
        }
    }
}