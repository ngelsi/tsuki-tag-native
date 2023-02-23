using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models
{
    public class Provider
    {
        public static Provider Safebooru = new Provider() { Name = "Safebooru", ImageUrl = "https://safebooru.org/index.php?page=post&s=view&id={0}" };
        public static Provider Gelbooru = new Provider() { Name = "Gelbooru", ImageUrl = "https://gelbooru.com/index.php?page=post&s=view&id={0}" };
        public static Provider R34 = new Provider() { Name = "R34", ImageUrl = "https://rule34.xxx/index.php?page=post&s=view&id={0}" };
        public static Provider Konachan = new Provider() { Name = "Konachan", ImageUrl = "https://konachan.com/post/show/{0}" };
        public static Provider Danbooru = new Provider() { Name = "Danbooru", ImageUrl = "https://danbooru.donmai.us/posts/{0}" };
        public static Provider Yandere = new Provider() { Name = "Yande.re", ImageUrl = "https://yande.re/post/show/{0}" };
        public static Provider Local = new Provider() { Name = "OS" };

        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public static Provider? Get(string name)
        {
            return new Provider[]
            {
                Safebooru,
                Gelbooru,
                Konachan,
                Danbooru,
                Yandere,
                R34
            }.FirstOrDefault(p => p.Name == name);
        }
    }
}
