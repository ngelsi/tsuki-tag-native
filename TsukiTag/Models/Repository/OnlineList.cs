using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Extensions;

namespace TsukiTag.Models.Repository
{
    public class OnlineList : PictureResourceList
    {
        public static Guid DefaultFavoriteList = Guid.Parse("95683ed7-efe0-4820-9c4a-0ef7a37c4aa0");

        public bool IsRemovable
        {
            get
            {
                return Id != DefaultFavoriteList && !IsDefault;
            }
        }
    }
}
