using Avalonia.Controls;
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
    public class OnlineProviderViewModel : ViewModelBase
    {
        private readonly IOnlinePictureProvider onlinePictureProvider;
        private readonly IPictureControl pictureControl;
        private ObservableCollection<ProviderTabModel> tabs;
        private int selectedTabIndex;
        private ContentControl pictureContextContent;
        private int selectedPictureCount;

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
                return selectedPictureCount > 0;
            }
        }

        public bool NoSelectedPictures
        {
            get
            {
                return selectedPictureCount == 0;
            }
        }

        public OnlineProviderViewModel(
            IOnlinePictureProvider onlinePictureProvider,
            IPictureControl pictureControl
        )
        {
            this.onlinePictureProvider = onlinePictureProvider;
            this.pictureControl = pictureControl;
        }

        ~OnlineProviderViewModel()
        {
            this.pictureControl.PictureOpened -= OnPictureOpened;
            this.pictureControl.PictureClosed -= OnPictureClosed;
            this.pictureControl.PictureOpenedInBackground -= OnPictureOpenedInBackground;
            this.pictureControl.PictureSelected -= OnPictureSelected;
            this.pictureControl.PictureDeselected -= OnPictureDeselected;
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

            await Task.Run(async () =>
            {
                await onlinePictureProvider.GetPictures();
            });
        }

        private void OnPictureDeselected(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                SelectedPictureCount = await this.pictureControl.GetSelectedPictureCount();
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
