using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models.ProviderSpecific
{
    public class GelbooruPicture : Picture
    {
        public GelbooruPicture()
        {
            Provider = Models.Provider.Gelbooru.Name;
        }
    }
}
