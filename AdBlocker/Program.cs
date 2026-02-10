using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AddBlocker
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += (s, e) => Form1.TryRestoreProxyOnExit();
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Form1.TryRestoreProxyOnExit();
            Application.Run(new Form1());
        }
    }
}
