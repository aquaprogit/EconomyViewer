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
        private ItemList<Item> toSumUpItems = new ItemList<Item>();
        private readonly Item clearItem = new Item("", 0, 0, "", true);
        private List<Item> items = new List<Item>();
        public int ToSumUpResult
        {
            get
            {
                int res = 0;
                foreach (var item in toSumUpItems)
                {
                    res += item.Price;
                }
                return res;
            }
        }
        public ItemList<Item> ToSumUpItems
        {
            get => toSumUpItems;
            set
            {
                toSumUpItems = value;
                OnPropertyChanged();
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
                if (items != DataBaseWorker.GetData(App.Server).OrderBy(c => c.Header).ToList())
                {
                    items = DataBaseWorker.GetData(App.Server).OrderBy(c => c.Header).ToList();
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
                    ? items.Where(i => i.Mod.IsOneOf(filterMod.ToArray())).Select(i => i.Header).ToList()
                    : items.Select(i => i.Header).ToList();
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
            toSumUpItems.OnDataChanged += ToSumUpItems_OnDataChanged;
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
