using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;

namespace TsukiTag.ViewModels
{
    public class OnlineProviderViewModel : ViewModelBase
    {
        private readonly IOnlinePictureProvider onlinePictureProvider;

        public OnlineProviderViewModel(
            IOnlinePictureProvider onlinePictureProvider
        )
        {
            this.onlinePictureProvider = onlinePictureProvider;            
        }

        public async void GetImages()
        {
            await Task.Run(async () =>
            {
                await onlinePictureProvider.GetPictures();
            });
        }
    }
}
