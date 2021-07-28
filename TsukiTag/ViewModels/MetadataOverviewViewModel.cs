using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;
namespace TsukiTag.ViewModels
{
    public class MetadataOverviewViewModel : ViewModelCollectionHandlerBase
    {
        private readonly IPictureControl pictureControl;
        private readonly IProviderFilterControl providerFilterControl;
        private readonly INavigationControl navigationControl;
        private readonly IDbRepository dbRepository;

        private string filterString;
        private int currentPictureIndex;
        private ObservableCollection<MenuItemViewModel> thisImageMenus;
        private ObservableCollection<MenuItemViewModel> selectionMenus;

        public ObservableCollection<Picture> SelectedPictures { get; set; }

        public bool HasSelectedPicture => SelectedPictures.Count > 0;

        public bool HasMultipleSelectedPicture => SelectedPictures.Count > 1;

        public int SelectedPictureCount => SelectedPictures.Count;

        public Picture CurrentPicture => SelectedPictures.Count != 0 && SelectedPictures.Count >= CurrentPictureIndex ? SelectedPictures[CurrentPictureIndex] : null;

        public int CurrentPictureIndex
        {
            get { return currentPictureIndex; }
            set
            {
                currentPictureIndex = value;

                this.RaisePropertyChanged(nameof(CurrentPictureIndex));
                this.RaisePropertyChanged(nameof(CurrentPictureIndexDisplay));
                this.RaisePropertyChanged(nameof(CurrentPicture));
                this.RaisePropertyChanged(nameof(FilterString));
                this.RaisePropertyChanged(nameof(FilteredTags));
                this.RaisePropertyChanged(nameof(TagCount));
            }
        }

        public string FilterString
        {
            get { return filterString; }
            set
            {
                filterString = value;
                this.RaisePropertyChanged(nameof(FilterString));
                this.RaisePropertyChanged(nameof(FilteredTags));
                this.RaisePropertyChanged(nameof(TagCount));
            }
        }

        public int TagCount => FilteredTags.Count;

        public List<string> FilteredTags
        {
            get
            {
                if (!string.IsNullOrEmpty(FilterString))
                {
                    var filterParts = FilterString.Split(' ').Where(s => !string.IsNullOrEmpty(s));
                    return CurrentPicture?.TagList.Where(s => filterParts.Any(fs => s.IndexOf(fs) > -1)).ToList() ?? new List<string>();
                }

                return CurrentPicture?.TagList ?? new List<string>();
            }
        }

        public string CurrentPictureIndexDisplay => (currentPictureIndex + 1).ToString();

        public ObservableCollection<MenuItemViewModel> ThisImageMenus
        {
            get { return thisImageMenus; }
            set
            {
                thisImageMenus = value;
                this.RaisePropertyChanged(nameof(ThisImageMenus));
            }
        }

        public ObservableCollection<MenuItemViewModel> SelectionMenus
        {
            get { return selectionMenus; }
            set
            {
                selectionMenus = value;
                this.RaisePropertyChanged(nameof(SelectionMenus));
            }
        }

        public ReactiveCommand<Unit, Unit> PreviousPictureCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NextPictureCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeselectPictureCommand { get; set; }
        public ReactiveCommand<Unit, Unit> OpenPictureCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SwitchToTabOverviewCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeselectAllCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SelectAllCommand { get; set; }

        public ReactiveCommand<Unit, Unit> SelectionAddToDefaultListCommand { get; protected set; }
        public ReactiveCommand<Guid, Unit> SelectionAddToSpecificListCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> SelectionAddToEligibleListsCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> SelectionAddToAllListsCommand { get; protected set; }
        public ReactiveCommand<Guid, Unit> SelectionRemoveFromSpecificListCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> SelectionRemoveFromAllListCommand { get; protected set; }


