using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies.ProviderSpecific;
using TsukiTag.Extensions;
using TsukiTag.Models;

namespace TsukiTag.Dependencies
{
    public interface IOnlinePictureProvider : IPictureProvider
    {
    }

    public class OnlinePictureProvider : IOnlinePictureProvider
    {
        private readonly ISafebooruPictureProvider safebooruPictureProvider;
        private readonly IGelbooruPictureProvider gelbooruPictureProvider;
        private readonly IKonachanPictureProvider konachanPictureProvider;
        private readonly IDanbooruPictureProvider danbooruPictureProvider;
        private readonly IYanderePictureProvider yanderePictureProvider;

        private readonly IPictureControl pictureControl;
        private readonly IProviderFilterControl providerFilterControl;
        private readonly INotificationControl notificationControl;
        private readonly ILocalizer localizer;
        private readonly IDbRepository dbRepository;

        private List<string> finishedProviders;

        public OnlinePictureProvider(
            ISafebooruPictureProvider safebooruPictureProvider,
            IGelbooruPictureProvider gelbooruPictureProvider,
            IKonachanPictureProvider konachanPictureProvider,
            IDanbooruPictureProvider danbooruPictureProvider,
            IYanderePictureProvider yanderePictureProvider,

            IDbRepository dbRepository,
            IProviderFilterControl providerFilterControl,
            IPictureControl pictureControl,
            INotificationControl notificationControl,
            ILocalizer localizer
        )
        {
            this.safebooruPictureProvider = safebooruPictureProvider;
            this.gelbooruPictureProvider = gelbooruPictureProvider;
            this.konachanPictureProvider = konachanPictureProvider;
            this.danbooruPictureProvider = danbooruPictureProvider;
            this.yanderePictureProvider = yanderePictureProvider;

            this.providerFilterControl = providerFilterControl;
            this.notificationControl = notificationControl;
            this.pictureControl = pictureControl;
            this.localizer = localizer;
            this.dbRepository = dbRepository;

            this.finishedProviders = new List<string>();
        }

        ~OnlinePictureProvider()
        {
            this.providerFilterControl.PageChanged -= OnPageChanged;
            this.providerFilterControl.FilterChanged -= OnFilterChanged;
        }

        private List<IPictureProviderElement> allProviders => new List<IPictureProviderElement>()
            { safebooruPictureProvider, gelbooruPictureProvider, konachanPictureProvider, danbooruPictureProvider, yanderePictureProvider }
            .Where(p => !finishedProviders.Contains(p.Provider))
            .ToList();

        private async void OnFilterChanged(object? sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                this.finishedProviders = new List<string>();
                this.pictureControl.ResetPictures();

                await this.GetPictures();
            });
        }

        private async void OnPageChanged(object? sender, int e)
        {
            await Task.Run(async () =>
            {
                this.pictureControl.ResetPictures();
                await this.GetPictures();
            });
        }

        public async Task GetPictures()
        {
            var filter = await this.providerFilterControl.GetCurrentFilter();
            var settings = this.dbRepository.ApplicationSettings.Get();
            var providers = allProviders;

            Parallel.ForEach(providers, async (provider) =>
            {
                if (filter.Providers.Contains(provider.Provider))
                {
                    var result = await provider.GetPictures(filter.FilterElement);

                    if (result.Pictures.Count == 0 && !string.IsNullOrEmpty(result.ErrorCode))
                    {
                        if (result.ProviderEnd)
                        {
                            finishedProviders.Add(provider.Provider);
                        }

                        notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(localizer.Get(result.ErrorCode), provider.Provider)));
                    }
                    else if (result.ProviderEnd)
                    {
                        finishedProviders.Add(provider.Provider);
                        notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ToastProviderEnd, provider.Provider)));
                    }
                    else
                    {
                        foreach (var picture in result.Pictures)
                        {
                            if (picture != null)
                            {
                                if (!filter.Ratings.Contains(picture.Rating))
                                {
                                    continue;
                                }

                                if (filter.ExcludedTags.Any(e => picture.TagList.Any(ee => ee.WildcardMatches(e))))
                                {
                                    continue;
                                }

                                if (settings.BlacklistTags?.Any(t => picture.TagList.Any(tt => tt.WildcardMatches(t))) == true)
                                {
                                    continue;
                                }

                                if (picture.IsMedia)
                                {
                                    continue;
                                }

                                picture.IsOnline = true;
                                pictureControl.AddPicture(picture);
                            }
                        }
                    }
                }
            });
        }

        public async Task HookToFilter()
        {
            this.providerFilterControl.PageChanged += OnPageChanged;
            this.providerFilterControl.FilterChanged += OnFilterChanged;
        }

        public async Task UnhookFromFilter()
        {
            this.providerFilterControl.PageChanged -= OnPageChanged;
            this.providerFilterControl.FilterChanged -= OnFilterChanged;
        }
    }
}
