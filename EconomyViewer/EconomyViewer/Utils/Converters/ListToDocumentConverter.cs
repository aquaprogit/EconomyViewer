using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;

namespace EconomyViewer.Utils
{
    class ListToDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != null)
            {
                List<Item> items = value as List<Item>;
                FlowDocument document = new FlowDocument();
                foreach (var item in items)
                {
                    document.Blocks.Add(new Paragraph(new Run(item.ToString())));
                }
                return document;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
