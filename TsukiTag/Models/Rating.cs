using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models
{
    public class Rating
    {
        public static Rating Safe = new Rating() { Name = "s" };
        public static Rating Questionable = new Rating() { Name = "q" };
        public static Rating Explicit = new Rating() { Name = "e" };

        public string Name { get; set; }
    }
}
