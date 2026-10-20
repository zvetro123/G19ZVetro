using G19ZVetro.Services;
using HidSharp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

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