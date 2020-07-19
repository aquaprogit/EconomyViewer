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
        private List<string> Headers = new List<string>();
        private List<string> FilterMod = new List<string>();
        public ItemContainer ItemContainer { get; set; } = new ItemContainer();
        public string SelectedServer
        {
            get => App.Server;
            set => App.Server = value;
        }
        #region Private methods
        private void RefillFilterCheckBoxes()
        {
            MainFilterList_ListView.Items.Clear();
            foreach (var item in DataBaseWorker.GetOnlyColumnList(App.Server, "i_mod"))
                MainFilterList_ListView.Items.Add(new CheckBox() { Content = item });
        }
        private void App_ServerChanged(object sender, EventArgs e)
        {
            MainFilterList_ListView.Items.Clear();
            ItemContainer.ItemList.Clear();
            Headers.Clear();
            if (DataBaseWorker.GetData(App.Server).Count != 0)
            {
                RefillFilterCheckBoxes();

                foreach (Item item in DataBaseWorker.GetData(App.Server))
                {
                    ItemContainer.ItemList.Add(item);
                    Headers.Add(item.Header);
                }
            }
            MainCheckAllFilter_CheckBox.IsEnabled = Headers.Count != 0;
            MainSelectItem_ComboBox.ItemsSource = Headers;
            MainSelectItem_ComboBox.MaxDropDownHeight = Headers.Count != 0 ? 360 : 0;
            RefillFilterCheckBoxes();
        }
        private void SearchToAutoComplete(ComboBox comboBox)
        {
            if (comboBox.Text == "")
            {
                comboBox.ItemsSource = Headers;
                comboBox.Text = "";
                comboBox.IsDropDownOpen = false;
                ItemContainer.SelectedItem = null;
                return;
            }
            var list = Search(comboBox.Text, Headers);
            comboBox.ItemsSource = list;
            comboBox.IsDropDownOpen = list.Count != 0;
            comboBox.SelectedIndex = -1;
        }

        private List<string> Search(string target, IEnumerable<string> coll)
        {
            List<string> result = new List<string>();
            if (coll.Where(c => c.ToUpper().Contains(target.ToUpper())).Count() != 0)
            {
                foreach (var item in coll)
                {
                    if (item.ToUpper().Contains(target.ToUpper()))
                        result.Add(item);
                }
                return result;
            }
            return new List<string>();
        }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            App.ServerChanged += App_ServerChanged;
            MainSelectServer_ComboBox.ItemsSource = DataBaseWorker.GetAllTables();
            App_ServerChanged(null, null);
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
        private void MenuItem_Click(object sender, MouseButtonEventArgs e)
        {
            string selectedWindowTag = (sender as ListViewItem).Tag.ToString();
            Dictionary<Grid, string> ActionGridWithHeader = new Dictionary<Grid, string>()
            {
                {Main_Grid, "Главная" },
                {Exchange_Grid, "Обмен" },
                {Edit_Grid, "Редактировать" },
                {Add_Grid, "Добавить" }
            };
            void SelectGrid(KeyValuePair<Grid, string> gridWithHeader)
            {
                Grid_Header.Text = gridWithHeader.Value;
                ActionGridWithHeader.Keys.ToList().ForEach(g => g.Visibility = g == gridWithHeader.Key ? Visibility.Visible : Visibility.Hidden);
            }
            SelectGrid(ActionGridWithHeader.First(p => p.Key.Name.Replace("_Grid", "") == selectedWindowTag));
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
        #endregion
        #region Main grid control
        private void MainSelectServer_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainSelectServer_ComboBox.SelectedItem != null)
            {
                if (ItemContainer.ItemList.Count == 0 && IsLoaded && Main_Grid.Visibility == Visibility.Visible)
                {
                    MessageBoxResult result = MyMessageBox.Show("Таблица не содержит данных.\nХотите добавить их?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                        MenuItem_Click(new ListViewItem() { Tag = "Add" }, null);
                    return;
                }

            }
        }

        private void MainSelectItem_ComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(ComboBoxItem))
                return;
            if (FilterMod.Count != 0)
                MainSelectItem_ComboBox.ItemsSource = Headers.Where(c => ItemContainer.ItemList.First(b => b.Header == c).Mod.IsOneOf(FilterMod.ToArray()));
            else
                MainSelectItem_ComboBox.ItemsSource = Headers;
            MainSelectItem_ComboBox.SelectedIndex = -1;
            if (MainSelectItem_ComboBox.ItemsSource.Cast<string>().Count() == 0)
            {
                MainSelectItem_ComboBox.MaxDropDownHeight = 0;

                MessageBoxResult result = MyMessageBox.Show("Таблица не содержит данных.\nХотите добавить их?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    MenuItem_Click(new ListViewItem() { Tag = "Add" }, null);
            }
            else
            {
                MainSelectItem_ComboBox.MaxDropDownHeight = 360;
            }
        }
        private void MainSelectItem_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainSelectItem_ComboBox.SelectedItem == null)
            {
                //ToViewSelectedItemPrice_TextBlock.Text = "1";
                //ToViewSelectedItemCount_IntUpDown.Value = 1;
                //ToViewSelectedItemName_TextBlock.Text = "";
                MainToCopyValue_TextBox.Text = "";
                MainSelectItem_ComboBox.SelectedIndex = -1;
                return;
            }
            ItemContainer.SelectedItem = ItemContainer.ItemList.ToList().Find(i => i.Header == MainSelectItem_ComboBox.SelectedItem.ToString());
            Debug.WriteLine(ItemContainer.SelectedItem.ToString());
            if (MainSelectItem_ComboBox.Template.FindName("PART_EditableTextBox", MainSelectItem_ComboBox) is TextBox editablePart)
            {
                editablePart.SelectionLength = 0;
            }
            MainSelectItem_ComboBox.MaxDropDownHeight = 360;
            e.Handled = true;
        }
        private void MainSelectItem_ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchToAutoComplete(MainSelectItem_ComboBox);
        }

        private void MainSelectItem_ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }
        private void MainSelectItem_ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (MainSelectItem_ComboBox.ItemsSource.Cast<string>().Count() == 0)
                MainSelectItem_ComboBox.IsDropDownOpen = false;
            else
            {
                MainSelectItem_ComboBox.MaxDropDownHeight = 360;
            }
            MainSelectItem_ComboBox.ClearSelection();
        }

        private void MainCheckAllFilter_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            List<CheckBox> filterCheckBoxes = MainFilterList_ListView.Items.Cast<CheckBox>().ToList();
            filterCheckBoxes.ForEach(c => c.IsChecked = MainCheckAllFilter_CheckBox.IsChecked);
            FilterMod.Clear();
        }
        private void FilterCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (MainFilterList_ListView.Items.Cast<CheckBox>().All(c => c.IsChecked == true) ||
                MainFilterList_ListView.Items.Cast<CheckBox>().All(c => c.IsChecked == false))
            {
                MainCheckAllFilter_CheckBox.IsChecked = MainFilterList_ListView.Items.Cast<CheckBox>().First().IsChecked;
            }
            else
            {
                MainCheckAllFilter_CheckBox.IsChecked = false;
            }
            FilterMod.Clear();
            foreach (var item in MainFilterList_ListView.Items.Cast<CheckBox>().ToList())
            {
                if (item.IsChecked == true)
                    FilterMod.Add(item.Content.ToString());
            }
        }

        private void MainCopyValue_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SelectedItemCount_IntUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void RemoveFromSumUp_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddToSumUp_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearFromSumUp_Button_Click(object sender, RoutedEventArgs e)
        {

        }


        #endregion

    }
}
