using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EconomyViewer.Utils
{
    public class ItemContainer : INotifyPropertyChanged
    {
        private Item selectedItem;
        private ItemList<Item> toCompareItems;
        private ItemList<Item> itemsToSumUp;
        private ObservableCollection<Item> itemList;
        public ObservableCollection<Item> ItemList
        {
            get => itemList;
            set
            {
                itemList = value;
                OnPropertyChanged();
            }
        }
        public ItemList<Item> ToCompareItems
        {
            get => toCompareItems;
            set
            {
                toCompareItems = value;
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

        public ItemList<Item> ItemsToSumUp
        {
            get
            {
                return itemsToSumUp;
            }
            set
            {
                itemsToSumUp = value;
                OnPropertyChanged();
            }
        }
        public ItemContainer()
        {
            itemsToSumUp = new ItemList<Item>();
            itemsToSumUp.OnDataChanged += ItemsToSumUp_OnDataChanged;
            toCompareItems = new ItemList<Item>();
            toCompareItems.OnDataChanged += ToCompareItems_OnDataChanged;
            itemList = new ObservableCollection<Item>();
            itemList.CollectionChanged += ItemList_CollectionChanged;
        }

        private void ItemList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("ItemList");
        }

        private void ToCompareItems_OnDataChanged()
        {
            if (toCompareItems.Count > 2)
            {
                toCompareItems = (ItemList<Item>)toCompareItems.Reverse().Skip(toCompareItems.Count-2);
            }
            else
            {
                OnPropertyChanged("ToCompareItems");
            }
        }

        private void ItemsToSumUp_OnDataChanged()
        {
            OnPropertyChanged("ItemsToSumUp");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
