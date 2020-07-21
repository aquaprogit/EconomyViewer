using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace EconomyViewer.Utils
{
    /// <summary>
    /// Класс представление для всех предметов экономики SMC.
    /// </summary>
    public class Item : INotifyPropertyChanged
    {
        private string header;
        private int count = 1;
        private int price = 1;
        private string mod;
        private bool clearToStringValue = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string StringFormat =>  ToString();
        /// <summary>
        /// ID предмета в базе данных. Доступ только для чтения
        /// </summary>
        public int ID { get; private set; }
        /// <summary>
        /// Название предмета.
        /// </summary>
        public string Header
        {
            get => header;

            set
            {
                header = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Количество предмета за стоимость <see cref="Price"/>.
        /// </summary>
        public int Count
        {
            get => count;

            set
            {
                int priceForOne = price / count;
                count = value;
                Price = priceForOne * count;
                OnPropertyChanged();
                OnPropertyChanged("Price");
            }
        }
        /// <summary>
        /// Стоимость предмета в количестве <see cref="Count"/>.
        /// </summary>
        public int Price
        {
            get => price;

            set
            {
                price = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Модификация, которая предоставляет данный предмет.
        /// </summary>
        public string Mod
        {
            get => mod;
            set
            {
                mod = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="Item"/> c полной задачей параметров.
        /// </summary>
        /// <param name="id">ID предмета.</param>
        /// <param name="header">Название предмета.</param>
        /// <param name="count">Количество предмета.</param>
        /// <param name="price">Стоимость предмета за его количество.</param>
        /// <param name="mod">Модификация предмета.</param>
        public Item(int id, string header, int count, int price, string mod)
        {
            ID = id;
            this.header = header;
            this.count = count;
            this.price = price;
            this.mod = mod;
        }
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="Item"/> c задачей открытых параметров.
        /// </summary>
        /// <param name="header">Название предмета.</param>
        /// <param name="count">Количество предмета.</param>
        /// <param name="price">Стоимость предмета за его количество.</param>
        /// <param name="mod">Модификация предмета.</param>
        public Item(string header, int count, int price, string mod, bool clearToString = false)
        {
            this.header = header ?? throw new ArgumentNullException("Header value equals null");
            this.count = count;
            this.price = price;
            this.mod = mod ?? throw new ArgumentNullException("Mod value equals null");
            clearToStringValue = clearToString;
        }
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="Item"/> со стандартными параметрами.
        /// </summary>
        public Item() { }
        /// <summary>
        /// Возвращает строку, представляющую данный <see cref="Item"/>
        /// </summary>
        /// <returns>Строку в формате "Название Количество шт. - Цена"</returns>
        public override string ToString()
        {
            return clearToStringValue ? "" : $"{header} {count} шт. - {price}";
        }
        public override bool Equals(object obj)
        {
            Item item = (Item)obj;
            return item.header == header && item.count == count && item.price == price && item.mod == mod;
        }
        public static Item FromString(string value, string mod)
        {
            if (Regex.IsMatch(value, "(.+) ([0-9]+) шт. - ([0-9]+)$"))
            {
                var comp = Regex.Match(value, @"(.+) ([0-9]+) шт. - ([0-9]+)$").Groups;
                string itemName = comp[1].Value;
                int itemCount = Convert.ToInt32(comp[2].Value);
                int itemPrice = Convert.ToInt32(comp[3].Value);

                return new Item(itemName, itemCount, itemPrice, mod);
            }
            return null;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
