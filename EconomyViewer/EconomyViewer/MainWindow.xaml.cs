using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        #endregion
    }
}
