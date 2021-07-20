using System;
using System.Collections.Generic;
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

        private readonly IPictureControl pictureControl;

        public OnlinePictureProvider(
            ISafebooruPictureProvider safebooruPictureProvider,
            IGelbooruPictureProvider gelbooruPictureProvider,

            IProviderFilterControl providerFilterControl,
            IPictureControl pictureControl
        )
        {
            this.safebooruPictureProvider = safebooruPictureProvider;
            this.gelbooruPictureProvider = gelbooruPictureProvider;

            this.providerFilterControl = providerFilterControl;
            this.pictureControl = pictureControl;

            this.providerFilterControl.PageChanged += OnPageChanged;
            this.providerFilterControl.FilterChanged += OnFilterChanged;
        }

        ~OnlinePictureProvider()
        {
            this.providerFilterControl.PageChanged -= OnPageChanged;
            this.providerFilterControl.FilterChanged -= OnFilterChanged;
        }

        private List<IOnlinePictureProviderElement> allProviders => new List<IOnlinePictureProviderElement>() { safebooruPictureProvider, gelbooruPictureProvider };

        private async void OnFilterChanged(object? sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
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

            Parallel.ForEach(allProviders, async (provider) =>
            {
                if(filter.Providers.Contains(provider.Provider))
                {
                    var result = await provider.GetPictures(filter.FilterElement);
                    foreach (var picture in result.Pictures)
                    {
                        if(picture != null)
                        {
                            if(!filter.Ratings.Contains(picture.Rating))
                            {
                                continue;
                            }

                            pictureControl.AddPicture(picture);
                        }
                    }
                }
            });
        }
    }
}
