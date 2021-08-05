using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models;
using TsukiTag.Models.Repository;

namespace TsukiTag.Dependencies
{
    public interface IPictureProvider
    {
        Task<Picture> RedownloadPicture(Picture picture);

        Task GetPictures();

        Task HookToFilter();

        Task UnhookFromFilter();
    }

    public interface IPictureProviderContext : IPictureProvider
    {
        Task SetContextToOnline();

        Task SetContextToAllOnlineLists();

        Task SetContextToSpecificOnlineList(Guid id);

        Task SetContextToAllWorkspaces();

        Task SetContextToSpecificWorkspace(Guid id);
    }

    public class PictureProviderContext : IPictureProviderContext
    {
        private readonly IOnlinePictureProvider onlinePictureProvider;
        private readonly IOnlineListPictureProvider onlineListPictureProvider;
        private readonly IWorkspacePictureProvider workspacePictureProvider;
        private readonly IPictureControl pictureControl;

        private readonly IProviderFilterControl providerFilterControl;

        private IPictureProvider currentProvider;

        public PictureProviderContext(
            IOnlinePictureProvider onlinePictureProvider,
            IOnlineListPictureProvider onlineListPictureProvider,
            IWorkspacePictureProvider workspacePictureProvider,
            IPictureControl pictureControl,
            IProviderFilterControl providerFilterControl
        )
        {
            this.providerFilterControl = providerFilterControl;
            this.pictureControl = pictureControl;
            this.onlineListPictureProvider = onlineListPictureProvider;
            this.onlinePictureProvider = onlinePictureProvider;
            this.workspacePictureProvider = workspacePictureProvider;
        }

        public async Task<Picture> RedownloadPicture(Picture picture)
        {
            return await onlinePictureProvider.RedownloadPicture(picture);
        }

        public Task GetPictures()
        {
            return this.currentProvider.GetPictures();
        }

        public async Task SetContextToOnline()
        {
            if (this.currentProvider != null)
            {
                await this.currentProvider.UnhookFromFilter();
            }

            this.currentProvider = this.onlinePictureProvider;

            await this.pictureControl.SwitchPictureContext();
            await this.currentProvider.HookToFilter();
            await this.providerFilterControl.ReinitializeFilter(ProviderSession.OnlineProviderSession);
        }

        public async Task SetContextToAllOnlineLists()
        {
            if (this.currentProvider != null)
            {
                await this.currentProvider.UnhookFromFilter();
            }

            this.currentProvider = this.onlineListPictureProvider;

            await this.pictureControl.SwitchPictureContext();
            await this.currentProvider.HookToFilter();
            await this.providerFilterControl.ReinitializeFilter(ProviderSession.AllOnlineListsSession);
        }

        public async Task SetContextToSpecificOnlineList(Guid id)
        {
            if (this.currentProvider != null)
            {
                await this.currentProvider.UnhookFromFilter();
            }

            this.currentProvider = this.onlineListPictureProvider;

            await this.pictureControl.SwitchPictureContext();
            await this.currentProvider.HookToFilter();
            await this.providerFilterControl.ReinitializeFilter(id.ToString());
        }

        public async Task SetContextToAllWorkspaces()
        {
            if (this.currentProvider != null)
            {
                await this.currentProvider.UnhookFromFilter();
            }

            this.currentProvider = this.workspacePictureProvider;

            await this.pictureControl.SwitchPictureContext();
            await this.currentProvider.HookToFilter();
            await this.providerFilterControl.ReinitializeFilter(ProviderSession.AllWorkspacesSession);
        }

        public async Task SetContextToSpecificWorkspace(Guid id)
        {
            if (this.currentProvider != null)
            {
                await this.currentProvider.UnhookFromFilter();
            }

            this.currentProvider = this.workspacePictureProvider;

            await this.pictureControl.SwitchPictureContext();
            await this.currentProvider.HookToFilter();
            await this.providerFilterControl.ReinitializeFilter(id.ToString());
        }

        public Task UnhookFromFilter()
        {
            return this.currentProvider.UnhookFromFilter();
        }

        public Task HookToFilter()
        {
            return this.currentProvider.HookToFilter();
        }
    }
}
