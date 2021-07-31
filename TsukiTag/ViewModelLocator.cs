using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.ViewModels;

namespace TsukiTag
{
    public static class ViewModelLocator
    {
        public static T GetDataContext<T>() where T : class => Ioc.SimpleIoc.GetDataContext<T>();
    }

    public static class ViewModelIoc
    {
        public static TagBarViewModel TagBarViewModel => ViewModelLocator.GetDataContext<TagBarViewModel>();

        public static TagOverviewViewModel TagOverviewViewModel => ViewModelLocator.GetDataContext<TagOverviewViewModel>();

        public static OnlineNavigationBarViewModel OnlineNavigationBarViewModel => ViewModelLocator.GetDataContext<OnlineNavigationBarViewModel>();

        public static PictureListViewModel PictureListViewModel => ViewModelLocator.GetDataContext<PictureListViewModel>();

        public static ProviderContextViewModel ProviderContextViewModel => ViewModelLocator.GetDataContext<ProviderContextViewModel>();

        public static OnlineBrowserViewModel OnlineBrowserViewModel => ViewModelLocator.GetDataContext<OnlineBrowserViewModel>();

        public static MetadataOverviewViewModel MetadataOverviewViewModel => ViewModelLocator.GetDataContext<MetadataOverviewViewModel>();

        public static NotificationBarViewModel NotificationBarViewModel => ViewModelLocator.GetDataContext<NotificationBarViewModel>();

        public static SettingsViewModel SettingsViewModel => ViewModelLocator.GetDataContext<SettingsViewModel>();

        public static OnlineListBrowserViewModel OnlineListBrowserViewModel => ViewModelLocator.GetDataContext<OnlineListBrowserViewModel>();

        public static OnlineListNavigationBarViewModel OnlineListNavigationBarViewModel => ViewModelLocator.GetDataContext<OnlineListNavigationBarViewModel>();
        
        public static WorkspaceBrowserViewModel WorkspaceBrowserViewModel => ViewModelLocator.GetDataContext<WorkspaceBrowserViewModel>();
        
        public static WorkspaceNavigationBarViewModel WorkspaceNavigationBarViewModel => ViewModelLocator.GetDataContext<WorkspaceNavigationBarViewModel>();
    }
}
