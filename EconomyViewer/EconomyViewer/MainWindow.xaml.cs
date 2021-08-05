using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

using EconomyViewer.Utils;
using EconomyViewer.ViewModels;

namespace EconomyViewer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> FilterMod = new ObservableCollection<string>();
        public ServerViewModel ServerViewModel { get; set; } = new ServerViewModel();
        public ItemViewModel ItemViewModel { get; set; } = new ItemViewModel();
        public bool IsFastAdding { get; set; } = false;
        public bool IsEditMod { get; set; } = false;
        #region Private methods        
        private void CommonAddData()
        {
            if (ToAddItemName_TextBox.Text == "")
            {
                MyMessageBox.Show("Необходимо заполнить все поля!\nПоле \"Название\" было пустое.", "Недостаточно данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                ToAddItemName_TextBox.Focus();
                return;
            }
            if (ToAddItemMod_ComboBox.SelectedIndex == -1 && ToAddItemMod_ComboBox.Text == "")
            {
                MyMessageBox.Show("Необходимо заполнить все поля!\nПоле \"Мод\" было пустое.", "Недостаточно данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                ToAddItemMod_ComboBox.Focus();
                return;
            }
            if (ToAddItemMod_ComboBox.Text != "" && !ServerViewModel.Mods.Contains(ToAddItemMod_ComboBox.Text))
            {
                var result = MyMessageBox.Show("Вы указали в поле \'Мод\' значение, которого ещё нет в таблице.\nХотите добавить новый тип значений для \'Мод\'", "Изменение данных", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result != MessageBoxResult.Yes)
                    return;
            }

            Item newItem = new Item()
            {
                Header = ToAddItemName_TextBox.Text,
                Count = ToAddItemCount_IntUpDown.Value.Value,
                Price = ToAddItemPrice_IntUpDown.Value.Value,
                Mod = ToAddItemMod_ComboBox.Text
            };
            if (ItemViewModel.ItemList.Contains(newItem) == false)
            {
                DataBaseWorker.InsertData(App.Server, newItem);
                MyMessageBox.Show("Добавление новых данных прошло успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                RefillFilterCheckBoxes();
            }
            else
                MyMessageBox.Show("Таблица уже содержит такой объект.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            ToAddItemName_TextBox.Text = ToAddItemMod_ComboBox.Text = "";
            ToAddItemCount_IntUpDown.Value = ToAddItemPrice_IntUpDown.Value = 1;
        }

        private void FastAddData()
        {
            string data = new TextRange(ToAddData_RichTextBox.Document.ContentStart, ToAddData_RichTextBox.Document.ContentEnd).Text;
            string mod = ToFastAddItemMod_ComboBox.Text;
            if (data.Length == 0)
            {
                MyMessageBox.Show("Необходимо заполнить все поля!" +
                    "\nПоле \"Данные\" было пустое.", "Недостаточно данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                ToAddData_RichTextBox.Focus();
                return;
            }
            else if (mod == "")
            {
                MyMessageBox.Show("Необходимо заполнить все поля!" +
                    "\nПоле \"Мод\" было пустое.", "Недостаточно данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                ToFastAddItemMod_ComboBox.Focus();
                return;
            }
            else if (DataBaseWorker.GetOnlyColumnList(App.Server, "i_mod").Contains(mod) == false)
            {
                var result = MyMessageBox.Show("Вы указали в поле \'Мод\' значение, которого ещё нет в таблице." +
                    "\nХотите добавить новый тип значений для \'Мод\'", "Изменение данных", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result != MessageBoxResult.Yes)
                    return;
            }
            int success = 0;
            List<string> errorList = new List<string>();
            foreach (var item in data.Split(new char[] { '\n' }, options: StringSplitOptions.RemoveEmptyEntries).Reverse())
            {
                ToAddData_RichTextBox.ScrollToHome();
                ToAddData_RichTextBox.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;
                string clearItem = item.Remove(item.IndexOf('\r'));
                if (clearItem == "")
                    continue;
                if (Regex.IsMatch(clearItem, @"(.+) ([0-9]+) шт. - ([0-9]+)$"))
                {
                    Item newItem = Item.FromString(clearItem, mod);
                    if (ItemViewModel.ItemList.Contains(newItem) == false)
                    {
                        DataBaseWorker.InsertData(App.Server, Item.FromString(clearItem, mod));
                        success++;
                    }
                    ToAddData_RichTextBox.Document.Blocks.Remove(ToAddData_RichTextBox.Document.Blocks.LastBlock);
                    ToAddData_RichTextBox.ScrollToEnd();
                    UpdateDispatcher();
                }
                else
                {
                    errorList.Add(clearItem);
                    Debug.WriteLine(clearItem);
                }
            }
            ToFastAddItemMod_ComboBox.Text = "";
            ToAddData_RichTextBox.IsEnabled = true;
            Mouse.OverrideCursor = Cursors.Arrow;
            if (errorList.Count == 0)
            {
                if (success > 0)
                    MyMessageBox.Show($"{success} строк были успешно добавлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                {
                    MyMessageBox.Show($"Таблица уже содержит все эти данные", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ToAddData_RichTextBox.Document.Blocks.Clear();
                    ToFastAddItemMod_ComboBox.Text = "";
                }
                RefillFilterCheckBoxes();
            }
            else
                MyMessageBox.Show($"{errorList.Count} строк не были добавлены. \nПроверьте их валидность, или воспользуйтесь обычным режимом добавления.", "Неверный формат", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void UpdateDispatcher()
        {
            DispatcherFrame frame = new DispatcherFrame(true);
            Dispatcher.CurrentDispatcher.BeginInvoke
            (
            DispatcherPriority.Background,
            (SendOrPostCallback)delegate (object arg)
            {
                var f = arg as DispatcherFrame;
                f.Continue = false;
            },
            frame
            );
            Dispatcher.PushFrame(frame);
        }
        private void RefillFilterCheckBoxes()
        {
            MainFilterList_ListView.Items.Clear();
            foreach (var item in DataBaseWorker.GetOnlyColumnList(App.Server, "i_mod"))
                MainFilterList_ListView.Items.Add(new CheckBox() { Content = item });
        }
        private void App_ServerChanged(object sender, EventArgs e)
        {
            if (DataBaseWorker.GetData(App.Server).Count != 0)
            {
                RefillFilterCheckBoxes();
            }
            FilterMod.Clear();
            RefillFilterCheckBoxes();
        }
        private void FilterMod_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ItemViewModel.FilterMod = FilterMod.ToList();
        }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            FilterMod.CollectionChanged += FilterMod_CollectionChanged;
            MainSelectItem_ComboBox.DataContext = ItemViewModel;
            DataContext = this;

            App.ServerChanged += App_ServerChanged;
            App_ServerChanged(null, null);
#if RELEASE
            MenuItem_Click(new ListViewItem() { Tag = "Main" }, null);
#endif
        }
        #region Navigation Bar controls
        private void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            List<ToolTip> toolTips = new List<ToolTip>()
            {
                toolTipAdd,
                toolTipEdit,
                //toolTipExchange,
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
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();

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
            if (ItemViewModel.ItemList.Count == 0 && IsLoaded && Add_Grid.Visibility == Visibility.Hidden)
            {
                MessageBoxResult result = MyMessageBox.Show("Таблица не содержит данных.\nХотите добавить их?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    MenuItem_Click(new ListViewItem() { Tag = "Add" }, null);
                return;
            }
        }
        private void MainSelectItem_ComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(ComboBoxItem))
                return;
            if (MainSelectItem_ComboBox.ItemsSource.Cast<string>().Count() == 0)
            {
                MessageBoxResult result = MyMessageBox.Show("Таблица не содержит данных.\nХотите добавить их?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    MenuItem_Click(new ListViewItem() { Tag = "Add" }, null);
            }
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
                MainCheckAllFilter_CheckBox.IsChecked = false;
            FilterMod.Clear();
            foreach (var item in MainFilterList_ListView.Items.Cast<CheckBox>().ToList())
            {
                if (item.IsChecked == true)
                    FilterMod.Add(item.Content.ToString());
            }
        }

        private void MainCopyValue_Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(MainToCopyValue_TextBox.Text);
        }

        private void RemoveFromSumUp_Button_Click(object sender, RoutedEventArgs e)
        {
            ItemViewModel.ItemsToSumUp.RemoveAt(ItemViewModel.ItemsToSumUp.Count - 1);
            MainToSumUpContainer_RichTextBox.Document.Blocks.Clear();
            foreach (var item in ItemViewModel.ItemsToSumUp)
                MainToSumUpContainer_RichTextBox.Document.Blocks.Add(new Paragraph(new Run(item.ToString())));
            MainToSumUpContainer_RichTextBox.ScrollToEnd();
        }

        private void AddToSumUp_Button_Click(object sender, RoutedEventArgs e)
        {
            Item newItem = ItemViewModel.SelectedItem;
            ItemViewModel.ItemsToSumUp.Add(new Item(newItem.Header, newItem.Count, newItem.Price, newItem.Mod));
            MainToSumUpContainer_RichTextBox.Document.Blocks.Clear();
            foreach (var item in ItemViewModel.ItemsToSumUp)
                MainToSumUpContainer_RichTextBox.Document.Blocks.Add(new Paragraph(new Run(item.ToString())));
            MainToSumUpContainer_RichTextBox.ScrollToEnd();
        }

        private void ClearFromSumUp_Button_Click(object sender, RoutedEventArgs e)
        {
            ItemViewModel.ItemsToSumUp.Clear();
            MainToSumUpContainer_RichTextBox.Document.Blocks.Clear();
        }
        #endregion
        #region Add grid control
        private void ToAdd_Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsFastAdding)
            {
                FastAddData();
            }
            else
            {
                CommonAddData();
            }
        }

        #endregion
        #region Edit grid control
        private void Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ItemToEditName_TextBox.Text == "")
            {
                MyMessageBox.Show("Необходимо заполнить все поля!\nПоле \'Название\' было пустое.", "Недостаточно данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                ItemToEditName_TextBox.Focus();
            }
            else if (ItemToEditMod_ComboBox.Text == "")
            {
                MyMessageBox.Show("Необходимо заполнить все поля!\nПоле \'Мод\' было пустое.", "Недостаточно данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                ItemToEditMod_ComboBox.Focus();
            }
            else
            {
                if (ItemToEditMod_ComboBox.Text.IsOneOf(ServerViewModel.Mods.ToArray()) == false)
                {
                    var result = MyMessageBox.Show("Вы указали в поле \'Мод\' значение, которого ещё нет в таблице.\nХотите добавить новый тип значений для \'Мод\'", "Изменение данных", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result != MessageBoxResult.Yes)
                        return;
                }
                Item editedItem = new Item()
                {
                    Header = ItemToEditName_TextBox.Text,
                    Count = ItemToEditCount_IntUpDown.Value.Value,
                    Price = ItemToEditPrice_IntUpDown.Value.Value,
                    Mod = ItemToEditMod_ComboBox.Text
                };
                DataBaseWorker.UpdateData(App.Server, editedItem, ItemViewModel.SelectedItem.ID);
                MyMessageBox.Show("Редактирование данных прошло успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                RefillFilterCheckBoxes();
                EditSelectItem_ComboBox.SelectedIndex = -1;
            }

        }
        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MyMessageBox.Show("Вы пытаетесь удалить объект из таблицы.\nЭто действие нельзя отменить.\nПродолжить?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                DataBaseWorker.DeleteData(App.Server, ItemViewModel.SelectedItem);
                MyMessageBox.Show("Удаление данных прошло успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion
    }
}
