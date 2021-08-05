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
    public interface IWorkspacePictureProvider : IPictureProvider
    {

    }

    public class WorkspacePictureProvider : IWorkspacePictureProvider
    {
        private readonly IPictureControl pictureControl;
        private readonly IProviderFilterControl providerFilterControl;
        private readonly INotificationControl notificationControl;
        private readonly ILocalizer localizer;
        private readonly IDbRepository dbRepository;

        public WorkspacePictureProvider(
            IProviderFilterControl providerFilterControl,
            IPictureControl pictureControl,
            INotificationControl notificationControl,
            ILocalizer localizer,
            IDbRepository dbRepository
)
        {
            this.providerFilterControl = providerFilterControl;
            this.notificationControl = notificationControl;
            this.pictureControl = pictureControl;
            this.localizer = localizer;
            this.dbRepository = dbRepository;
        }

        ~WorkspacePictureProvider()
        {
            this.providerFilterControl.PageChanged -= OnPageChanged;
            this.providerFilterControl.FilterChanged -= OnFilterChanged;
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

        public async Task GetPictures()
        {
            var filter = await this.providerFilterControl.GetCurrentFilter();
            var pictureContext = await this.pictureControl.GetPictureContext();
            var pictures = this.dbRepository.WorkspacePicture.GetAllForFilter(filter);

            if (pictures == null || pictures.Count == 0)
            {
                notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(filter.Providers?.Count == 1 ? Language.ToastWorkspaceEnd : Language.ToastWorkspacesEnd, filter.Providers?.FirstOrDefault())));
            }
            else
            {
                foreach (var picture in pictures)
                {
                    if (picture?.Picture != null)
                    {
                        if (picture.Picture.Rating != Rating.Unknown.Name && !filter.Ratings.Contains(picture.Picture.Rating))
                        {
                            continue;
                        }

                        if (filter.ExcludedTags.Any(e => picture.Picture.TagList.Any(ee => ee.WildcardMatches(e))))
                        {
                            continue;
                        }

                        picture.Picture.PictureContext = pictureContext;
                        pictureControl.AddPicture(picture.Picture);
                    }
                }
            }
        }

        public async Task<Picture> RedownloadPicture(Picture picture)
        {
            return null;
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
    }
}
