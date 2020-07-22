using EconomyViewer.Utils;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;

namespace EconomyViewer
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static event EventHandler ServerChanged;
        public static string Server
        {
            get
            {
                return EconomyViewer.Properties.Settings.Default.DefaultServer != "" ? EconomyViewer.Properties.Settings.Default.DefaultServer : "Classic";
            }
            set
            {
                if (value == null) throw new Exception("Server value = null");
                if (value == EconomyViewer.Properties.Settings.Default.DefaultServer) return;
                EconomyViewer.Properties.Settings.Default.DefaultServer = value;
                EconomyViewer.Properties.Settings.Default.Save();
                ServerChanged(new object(), new EventArgs());
            }
        }
        public App()
        {
            if (!File.Exists(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\economy.db"))
            {
                DataBaseWorker.CreateDataBase();
            }
            ServerChanged += App_ServerChanged;
            Server = EconomyViewer.Properties.Settings.Default.DefaultServer;
        }
        private void App_ServerChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("Selected server - " + Server);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var st = new StackTrace(e.Exception, true);
            // Get the top stack frame
            var frame = st.GetFrame(0);
            // Get the line number from the stack frame
            var line = frame.GetFileLineNumber();
            var file = frame.GetFileName();
            var method = frame.GetMethod();
            File.AppendAllLines("debug.txt", new List<string>() { "====================", $"{DateTime.Now}", $"Error placement - {file} - {method} - {line} ", $"Error message - {e.Exception.Message}", "====================" });
            MyMessageBox.Show("Fatal exeption accured.\nSend debug.txt file to this Discord: aqua#4101", "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SplashScreen sp = new SplashScreen("Utils/splashscreen.png");
            sp.Show(false);

            sp.Close(new TimeSpan(0, 0, 0, 1));
            Thread.Sleep(1000);
            MainWindow window = new MainWindow();
            window.Show();
        }
    }
}
