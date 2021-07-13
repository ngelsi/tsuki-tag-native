using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models
{
    public class ProviderResult
    {
        public List<Picture> Pictures { get; set; }

        public string Provider { get; set; }

        public bool Succeeded { get; set; }

        public bool ProviderEnd { get; set; }

        public string ErrorCode { get; set; }

        public ProviderResult()
        {
            Pictures = new List<Picture>();
        }
    }
}
