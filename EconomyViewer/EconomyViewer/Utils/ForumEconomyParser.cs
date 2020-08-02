using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

namespace EconomyViewer.Utils
{
    internal class ForumEconomyParser
    {
        public List<Item> GetPostData(string server)
        {
            HtmlWeb hw = new HtmlWeb();
            string[] innerHtmlByLine = new string[1];
            HtmlDocument doc = hw.Load(@"https://f.simpleminecraft.ru/index.php?/forum/49-ekonomika/");
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]").Where(c => c.GetAttributeValue("title", "").StartsWith("Экономика ")))
            {
                if (link.InnerText.Replace("\n", "") == $"Экономика {server}")
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
                if (thisLine.StartsWith("<strong>") || thisLine.StartsWith("<span"))
                {
                    if (thisLine.Length > 38)
                    {
                        string startWithName = thisLine.Remove(0, 38);
                        if (Regex.IsMatch(startWithName, @"(.*?)<\S*\s*(span|strong)><\S*\s*(span|strong)>"))
                            mod = Regex.Match(startWithName, @"(.*?)<\S*\s*(span|strong)><\S*\s*(span|strong)>").Groups[1].Value.Split('/')[0];
                    }
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

        private string GetHtml(string source)
        {
            using (WebClient client = new WebClient())
            {
                return client.DownloadString(source);
            }
        }
    }
}
