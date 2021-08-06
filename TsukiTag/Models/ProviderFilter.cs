using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models
{
    public class ProviderFilter
    {
        public int Limit { get; set; }

        public int Page { get; set; }

        public List<string> Providers { get; set; }

        public List<string> Ratings { get; set; }

        public List<string> Tags { get; set; }

        public List<string> ExcludedTags { get; set; }        

        public string TagString => string.Join(" ", Tags);

        public ProviderFilterElement FilterElement => new ProviderFilterElement() { Limit = Limit, Page = Page, Tags = new List<string>(Tags), Ratings = new List<string>(Ratings) };

        public ProviderFilter()
        {
            Providers = new List<string>();
            Ratings = new List<string>();
            Tags = new List<string>();
            ExcludedTags = new List<string>();

            Limit = 25;
        }
    }

    public class ProviderFilterElement
    {
        public int Limit { get; set; }

        public int Page { get; set; }

        public List<string> Tags { get; set; }

        public List<string> Ratings { get; set; }

        public string TagString => string.Join(" ", Tags);

        public ProviderFilterElement()
        {
            Ratings = new List<string>();
            Tags = new List<string>();
            Limit = 30;
        }
    }
}
