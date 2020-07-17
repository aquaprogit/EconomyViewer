using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EconomyViewer.Utils;

namespace EconomyViewer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        #region Navigation Bar controls
        private void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            List<ToolTip> toolTips = new List<ToolTip>()
            {
                toolTipAdd,
                toolTipEdit,
                toolTipExchange,
                toolTipHome
            };
            toolTips.ForEach(c => c.Visibility = ToggleNavBar_ToggleButton.IsChecked.Value ? Visibility.Collapsed : Visibility.Visible);
        }
        private void WorkingPanel_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleNavBar_ToggleButton.IsChecked = false;
        }
        #endregion
        #region Window controls
        private void MinimizeWindow_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized; 
        }

        private void CloseWindow_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
