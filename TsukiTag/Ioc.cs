﻿using SimpleInjector;
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
        public INavigationControl NavigationControl => container.GetInstance<INavigationControl>();
        public ILocalizer Localizer => container.GetInstance<ILocalizer>();
        public INotificationControl NotificationControl => container.GetInstance<INotificationControl>();
        public IDbRepository DbRepository => container.GetInstance<IDbRepository>();
        public IPictureProviderContext PictureProviderContext => container.GetInstance<IPictureProviderContext>();
        public IOnlineListPictureProvider OnlineListPictureProvider => container.GetInstance<IOnlineListPictureProvider>();
        public IPictureWorker PictureWorker => container.GetInstance<IPictureWorker>();
        public IWorkspacePictureProvider WorkspacePictureProvider => container.GetInstance<IWorkspacePictureProvider>();
        public IPictureDownloader PictureDownloader => container.GetInstance<IPictureDownloader>();

        private Container CreateContainer()
        {
            var container = new Container();

            container.Register<TagBarViewModel>(Lifestyle.Transient);
            container.Register<OnlineNavigationBarViewModel>(Lifestyle.Transient);
            container.Register<PictureListViewModel>(Lifestyle.Transient);
            container.Register<ProviderContextViewModel>(Lifestyle.Transient);
            container.Register<TagOverviewViewModel>(Lifestyle.Transient);
            container.Register<OnlineBrowserViewModel>(Lifestyle.Transient);
            container.Register<MetadataOverviewViewModel>(Lifestyle.Transient);
            container.Register<NotificationBarViewModel>(Lifestyle.Transient);
            container.Register<SettingsViewModel>(Lifestyle.Transient);
            container.Register<OnlineListBrowserViewModel>(Lifestyle.Transient);
            container.Register<OnlineListNavigationBarViewModel>(Lifestyle.Transient);
            container.Register<WorkspaceBrowserViewModel>(Lifestyle.Transient);
            container.Register<WorkspaceNavigationBarViewModel>(Lifestyle.Transient);

            container.Register<IOnlinePictureProvider, OnlinePictureProvider>(Lifestyle.Singleton);
            container.Register<ISafebooruPictureProvider, SafebooruPictureProvider>(Lifestyle.Singleton);
            container.Register<IGelbooruPictureProvider, GelbooruPictureProvider>(Lifestyle.Singleton);
            container.Register<IKonachanPictureProvider, KonachanPictureProvider>(Lifestyle.Singleton);
            container.Register<IDanbooruPictureProvider, DanbooruPictureProvider>(Lifestyle.Singleton);
            container.Register<IYanderePictureProvider, YanderePictureProvider>(Lifestyle.Singleton);
            container.Register<IR34PictureProvider, R34PictureProvider>(Lifestyle.Singleton);
            container.Register<IPictureDownloader, PictureDownloader>(Lifestyle.Singleton);
            container.Register<IPictureControl, PictureControl>(Lifestyle.Singleton);
            container.Register<IProviderFilterControl, ProviderFilterControl>(Lifestyle.Singleton);
            container.Register<INavigationControl, NavigationControl>(Lifestyle.Singleton);
            container.Register<IDbRepository, DbRepository>(Lifestyle.Singleton);
            container.Register<ILocalizer, Localizer>(Lifestyle.Singleton);
            container.Register<INotificationControl, NotificationControl>(Lifestyle.Singleton);
            container.Register<IPictureProviderContext, PictureProviderContext>(Lifestyle.Singleton);
            container.Register<IOnlineListPictureProvider, OnlineListPictureProvider>(Lifestyle.Singleton);
            container.Register<IPictureWorker, PictureWorker>(Lifestyle.Singleton);
            container.Register<IWorkspacePictureProvider, WorkspacePictureProvider>(Lifestyle.Singleton);

            container.Verify();

            return container;
        }
    }
}
