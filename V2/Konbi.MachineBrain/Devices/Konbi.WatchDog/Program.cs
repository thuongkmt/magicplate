using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Konbi.WatchDog
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Process thisProc = Process.GetCurrentProcess();
            //Check how many total processes have the same name as the current one
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                // If there is more than one, than it is already running. notify by dialog and closes it after 5 seconds
                Task.Run(async () => {
                    await Task.Delay(5000);
                    Process.GetCurrentProcess().Kill();
                    //Application.Current.Shutdown();
                });
                MessageBox.Show("Application is already running.", "Watchdog does not want to have brothers", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Application.Run(new MainForm());
        }
    }
}
