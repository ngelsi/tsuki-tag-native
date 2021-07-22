using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models.ProviderSpecific
{
    public class YanderePicture : Picture
    {
        public YanderePicture()
        {
            Provider = Models.Provider.Yandere.Name;
        }
    }
}
