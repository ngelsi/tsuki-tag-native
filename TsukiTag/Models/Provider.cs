using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models
{
    public class Provider
    {
        public static Provider Safebooru = new Provider() { Name = "Safebooru" };
        public static Provider Gelbooru = new Provider() { Name = "Gelbooru" };
        public static Provider Konachan = new Provider() { Name = "Konachan" };
        public static Provider Danbooru = new Provider() { Name = "Danbooru" };
        public static Provider Yandere = new Provider() { Name = "Yande.re" };

        public string Name { get; set; }
    }
}
