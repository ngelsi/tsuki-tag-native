using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;
using TsukiTag.Views;

namespace TsukiTag.ViewModels
{
    public class ProviderContextViewModel : ViewModelBase
    {
        private readonly IPictureProviderContext pictureProviderContext;
        private readonly IPictureControl pictureControl;
        private readonly INavigationControl navigationControl;

        private ObservableCollection<ProviderTabModel> tabs;
        private int selectedTabIndex;
        private ContentControl pictureContextContent;
        private int selectedPictureCount;
        private bool enforceTagOverview;
        private bool enforceMetadataOverview;
        private bool tagOverviewWasEnforced;

        public ObservableCollection<ProviderTabModel> Tabs
        {
            get { return tabs; }
            set
            {
                tabs = value; this.RaisePropertyChanged(nameof(Tabs));
                this.RaisePropertyChanged(nameof(SelectedTabIndex));
                this.RaisePropertyChanged(nameof(IsBrowserContext));
                this.RaisePropertyChanged(nameof(IsPictureContext));
                this.RaisePropertyChanged(nameof(CurrentPicture));
                this.RaisePropertyChanged(nameof(CurrentPictureContext));
            }
        }

        public int SelectedTabIndex
        {
            get { return selectedTabIndex; }
            set
            {
                selectedTabIndex = value;

                this.pictureContextContent = new PictureMetadataEditor(CurrentPicture);

                this.RaisePropertyChanged(nameof(SelectedTabIndex));
                this.RaisePropertyChanged(nameof(IsBrowserContext));
                this.RaisePropertyChanged(nameof(IsPictureContext));
                this.RaisePropertyChanged(nameof(CurrentPicture));
                this.RaisePropertyChanged(nameof(CurrentPictureContext));
            }
        }

        public int SelectedPictureCount
        {
            get { return selectedPictureCount; }
            set
            {
                selectedPictureCount = value;
                this.RaisePropertyChanged(nameof(SelectedPictureCount));
                this.RaisePropertyChanged(nameof(HasSelectedPictures));
                this.RaisePropertyChanged(nameof(NoSelectedPictures));
            }
        }

        public bool IsBrowserContext
        {
            get
            {
                return SelectedTabIndex == 0;
            }
        }

        public bool IsPictureContext
        {
            get
            {
                return SelectedTabIndex != 0;
            }
        }

        public Picture CurrentPicture
        {
            get
            {
                return (this.Tabs?.ElementAtOrDefault(SelectedTabIndex)?.Context as Picture);
            }
        }

        public ContentControl CurrentPictureContext
        {
            get { return pictureContextContent; }
            set { pictureContextContent = value; }
        }

        public bool HasSelectedPictures
        {
            get
            {
                if (EnforceTagOverview)
                {
                    return false;
                }

                return EnforceMetadataOverview || selectedPictureCount > 0;
            }
        }

        public bool NoSelectedPictures
        {
            get
            {
                if (EnforceMetadataOverview)
                {
                    return false;
                }

                return EnforceTagOverview || selectedPictureCount == 0;
            }
        }

        public bool EnforceTagOverview
        {
            get { return enforceTagOverview; }
            set
            {
                enforceTagOverview = value;

                this.RaisePropertyChanged(nameof(EnforceTagOverview));
                this.RaisePropertyChanged(nameof(EnforceMetadataOverview));
                this.RaisePropertyChanged(nameof(NoSelectedPictures));
                this.RaisePropertyChanged(nameof(HasSelectedPictures));
            }
        }

        public bool EnforceMetadataOverview
        {
            get { return enforceMetadataOverview; }
            set
            {
                enforceMetadataOverview = value;

                this.RaisePropertyChanged(nameof(EnforceTagOverview));
                this.RaisePropertyChanged(nameof(EnforceMetadataOverview));
                this.RaisePropertyChanged(nameof(NoSelectedPictures));
                this.RaisePropertyChanged(nameof(HasSelectedPictures));
            }
        }

        public ProviderContextViewModel(
            IPictureProviderContext pictureProviderContext,
            IPictureControl pictureControl,
            INavigationControl navigationControl
        )
        {
            this.navigationControl = navigationControl;
            this.pictureProviderContext = pictureProviderContext;
            this.pictureControl = pictureControl;
        }