        public MetadataOverviewViewModel(
            IPictureControl pictureControl,
            IProviderFilterControl providerFilterControl,
            INavigationControl navigationControl,
            INotificationControl notificationControl,
            IDbRepository dbRepository
        ) : base(dbRepository, notificationControl)
        {
            this.SelectedPictures = new ObservableCollection<Picture>();

            this.pictureControl = pictureControl;
            this.providerFilterControl = providerFilterControl;
            this.navigationControl = navigationControl;
            this.dbRepository = dbRepository;

            this.pictureControl.PictureSelected += OnPictureSelected;
            this.pictureControl.PictureDeselected += OnPictureDeselected;
            this.navigationControl.SwitchedToAllOnlineListBrowsing += OnSwitchedToOnlineListBrowsing;
            this.navigationControl.SwitchedToOnlineBrowsing += OnSwitchedToOnlineBrowsing
                ;
            //this.pictureControl.PicturesReset += OnPicturesReset;

            this.PreviousPictureCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnPreviousPicture();
            });

            this.NextPictureCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnNextPicture();
            });

            this.DeselectPictureCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnDeselectPicture();
            });

            this.OpenPictureCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnOpenPicture();
            });

            this.SwitchToTabOverviewCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnSwitchToTagOverview();
            });

            this.DeselectAllCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnDeselectAll();
            });

            this.SelectAllCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnSelectAll();
            });

            this.AddToDefaultListCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnAddToDefaulList(CurrentPicture);
            });

            this.AddToSpecificListCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                OnAddToSpecificList(id, CurrentPicture);
            });

            this.AddToEligibleListsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnAddToEligibleLists(CurrentPicture);
            });

            this.AddToAllListsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnAddToAllLists(CurrentPicture);
            });

            this.RemoveFromSpecificListCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                OnRemoveFromSpecificList(id, CurrentPicture);
            });

            this.RemoveFromAllListCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnRemoveFromAllLists(CurrentPicture);
            });

            this.SelectionAddToDefaultListCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                foreach (var picture in SelectedPictures.ToList())
                {
                    await OnAddToDefaulList(picture, false);
                }

                await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionSelectionActionSuccess));
            });

            this.SelectionAddToSpecificListCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                foreach (var picture in SelectedPictures.ToList())
                {
                    await OnAddToSpecificList(id, picture, false);
                }

                await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionSelectionActionSuccess));
            });

            this.SelectionAddToEligibleListsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                foreach (var picture in SelectedPictures.ToList())
                {
                    await OnAddToEligibleLists(picture, false);
                }

                await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionSelectionActionSuccess));
            });

            this.SelectionAddToAllListsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                foreach (var picture in SelectedPictures.ToList())
                {
                    await OnAddToAllLists(picture, false);
                }

                await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionSelectionActionSuccess));
            });

            this.SelectionRemoveFromSpecificListCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                foreach (var picture in SelectedPictures.ToList())
                {
                    await OnRemoveFromSpecificList(id, picture, false);
                }

                await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionSelectionActionSuccess));
            });

            this.SelectionRemoveFromAllListCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                foreach (var picture in SelectedPictures.ToList())
                {
                    await OnRemoveFromAllLists(picture, false);
                }

                await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionSelectionActionSuccess));
            });
        }

        ~MetadataOverviewViewModel()
        {
            this.pictureControl.PictureSelected -= OnPictureSelected;
            this.pictureControl.PictureDeselected -= OnPictureDeselected;
            this.navigationControl.SwitchedToAllOnlineListBrowsing -= OnSwitchedToOnlineListBrowsing;
            this.navigationControl.SwitchedToOnlineBrowsing -= OnSwitchedToOnlineBrowsing;
        }

        public async void OnTagClicked(string tag)
        {
            await Task.Run(async () =>
            {
                await this.providerFilterControl.SetTag(tag);
            });
        }

        public async void OnTagAdded(string tag)
        {
            await Task.Run(async () =>
            {
                await this.providerFilterControl.AddTag(tag);
            });
        }

        public async void OnTagRemoved(string tag)
        {
            await Task.Run(async () =>
            {
                await this.providerFilterControl.RemoveTag(tag);
            });
        }

        public async void OnNextPicture()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if ((CurrentPictureIndex + 1) < SelectedPictures.Count)
                {
                    CurrentPictureIndex += 1;
                }
            });
        }

        public async void OnOpenPicture()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var picture = SelectedPictures.ElementAt(CurrentPictureIndex);
                if (picture != null)
                {
                    this.pictureControl.OpenPicture(picture);
                }
            });
        }

        private async void OnSwitchToTagOverview()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                this.navigationControl.SwitchToTagOverview();
            });
        }

        public async void OnOpenPictureInBackground()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var picture = SelectedPictures.ElementAt(CurrentPictureIndex);
                if (picture != null)
                {
                    this.pictureControl.OpenPictureInBackground(picture);
                }
            });
        }

        public async void OnDeselectPicture()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var picture = SelectedPictures.ElementAt(CurrentPictureIndex);
                if (picture != null)
                {
                    this.pictureControl.DeselectPicture(picture);
                }
            });
        }

        private async void OnSelectAll()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                this.pictureControl.SelectAllPictures();
            });
        }

        private async void OnDeselectAll()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                for(var i = SelectedPictures.Count - 1; i >= 0; i--)
                {
                    var picture = SelectedPictures.ElementAtOrDefault(CurrentPictureIndex);
                    if (picture != null)
                    {
                        this.pictureControl.DeselectPicture(picture);
                    }
                }
            });
        }

        public async void OnPreviousPicture()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if ((CurrentPictureIndex - 1) >= 0)
                {
                    CurrentPictureIndex -= 1;
                }
            });
        }

        private void OnSwitchedToOnlineBrowsing(object? sender, EventArgs e)
        {
            //OnPicturesReset(this, e);
        }

        private void OnSwitchedToOnlineListBrowsing(object? sender, EventArgs e)
        {
            //OnPicturesReset(this, e);
        }

        private void OnPicturesReset(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                SelectedPictures = new ObservableCollection<Picture>();

                this.RaisePropertyChanged(nameof(CurrentPicture));
                this.RaisePropertyChanged(nameof(SelectedPictures));
                this.RaisePropertyChanged(nameof(SelectedPictureCount));
                this.RaisePropertyChanged(nameof(CurrentPictureIndex));
                this.RaisePropertyChanged(nameof(HasSelectedPicture));
                this.RaisePropertyChanged(nameof(HasMultipleSelectedPicture));
                this.RaisePropertyChanged(nameof(CurrentPictureIndexDisplay));
            });
        }

        private void OnPictureDeselected(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                SelectedPictures.Remove(e);

                if (CurrentPictureIndex >= SelectedPictures.Count)
                {
                    CurrentPictureIndex = SelectedPictures.Count - 1;
                }

                this.RaisePropertyChanged(nameof(CurrentPicture));
                this.RaisePropertyChanged(nameof(SelectedPictures));
                this.RaisePropertyChanged(nameof(SelectedPictureCount));
                this.RaisePropertyChanged(nameof(CurrentPictureIndex));
                this.RaisePropertyChanged(nameof(HasSelectedPicture));
                this.RaisePropertyChanged(nameof(HasMultipleSelectedPicture));
            });
        }

        private void OnPictureSelected(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var picture = SelectedPictures.FirstOrDefault(p => p.Md5 == e.Md5);
                if (picture != null)
                {
                    CurrentPictureIndex = SelectedPictures.IndexOf(picture);
                }
                else
                {
                    SelectedPictures.Add(e);
                    CurrentPictureIndex = SelectedPictures.Count - 1;
                }

                if (thisImageMenus == null || selectionMenus == null)
                {
                    Reinitialize();
                }

                this.RaisePropertyChanged(nameof(CurrentPicture));
                this.RaisePropertyChanged(nameof(SelectedPictures));
                this.RaisePropertyChanged(nameof(SelectedPictureCount));
                this.RaisePropertyChanged(nameof(CurrentPictureIndex));
                this.RaisePropertyChanged(nameof(HasSelectedPicture));
                this.RaisePropertyChanged(nameof(HasMultipleSelectedPicture));
                this.RaisePropertyChanged(nameof(CurrentPictureIndexDisplay));
            });
        }

        private ObservableCollection<MenuItemViewModel> GetThisImageMenus()
        {
            var menus = new MenuItemViewModel();
            var allLists = this.dbRepository.OnlineList.GetAll();

            menus.Header = Language.ActionThisImage;

            var onlineListMenus = new MenuItemViewModel();
            onlineListMenus.Header = Language.ActionOnlineLists;
            onlineListMenus.Items = new List<MenuItemViewModel>() {
                {
                    new MenuItemViewModel()
                    {
                        Header = $"{Language.ActionAddToDefault} ({allLists.FirstOrDefault(s => s.IsDefault)?.Name})",
                        Command = AddToDefaultListCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionAddToEligible,
                        Command = AddToEligibleListsCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionAddTo,
                        Items = new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = AddToAllListsCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(allLists.Select(l => new MenuItemViewModel() { Header = l.Name, Command = AddToSpecificListCommand, CommandParameter = l.Id })))
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = "-"
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionRemoveFromAll,
                        Command = RemoveFromAllListCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionRemoveFrom,
                        Items = new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = RemoveFromAllListCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(allLists.Select(l => new MenuItemViewModel() { Header = l.Name, Command = RemoveFromSpecificListCommand,  CommandParameter = l.Id }))),
                    }
                }
            };

            var imageMenus = new MenuItemViewModel();
            imageMenus.Header = Language.ActionThisImage;
            imageMenus.Items = new List<MenuItemViewModel>()
            {
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.OpenPicture,
                        Command = OpenPictureCommand
                    }
                }
            };

            menus.Items = new List<MenuItemViewModel>()
            {
                {
                    onlineListMenus
                },
                {
                    imageMenus
                }
            };

            return new ObservableCollection<MenuItemViewModel>() { menus };
        }

        private ObservableCollection<MenuItemViewModel> GetSelectionMenus()
        {
            var menus = new MenuItemViewModel();
            var allLists = this.dbRepository.OnlineList.GetAll();

            menus.Header = Language.ActionSelection;

            var onlineListMenus = new MenuItemViewModel();
            onlineListMenus.Header = Language.ActionOnlineLists;
            onlineListMenus.Items = new List<MenuItemViewModel>() {
                {
                    new MenuItemViewModel()
                    {
                        Header = $"{Language.ActionAddToDefault} ({allLists.FirstOrDefault(s => s.IsDefault)?.Name})",
                        Command = SelectionAddToDefaultListCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionAddToEligible,
                        Command = SelectionAddToEligibleListsCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionAddTo,
                        Items = new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = SelectionAddToAllListsCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(allLists.Select(l => new MenuItemViewModel() { Header = l.Name, Command = SelectionAddToSpecificListCommand, CommandParameter = l.Id })))
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = "-"
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionRemoveFromAll,
                        Command = SelectionRemoveFromAllListCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionRemoveFrom,
                        Items = new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = SelectionRemoveFromAllListCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(allLists.Select(l => new MenuItemViewModel() { Header = l.Name, Command = SelectionRemoveFromSpecificListCommand,  CommandParameter = l.Id }))),
                    }
                }
            };

            var selectionMenus = new MenuItemViewModel();
            selectionMenus.Header = Language.ActionSelection;
            selectionMenus.Items = new List<MenuItemViewModel>()
            {
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.SelectAll,
                        Command = SelectAllCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.DeselectAllPictures,
                        Command = DeselectAllCommand
                    }
                }
            };

            menus.Items = new List<MenuItemViewModel>()
            {
                {
                    onlineListMenus
                },
                {
                    selectionMenus
                }
            };

            return new ObservableCollection<MenuItemViewModel>() { menus };
        }

        public override void Reinitialize()
        {
            this.ThisImageMenus = GetThisImageMenus();
            this.SelectionMenus = GetSelectionMenus();
        }
    }
}
