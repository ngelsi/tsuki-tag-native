using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models
{
    public class Rating
    {
        public static Rating Safe = new Rating() { Name = "s", DisplayName = Language.Safe };
        public static Rating Questionable = new Rating() { Name = "q", DisplayName = Language.Questionable };
        public static Rating Explicit = new Rating() { Name = "e", DisplayName = Language.Explicit };
        public static Rating Unknown = new Rating() { Name = "u", DisplayName = Language.Unknown };

        public string Name { get; set; }

        public string DisplayName { get; set; }
    }
}
