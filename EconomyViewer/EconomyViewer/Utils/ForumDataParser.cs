using HtmlAgilityPack;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace EconomyViewer.Utils
{
    internal class ForumDataParser
    {
        public List<Item> GetPostData(string server)
        {
            HtmlWeb hw = new HtmlWeb();
            string[] innerHtmlByLine = new string[1];
            HtmlDocument doc = hw.Load(@"https://f.simpleminecraft.ru/index.php?/forum/49-ekonomika/");
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]").Where(c => c.GetAttributeValue("title", "").StartsWith("Экономика ")))
            {
                if (link.InnerText.Replace("\n", "").Replace("\t", "") == $"Экономика {server}")
                {
                    HtmlDocument currentDocument = hw.Load(link.GetAttributeValue("href", null));
                    innerHtmlByLine = currentDocument.DocumentNode.SelectNodes("//div")
                    .First(c => Regex.IsMatch(c.GetAttributeValue("id", ""), "comment-[0-9]+_wrap")).InnerHtml.Split('\n').ToArray();
                }
            }
            string mod = "";
            List<Item> items = new List<Item>();
            foreach (string line in innerHtmlByLine)
            {
                string thisLine = line;
                if (thisLine.Contains("Список основных изменений"))
                    break;
                if (thisLine.Contains("style=\"font-size:16px;\""))
                {
                    if (Regex.IsMatch(thisLine, @">(.*?)<"))
                        mod = Regex.Matches(thisLine, @">(.*?)<")[1].Groups[1].Value;
                }
                else
                {
                    if (thisLine.EndsWith("<br>"))
                        thisLine = thisLine.Replace("<br>", "");
                    Item item = Item.FromString(thisLine, mod);
                    if (item != null)
                        items.Add(item);
                }
            }
            return items;
        }
    }
}
