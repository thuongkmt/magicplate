using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Konbi.Common.Interfaces;

namespace BarcodeReaderListener
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        protected override void OnStartup(StartupEventArgs e)
        {
            Process thisProc = Process.GetCurrentProcess();
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                Application.Current.Shutdown();
                return;
            }
            RegisterGlobalExceptionHandling();

            base.OnStartup(e);

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Icon = BarcodeReaderListener.Properties.Resources.barcode;
            _notifyIcon.Visible = true;

            CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip =
              new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => ExitApplication();
        }

        private void ExitApplication()
        {
            _notifyIcon.Dispose();
            _notifyIcon = null;
            App.Current.Shutdown();
        }

        private void RegisterGlobalExceptionHandling()
        {
            System.AppDomain.CurrentDomain.UnhandledException +=
                (sender, args) => CurrentDomainOnUnhandledException(args);

            this.Dispatcher.UnhandledException +=
                (sender, args) => DispatcherOnUnhandledException(args);

            Application.Current.DispatcherUnhandledException +=
                (sender, args) => CurrentOnDispatcherUnhandledException(args);

            TaskScheduler.UnobservedTaskException +=
                (sender, args) => TaskSchedulerOnUnobservedTaskException(args);
        }

        private static void TaskSchedulerOnUnobservedTaskException(UnobservedTaskExceptionEventArgs args)
        {

            LogService.LogException(args.Exception);
            args.SetObserved();
        }

        private static IKonbiBrainLogService LogService
        {
            get
            {
                var log = IoC.Get<IKonbiBrainLogService>();
                return log;
            }
        }

        private static void CurrentOnDispatcherUnhandledException(DispatcherUnhandledExceptionEventArgs args)
        {
            LogService.LogException(args.Exception);
        }

        private static void DispatcherOnUnhandledException(DispatcherUnhandledExceptionEventArgs args)
        {
            LogService.LogException(args.Exception);
        }

        private static void CurrentDomainOnUnhandledException(UnhandledExceptionEventArgs args)
        {
            var exception = args.ExceptionObject as Exception;
        
            LogService.LogException(exception);

        }
    }
}
