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

        public static OnlineProviderViewModel OnlineProviderViewModel => ViewModelLocator.GetDataContext<OnlineProviderViewModel>();

        public static OnlineBrowserViewModel OnlineBrowserViewModel => ViewModelLocator.GetDataContext<OnlineBrowserViewModel>();


    }
}
