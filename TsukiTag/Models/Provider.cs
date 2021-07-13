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

        public string Name { get; set; }
    }
}
