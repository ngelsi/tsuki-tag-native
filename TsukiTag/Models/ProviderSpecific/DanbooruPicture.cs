using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models.ProviderSpecific
{
    public class DanbooruPicture : Picture
    {
        public DanbooruPicture()
        {
            Provider = Models.Provider.Danbooru.Name;
        }
    }
}
