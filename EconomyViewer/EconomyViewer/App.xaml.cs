using EconomyViewer.Properties;
using EconomyViewer.Utils;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Threading;

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
                return Settings.Default.DefaultServer != ""
                    ? Settings.Default.DefaultServer
                    : "Classic";
            }
            set
            {
                if (value == null) throw new Exception("Server value = null");
                if (value == Settings.Default.DefaultServer) return;
                Settings.Default.DefaultServer = value;
                Settings.Default.Save();
                ServerChanged(new object(), new EventArgs());
            }
        }
        public App()
        {
#if RESEASE
            if (!IsUserAdministrator())
            {
                MyMessageBox.Show("Run as administrator.", "Нет прав", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
#endif
            if (!File.Exists(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\economy.db"))
                DataBaseWorker.CreateDataBase();
            ServerChanged += App_ServerChanged;
            Server = Settings.Default.DefaultServer;
        }
        private void App_ServerChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("Selected server: " + Server);
        }
        public bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            WindowsIdentity user = null;
            try
            {
                //get the currently logged in user
                user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            finally
            {
                if (user != null)
                    user.Dispose();
            }
            return isAdmin;
        }
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            StackTrace st = new StackTrace(e.Exception, true);
            // Get the top stack frame
            StackFrame frame = st.GetFrame(0);
            // Get the line number from the stack frame
            int line = frame.GetFileLineNumber();
            string file = frame.GetFileName();
            MethodBase method = frame.GetMethod();
            File.AppendAllLines("debug.txt", new List<string>() { "====================", $"{DateTime.Now}", $"Error placement - {file} - {method} - {line} ", $"Error message - {e.Exception.Message}", "====================" });
            MyMessageBox.Show("Fatal exeption accured.\nSend debug.txt file to this Discord: aqua#4101", "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
        }
    }
}
