using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;

namespace TsukiTag.ViewModels
{
    public class OnlineNavigationBarViewModel : ViewModelBaseBrowserNavigationHandler
    {
        private bool safebooruEnabled;
        private bool gelbooruEnabled;
        private bool konachanEnabled;
        private bool danbooruEnabled;
        private bool yandereEnabled;

        public bool SafebooruEnabled
        {
            get { return safebooruEnabled; }
            set
            {
                safebooruEnabled = value;
                this.RaisePropertyChanged(nameof(SafebooruEnabled));
            }
        }

        public bool GelbooruEnabled
        {
            get { return gelbooruEnabled; }
            set
            {
                gelbooruEnabled = value;
                this.RaisePropertyChanged(nameof(GelbooruEnabled));
            }
        }

        public bool KonachanEnabled
        {
            get { return konachanEnabled; }
            set
            {
                konachanEnabled = value;
                this.RaisePropertyChanged(nameof(KonachanEnabled));
            }
        }

        public bool DanbooruEnabled
        {
            get { return danbooruEnabled; }
            set
            {
                danbooruEnabled = value;
                this.RaisePropertyChanged(nameof(DanbooruEnabled));
            }
        }

        public bool YandereEnabled
        {
            get { return yandereEnabled; }
            set
            {
                yandereEnabled = value;
                this.RaisePropertyChanged(nameof(YandereEnabled));
            }
        }

        public OnlineNavigationBarViewModel(
            IProviderFilterControl providerFilterControl
        ) : base(providerFilterControl)
        {
        }

        protected override async void ConfigureFilters()
        {
            base.ConfigureFilters();

            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var currentFilter = await this.providerFilterControl.GetCurrentFilter();
                if(currentFilter != null)
                {
                    SafebooruEnabled = currentFilter.Providers.Contains(Provider.Safebooru.Name);
                    GelbooruEnabled = currentFilter.Providers.Contains(Provider.Gelbooru.Name);
                    KonachanEnabled = currentFilter.Providers.Contains(Provider.Konachan.Name);
                    DanbooruEnabled = currentFilter.Providers.Contains(Provider.Danbooru.Name);
                    YandereEnabled = currentFilter.Providers.Contains(Provider.Yandere.Name);
                }
            });
        }

    }
}
