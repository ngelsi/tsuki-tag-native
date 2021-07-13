using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Dependencies.ProviderSpecific;
using TsukiTag.ViewModels;

namespace TsukiTag
{
    public class Ioc
    {
        private static Ioc ioc;

        public static Ioc SimpleIoc
        {
            get
            {
                if (ioc == null)
                {
                    ioc = new Ioc();
                }

                return ioc;
            }
        }

        private Container container;

        private Ioc()
        {
            container = CreateContainer();
        }

        public T GetDataContext<T>() where T : class
        {
            return container.GetInstance<T>();
        }

        public IPictureControl PictureControl => container.GetInstance<IPictureControl>();
        public IProviderFilterControl ProviderFilterControl => container.GetInstance<IProviderFilterControl>();
        public IOnlinePictureProvider OnlinePictureProvider => container.GetInstance<IOnlinePictureProvider>();
        public IProviderFilterControl OnlineProviderFilterControl => container.GetInstance<IProviderFilterControl>();


        private Container CreateContainer()
        {
            var container = new Container();

            container.Register<TagBarViewModel>(Lifestyle.Transient);
            container.Register<OnlineNavigationBarViewModel>(Lifestyle.Transient);
            container.Register<PictureListViewModel>(Lifestyle.Transient);
            container.Register<OnlineProviderViewModel>(Lifestyle.Transient);
            container.Register<TagOverviewViewModel>(Lifestyle.Transient);
            container.Register<OnlineBrowserViewModel>(Lifestyle.Transient);

            container.Register<IOnlinePictureProvider, OnlinePictureProvider>(Lifestyle.Singleton);
            container.Register<ISafebooruPictureProvider, SafebooruPictureProvider>(Lifestyle.Singleton);
            container.Register<IPictureDownloader, PictureDownloader>(Lifestyle.Singleton);
            container.Register<IPictureControl, PictureControl>(Lifestyle.Singleton);
            container.Register<IProviderFilterControl, ProviderFilterControl>(Lifestyle.Singleton);

            container.Verify();

            return container;
        }
    }
}
