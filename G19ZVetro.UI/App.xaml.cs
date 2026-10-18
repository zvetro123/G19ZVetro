using System;
using System.Windows;
using System.Windows.Threading;
using G19ZVetro.Services;

namespace G19ZVetro.UI
{
    public partial class App : System.Windows.Application
    {
        private readonly G19Service _service = new();

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                System.Windows.MessageBox.Show(
                    e.ExceptionObject?.ToString() ?? "Unknown exception",
                    "UNHANDLED EXCEPTION");
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