        ~ProviderContextViewModel()
        {
            this.pictureControl.PictureOpened -= OnPictureOpened;
            this.pictureControl.PictureClosed -= OnPictureClosed;
            this.pictureControl.PictureOpenedInBackground -= OnPictureOpenedInBackground;
            this.pictureControl.PictureSelected -= OnPictureSelected;
            this.pictureControl.PictureDeselected -= OnPictureDeselected;
            this.navigationControl.SwitchedToMetadataOverview -= OnSwitchedToMetadataOverview;
            this.navigationControl.SwitchedToTagOverview -= OnSwitchedToTagOverview;
            this.navigationControl.TemporaryMetadataOverviewEnd -= OnTemporaryMetadataOverviewEnd;
            this.navigationControl.TemporaryMetadataOverviewStart -= OnTemporaryMetadataOverviewStart;
            this.navigationControl.SwitchedToOnlineBrowsing -= OnSwitchedToOnlineBrowsing;
            this.navigationControl.SwitchedToAllOnlineListBrowsing -= OnSwitchedToAllOnlineListBrowsing;
            this.navigationControl.SwitchedToSpecificOnlineListBrowsing -= OnSwitchedToSpecificOnlineListBrowsing;
        }

        public async void OnTabPictureClosed(Picture picture)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if (picture != null)
                {
                    this.pictureControl.ClosePicture(picture);
                }
            });
        }

        private void OnPictureClosed(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var tab = this.tabs.FirstOrDefault(t => (t.Context is Picture picture) && picture.Md5 == e.Md5);
                if (tab != null)
                {
                    var index = this.tabs.IndexOf(tab);

                    if (selectedTabIndex >= index)
                    {
                        selectedTabIndex -= 1;
                    }

                    var oldIndex = selectedTabIndex;

                    this.tabs.Remove(tab);

                    this.RaisePropertyChanged(nameof(SelectedTabIndex));
                    this.RaisePropertyChanged(nameof(Tabs));

                    SelectedTabIndex = oldIndex;
                    this.RaisePropertyChanged(nameof(SelectedTabIndex));
                    this.RaisePropertyChanged(nameof(Tabs));
                }
            });
        }

        private void OnPictureOpened(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if (!this.Tabs.Any(t => t.Identifier == e.Md5))
                {
                    this.Tabs.Add(new ProviderTabModel()
                    {
                        Header = e.Id ?? e.Md5,
                        Identifier = e.Md5,
                        Context = e,
                        Content = new PictureDetail(e)
                    });

                    this.SelectedTabIndex = this.Tabs.Count - 1;
                    this.RaisePropertyChanged(nameof(Tabs));
                    this.RaisePropertyChanged(nameof(SelectedTabIndex));
                }
                else
                {
                    var tab = this.Tabs.FirstOrDefault(t => t.Identifier == e.Md5);
                    if (tab != null)
                    {
                        this.SelectedTabIndex = this.Tabs.IndexOf(tab);
                        this.RaisePropertyChanged(nameof(Tabs));
                        this.RaisePropertyChanged(nameof(SelectedTabIndex));
                    }
                }
            });
        }

        public async void Initialize()
        {
            if (this.tabs == null || this.tabs.Count == 0)
            {
                this.tabs = new ObservableCollection<ProviderTabModel>();
                this.tabs.Add(ProviderTabModel.OnlineBrowserTab);

                this.RaisePropertyChanged(nameof(Tabs));
            }

            this.pictureControl.PictureOpened += OnPictureOpened;
            this.pictureControl.PictureClosed += OnPictureClosed;
            this.pictureControl.PictureSelected += OnPictureSelected;
            this.pictureControl.PictureDeselected += OnPictureDeselected;
            this.pictureControl.PictureOpenedInBackground += OnPictureOpenedInBackground;
            this.navigationControl.SwitchedToMetadataOverview += OnSwitchedToMetadataOverview;
            this.navigationControl.SwitchedToTagOverview += OnSwitchedToTagOverview;
            this.navigationControl.TemporaryMetadataOverviewEnd += OnTemporaryMetadataOverviewEnd;
            this.navigationControl.TemporaryMetadataOverviewStart += OnTemporaryMetadataOverviewStart;
            this.navigationControl.SwitchedToOnlineBrowsing += OnSwitchedToOnlineBrowsing;
            this.navigationControl.SwitchedToAllOnlineListBrowsing += OnSwitchedToAllOnlineListBrowsing;
            this.navigationControl.SwitchedToSpecificOnlineListBrowsing += OnSwitchedToSpecificOnlineListBrowsing;

            this.GetPictures();
        }

        private async Task GetPictures()
        {
            await Task.Run(async () =>
            {
                await pictureProviderContext.GetPictures();
            });
        }

        private void OnSwitchedToSpecificOnlineListBrowsing(object? sender, Guid e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                //*
                this.tabs.Remove(this.tabs.ElementAt(0));
                this.tabs.Insert(0, ProviderTabModel.AllOnlineListsTab);
                this.SelectedTabIndex = 0;
                /*/
                this.tabs = new ObservableCollection<ProviderTabModel>();
                this.tabs.Add(ProviderTabModel.AllOnlineListsTab);
                //*/

                this.RaisePropertyChanged(nameof(Tabs));

                await this.pictureProviderContext.SetContextToSpecificOnlineList(e);
            });
        }

        private void OnSwitchedToAllOnlineListBrowsing(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                //*
                this.tabs.Remove(this.tabs.ElementAt(0));
                this.tabs.Insert(0, ProviderTabModel.AllOnlineListsTab);
                this.SelectedTabIndex = 0;
                /*/
                this.tabs = new ObservableCollection<ProviderTabModel>();
                this.tabs.Add(ProviderTabModel.AllOnlineListsTab);
                //*/

                this.RaisePropertyChanged(nameof(Tabs));

                await this.pictureProviderContext.SetContextToAllOnlineLists();
            });
        }

        private void OnSwitchedToOnlineBrowsing(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                //*
                this.tabs.Remove(this.tabs.ElementAt(0));
                this.tabs.Insert(0, ProviderTabModel.OnlineBrowserTab);
                this.SelectedTabIndex = 0;
                /*/
                this.tabs = new ObservableCollection<ProviderTabModel>();
                this.tabs.Add(ProviderTabModel.OnlineBrowserTab);
                //*/

                this.RaisePropertyChanged(nameof(Tabs));

                await this.pictureProviderContext.SetContextToOnline();
            });
        }

        private void OnTemporaryMetadataOverviewStart(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                this.tagOverviewWasEnforced = EnforceTagOverview;
                OnSwitchedToMetadataOverview(this, e);
            });
        }

        private void OnTemporaryMetadataOverviewEnd(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if (this.tagOverviewWasEnforced)
                {
                    OnSwitchedToTagOverview(this, e);
                }
                else
                {
                    var count = await this.pictureControl.GetSelectedPictureCount();
                    if(count > 0)
                    {
                        OnSwitchedToMetadataOverview(this, e);
                    }                    
                    else
                    {
                        EnforceMetadataOverview = false;
                        EnforceTagOverview = false;
                    }
                }
            });
        }

        private void OnSwitchedToTagOverview(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                EnforceMetadataOverview = false;
                EnforceTagOverview = true;
            });
        }

        private void OnSwitchedToMetadataOverview(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                EnforceTagOverview = false;
                EnforceMetadataOverview = true;
            });
        }

        private void OnPictureDeselected(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var count = await this.pictureControl.GetSelectedPictureCount();
                SelectedPictureCount = count;

                if (count == 0)
                {
                    this.OnSwitchedToTagOverview(this, EventArgs.Empty);
                }
            });
        }

        private void OnPictureSelected(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                SelectedPictureCount = await this.pictureControl.GetSelectedPictureCount();
            });
        }

        private void OnPictureOpenedInBackground(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if (!this.Tabs.Any(t => t.Identifier == e.Md5))
                {
                    this.Tabs.Add(new ProviderTabModel()
                    {
                        Header = e.Id ?? e.Md5,
                        Identifier = e.Md5,
                        Content = new PictureDetail(e),
                        Context = e
                    });

                    this.RaisePropertyChanged(nameof(Tabs));
                }
            });
        }
    }
}
