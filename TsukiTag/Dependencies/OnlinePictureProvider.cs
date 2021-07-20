using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies.ProviderSpecific;
using TsukiTag.Models;

namespace TsukiTag.Dependencies
{
    public interface IOnlinePictureProvider
    {
        Task GetPictures();
    }

    public class OnlinePictureProvider : IOnlinePictureProvider
    {
        private readonly ISafebooruPictureProvider safebooruPictureProvider;
        private readonly IProviderFilterControl providerFilterControl;
        private readonly IGelbooruPictureProvider gelbooruPictureProvider;
        private readonly IKonachanPictureProvider konachanPictureProvider;
        private readonly IDanbooruPictureProvider danbooruPictureProvider;

        private readonly IPictureControl pictureControl;
        private List<string> finishedProviders;

        public OnlinePictureProvider(
            ISafebooruPictureProvider safebooruPictureProvider,
            IGelbooruPictureProvider gelbooruPictureProvider,
            IKonachanPictureProvider konachanPictureProvider,
            IDanbooruPictureProvider danbooruPictureProvider,

            IProviderFilterControl providerFilterControl,
            IPictureControl pictureControl
        )
        {
            this.safebooruPictureProvider = safebooruPictureProvider;
            this.gelbooruPictureProvider = gelbooruPictureProvider;
            this.konachanPictureProvider = konachanPictureProvider;
            this.danbooruPictureProvider = danbooruPictureProvider;

            this.providerFilterControl = providerFilterControl;
            this.pictureControl = pictureControl;

            this.providerFilterControl.PageChanged += OnPageChanged;
            this.providerFilterControl.FilterChanged += OnFilterChanged;

            this.finishedProviders = new List<string>();
        }

        ~OnlinePictureProvider()
        {
            this.providerFilterControl.PageChanged -= OnPageChanged;
            this.providerFilterControl.FilterChanged -= OnFilterChanged;
        }

        private List<IOnlinePictureProviderElement> allProviders => new List<IOnlinePictureProviderElement>()
            { safebooruPictureProvider, gelbooruPictureProvider, konachanPictureProvider, danbooruPictureProvider }
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
            var providers = allProviders;

            Parallel.ForEach(providers, async (provider) =>
            {
                if (filter.Providers.Contains(provider.Provider))
                {
                    var result = await provider.GetPictures(filter.FilterElement);

                    if (result.ProviderEnd)
                    {
                        finishedProviders.Add(provider.Provider);
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

                                pictureControl.AddPicture(picture);
                            }
                        }
                    }

                }
            });
        }
    }
}
