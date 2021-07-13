using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<ProviderTabModel> tabs;

        public ObservableCollection<ProviderTabModel> Tabs
        {
            get { return tabs; }
            set { tabs = value; this.RaisePropertyChanged(nameof(Tabs)); }
        }

        public OnlineProviderViewModel(
            IOnlinePictureProvider onlinePictureProvider
        )
        {
            this.onlinePictureProvider = onlinePictureProvider;            
        }

        public async void GetImages()
        {
            if(this.tabs == null || this.tabs.Count == 0)
            {
                this.tabs = new ObservableCollection<ProviderTabModel>();
                this.tabs.Add(ProviderTabModel.OnlineBrowserTab);

                this.RaisePropertyChanged(nameof(Tabs));
            }

            await Task.Run(async () =>
            {                
                await onlinePictureProvider.GetPictures();
            });
        }
    }
}
