using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using G19ZVetro.Services;
using System.Management;
using System.Diagnostics;
using HidSharp;
using System.Linq;

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

                Debug.WriteLine("========== HID DEVICES ==========");

                foreach (var device in DeviceList.Local.GetHidDevices())
                {
                    Debug.WriteLine(
                        $"HID: VID={device.VendorID:X4} " +
                        $"PID={device.ProductID:X4} " +
                        $"Product={device.ProductName}");
                }

                Debug.WriteLine("===============================");

                foreach (var device in DeviceList.Local.GetHidDevices())
                {
                    if (device.VendorID == 0x046D)
                    {
                        Debug.WriteLine(
                            $"PATH: {device.DevicePath}");

                        Debug.WriteLine(
                            $"VID={device.VendorID:X4} " +
                            $"PID={device.ProductID:X4}");

                        Debug.WriteLine(
                            $"PRODUCT={device.ProductName}");

                        Debug.WriteLine(
                            $"MAX INPUT={device.GetMaxInputReportLength()}");

                        Debug.WriteLine(
                            "--------------------------------");
                    }
                }

                var macroDevice = DeviceList.Local
                    .GetHidDevices(0x046D, 0xC229)
                    .FirstOrDefault(d =>
                        d.DevicePath.Contains("col02"));

                if (macroDevice != null)
                {
                    Debug.WriteLine(
                        $"PATH={macroDevice.DevicePath}");

                    Debug.WriteLine(
                        $"INPUT={macroDevice.GetMaxInputReportLength()}");

                    Debug.WriteLine(
                        $"OUTPUT={macroDevice.GetMaxOutputReportLength()}");

                    Debug.WriteLine(
                        $"FEATURE={macroDevice.GetMaxFeatureReportLength()}");
                }
                else
                {
                    Debug.WriteLine(
                        "C229 col02 not found");
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