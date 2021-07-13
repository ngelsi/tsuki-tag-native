using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models.ProviderSpecific
{
    public class SafebooruPicture : Picture
    {

        public SafebooruPicture()
        {
            Provider = TsukiTag.Models.Provider.Safebooru.Name;
        }
    }
}
