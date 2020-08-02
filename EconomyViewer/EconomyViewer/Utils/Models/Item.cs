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
        private const uint ZERO = 0;
        private string header;
        private uint count = 1;
        private uint price = 1;
        private string mod = "";
        private bool clearToStringValue = false;
        private readonly uint originalPricePerOne;
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string StringFormat => ToString();
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
        public uint Count
        {
            get => count;

            set
            {
                count = value;
                Price = originalPricePerOne * count;
                OnPropertyChanged();
                OnPropertyChanged("Price");
            }
        }
        /// <summary>
        /// Стоимость предмета в количестве <see cref="Count"/>.
        /// </summary>
        public uint Price
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
        public Item(int id, string header, uint count, uint price, string mod)
        {
            ID = id;
            this.header = header;
            this.count = count;
            this.price = price;
            this.mod = mod;
            if (ZERO.IsOneOf(price, count) == false)
                originalPricePerOne = price / count;
        }
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="Item"/> c задачей открытых параметров.
        /// </summary>
        /// <param name="header">Название предмета.</param>
        /// <param name="count">Количество предмета.</param>
        /// <param name="price">Стоимость предмета за его количество.</param>
        /// <param name="mod">Модификация предмета.</param>
        public Item(string header, uint count, uint price, string mod, bool clearToString = false)
        {
            this.header = header ?? throw new ArgumentNullException("Header value equals null");
            this.count = count;
            this.price = price;
            this.mod = mod ?? throw new ArgumentNullException("Mod value equals null");
            clearToStringValue = clearToString;
            if (ZERO.IsOneOf(price, count) == false)
                originalPricePerOne = price / count;
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
            if (Regex.IsMatch(value, @"(.+)\s([0-9]+) шт. - ([0-9]+)$"))
            {
                var comp = Regex.Match(value, @"(.+)\s([0-9]+) шт. - ([0-9]+)$").Groups;
                string itemName = comp[1].Value;
                uint itemCount = Convert.ToUInt32(comp[2].Value);
                uint itemPrice = Convert.ToUInt32(comp[3].Value);

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
