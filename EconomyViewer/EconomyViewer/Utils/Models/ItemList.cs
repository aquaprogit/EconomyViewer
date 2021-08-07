using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace EconomyViewer.Utils
{
    public class ItemList<T> : ICollection<Item> where T : Item
    {
        private readonly List<Item> itemList = new List<Item>();
        public int Count { get => itemList.Count; }
        public bool IsReadOnly { get => false; }
        public delegate void DataChangedHandler();
        public event DataChangedHandler OnDataChanged;

        public void RemoveAt(int index)
        {
            if (index < itemList.Count && index >= 0)
            {
                itemList.RemoveAt(index);
            }
            OnDataChanged?.Invoke();
        }
        public void Add(Item item)/* TODO REFACTORING NEEDED */
        {
            Item sameItem = itemList.FirstOrDefault(i => i.Header == item.Header);
            if (sameItem != null)
            {
                if (sameItem.Price / sameItem.Count == item.Price / item.Count)
                {
                    int index = itemList.IndexOf(sameItem);
                    itemList.RemoveAt(index);
                    itemList.Insert(index, new Item()
                    {
                        Header = sameItem.Header,
                        Count = sameItem.Count + item.Count,
                        Price = sameItem.Price + item.Price,
                        Mod = sameItem.Mod
                    });
                }
            }
            else itemList.Add(item);
            OnDataChanged?.Invoke();
        }

        public void Clear()
        {
            itemList.Clear();
            OnDataChanged?.Invoke();
        }

        public bool Contains(Item item)
        {
            return itemList.Contains(item);
        }

        public void CopyTo(Item[] array, int arrayIndex)
        {
            itemList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Item> GetEnumerator()
        {
            return itemList.GetEnumerator();
        }

        public bool Remove(Item item)
        {
            if (itemList.Contains(item))
            {
                itemList.Remove(item);
                OnDataChanged?.Invoke();
                return true;
            }
            else if (itemList.Any(i => i.Header == item.Header))
            {
                Item sameItem = itemList.Find(i => i.Header == item.Header);
                if (sameItem.Price / sameItem.Count == item.Price / item.Count)
                {
                    int index = itemList.IndexOf(sameItem);
                    itemList.RemoveAt(index);
                    itemList.Insert(index, new Item()
                    {
                        Header = sameItem.Header,
                        Count = sameItem.Count - item.Count,
                        Price = sameItem.Price - item.Price,
                        Mod = sameItem.Mod
                    });
                    OnDataChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
