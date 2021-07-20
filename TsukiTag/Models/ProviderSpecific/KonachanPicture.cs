using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models.ProviderSpecific
{
    public class KonachanPicture : Picture
    {
        public KonachanPicture()
        {
            Provider = Models.Provider.Konachan.Name;
        }
    }
}
