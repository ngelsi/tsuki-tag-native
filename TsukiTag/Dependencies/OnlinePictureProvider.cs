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
        private readonly IProviderFilterControl onlineProviderFilterControl;

        private readonly IPictureControl pictureControl;

        public OnlinePictureProvider(
            ISafebooruPictureProvider safebooruPictureProvider,

            IProviderFilterControl onlineProviderFilterControl,
            IPictureControl pictureControl
        )
        {
            this.safebooruPictureProvider = safebooruPictureProvider;

            this.onlineProviderFilterControl = onlineProviderFilterControl;
            this.pictureControl = pictureControl;

            this.onlineProviderFilterControl.PageChanged += OnPageChanged;
            this.onlineProviderFilterControl.FilterChanged += OnFilterChanged;
        }

        ~OnlinePictureProvider()
        {
            this.onlineProviderFilterControl.PageChanged -= OnPageChanged;
            this.onlineProviderFilterControl.FilterChanged -= OnFilterChanged;
        }

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
            var result = await safebooruPictureProvider.GetPictures(this.onlineProviderFilterControl.CurrentFilter.FilterElement);
            foreach (var picture in result.Pictures)
            {
                pictureControl.AddPicture(picture);
            }
        }
    }
}
