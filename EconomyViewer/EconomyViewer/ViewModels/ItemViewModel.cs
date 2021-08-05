using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EconomyViewer.Utils;

namespace EconomyViewer.ViewModels
{
    public class ItemViewModel : ViewModelBase
    {
        private Item selectedItem;
        private List<string> filterMod = new List<string>();
        private ItemList<Item> itemsToSumUp = new ItemList<Item>();
        private readonly Item clearItem = new Item("", 0, 0, "", true);
        private List<Item> items = new List<Item>();
        public uint ToSumUpResult
        {
            get
            {
                uint res = 0;
                foreach (var item in itemsToSumUp)
                {
                    res += item.Price;
                }
                return res;
            }
        }
        public ItemList<Item> ItemsToSumUp
        {
            get => itemsToSumUp;
            set
            {
                itemsToSumUp = value;
                OnPropertyChanged();
                OnPropertyChanged("ToSumUpResult");
            }
        }
        public Item SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged();
            }
        }
        public string SelectedHeader
        {
            get => selectedItem.Header;
            set
            {
                selectedItem = value != null ? ItemList.First(c => c.Header == value) : clearItem;
                selectedItem.PropertyChanged += SelectedItem_PropertyChanged;
                OnPropertyChanged("SelectedItem");
            }
        }
        public List<Item> ItemList
        {
            get
            {
                var data = DataBaseWorker.GetData(App.Server);
                
                if (!Enumerable.SequenceEqual(items, data))
                {
                    items = data;
                    OnPropertyChanged("ItemList");
                    OnPropertyChanged("Headers");
                }
                return items;
            }


        }
        public List<string> Headers
        {
            get
            {
                var headers = filterMod.Count != 0
                    ? ItemList.Where(i => i.Mod.IsOneOf(filterMod.ToArray())).Select(i => i.Header).ToList()
                    : ItemList.Select(i => i.Header).ToList();
                return headers;
            }
        }
        public List<string> FilterMod
        {
            get { return filterMod; }
            set
            {
                filterMod = value;
                OnPropertyChanged("Headers");
            }
        }

        public ItemViewModel(List<string> filter)
        {
            filterMod = filter;
            items = DataBaseWorker.GetData(App.Server).OrderBy(c => c.Header).ToList();
        }
        public ItemViewModel()
        {
            filterMod = new List<string>();
            itemsToSumUp.OnDataChanged += ToSumUpItems_OnDataChanged;
            DataBaseWorker.DataChanged += DataBaseWorker_DataChanged;
        }

        private void DataBaseWorker_DataChanged()
        {
            OnPropertyChanged("Headers");
        }

        private void ToSumUpItems_OnDataChanged()
        {
            OnPropertyChanged("ToSumUpItems");
            OnPropertyChanged("ToSumUpResult");
        }

        private void SelectedItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("SelectedItem");
        }
    }
}
