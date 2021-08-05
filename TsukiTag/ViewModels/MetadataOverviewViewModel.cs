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
using TsukiTag.Extensions;
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
                    return CurrentPicture?.TagList.Where(s => filterParts.Any(fs => s.WildcardMatchesEx(fs))).ToList() ?? new List<string>();
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

        public ReactiveCommand<Unit, Unit> SelectionAddToDefaultWorkspaceCommand { get; protected set; }
        public ReactiveCommand<Guid, Unit> SelectionAddToSpecificWorkspaceCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> SelectionAddToEligibleWorkspacesCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> SelectionAddToAllWorkspacesCommand { get; protected set; }
        public ReactiveCommand<Guid, Unit> SelectionRemoveFromSpecificWorkspaceCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> SelectionRemoveFromAllWorkspaceCommand { get; protected set; }

        public ReactiveCommand<Guid, Unit> SelectionApplyMetadataGroupCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> SelectionSavePictureChangesCommand { get; protected set; }


        public MetadataOverviewViewModel(
            IPictureControl pictureControl,
            IProviderFilterControl providerFilterControl,
            INavigationControl navigationControl,
            INotificationControl notificationControl,
            IDbRepository dbRepository,
            IPictureWorker pictureWorker
        ) : base(dbRepository, pictureWorker, notificationControl)
        {
            this.SelectedPictures = new ObservableCollection<Picture>();

            this.pictureControl = pictureControl;
            this.providerFilterControl = providerFilterControl;
            this.navigationControl = navigationControl;
            this.dbRepository = dbRepository;

            this.pictureControl.PictureSelected += OnPictureSelected;
            this.pictureControl.PictureDeselected += OnPictureDeselected;
            this.navigationControl.SwitchedToAllOnlineListBrowsing += OnSwitchedToBrowsingContext;
            this.navigationControl.SwitchedToOnlineBrowsing += OnSwitchedToBrowsingContext;
            this.navigationControl.SwitchedToAllWorkspaceBrowsing += OnSwitchedToBrowsingContext;
            this.navigationControl.SwitchedToSpecificOnlineListBrowsing += OnSwitchedToBrowsingContext;
            this.navigationControl.SwitchedToSpecificWorkspaceBrowsing += OnSwitchedToBrowsingContext;
            this.dbRepository.OnlineList.OnlineListsChanged += OnResourceListChanged;
            this.dbRepository.Workspace.WorkspacesChanged += OnResourceListChanged;

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

            this.OpenInDefaultApplicationCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnOpenInDefaultApplication(CurrentPicture);
            });

            this.OpenPictureWebsiteCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnOpenPictureWebsite(CurrentPicture);
            });

            this.SavePictureChangesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnSaveChanges(CurrentPicture);
            });

            this.ApplyMetadataGroupCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                OnApplyMetadataGroup(id, CurrentPicture);
            });

            #region Online List This Image

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

            #endregion Online List This Image

            #region Workspace This Image

            this.AddToDefaultWorkspaceCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnAddToDefaultWorkspace(CurrentPicture);
            });

            this.AddToSpecificWorkspaceCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                OnAddToSpecificWorkspace(id, CurrentPicture);
            });

            this.AddToEligibleWorkspacesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnAddToEligibleWorkspaces(CurrentPicture);
            });

            this.AddToAllWorkspacesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnAddToAllWorkspaces(CurrentPicture);
            });

            this.RemoveFromSpecificWorkspaceCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                OnRemoveFromSpecificWorkspace(id, CurrentPicture);
            });

            this.RemoveFromAllWorkspaceCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnRemoveFromAllWorkspaces(CurrentPicture);
            });

            #endregion Workspace This Image

            #region Online List Selection

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

            #endregion Online List Selection

            #region Workspace Selection

            this.SelectionAddToDefaultWorkspaceCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                foreach (var picture in SelectedPictures.ToList())
                {
                    await OnAddToDefaultWorkspace(picture, true);

                    if (picture.SourceImage != null)
                    {
                        picture.SourceImage.Dispose();
                        picture.SourceImage = null;
                    }
                }

                GC.Collect();
            });

            this.SelectionAddToSpecificWorkspaceCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                foreach (var picture in SelectedPictures.ToList())
                {
                    await OnAddToSpecificWorkspace(id, picture, true);

                    if (picture.SourceImage != null)
                    {
                        picture.SourceImage.Dispose();
                        picture.SourceImage = null;
                    }
                }

                GC.Collect();
            });

            this.SelectionAddToEligibleWorkspacesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                foreach (var picture in SelectedPictures.ToList())
                {
                    await OnAddToEligibleWorkspaces(picture, true);

                    if (picture.SourceImage != null)
                    {
                        picture.SourceImage.Dispose();
                        picture.SourceImage = null;
                    }
                }

                GC.Collect();
            });

            this.SelectionAddToAllWorkspacesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                foreach (var picture in SelectedPictures.ToList())
                {
                    await OnAddToAllWorkspaces(picture, true);

                    if (picture.SourceImage != null)
                    {
                        picture.SourceImage.Dispose();
                        picture.SourceImage = null;
                    }
                }

                GC.Collect();
            });

            this.SelectionRemoveFromSpecificWorkspaceCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                foreach (var picture in SelectedPictures.ToList())
                {
                    await OnRemoveFromSpecificWorkspace(id, picture, true);
                }
            });

            this.SelectionRemoveFromAllWorkspaceCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                foreach (var picture in SelectedPictures.ToList())
                {
                    await OnRemoveFromAllWorkspaces(picture, true);
                }
            });

            #endregion Workspace Selection

            #region Selection Selection
            this.SelectionApplyMetadataGroupCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                await OnSelectionApplyMetadataGroup(id);
                GC.Collect();
            });

            this.SelectionSavePictureChangesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await OnSelectionSaveChanges();
                GC.Collect();
            });

            #endregion SelectionSelection
        }

        ~MetadataOverviewViewModel()
        {
            this.pictureControl.PictureSelected -= OnPictureSelected;
            this.pictureControl.PictureDeselected -= OnPictureDeselected;
            this.navigationControl.SwitchedToAllOnlineListBrowsing -= OnSwitchedToBrowsingContext;
            this.navigationControl.SwitchedToOnlineBrowsing -= OnSwitchedToBrowsingContext;
            this.navigationControl.SwitchedToAllWorkspaceBrowsing -= OnSwitchedToBrowsingContext;
            this.navigationControl.SwitchedToSpecificOnlineListBrowsing -= OnSwitchedToBrowsingContext;
            this.navigationControl.SwitchedToSpecificWorkspaceBrowsing -= OnSwitchedToBrowsingContext;
            this.dbRepository.OnlineList.OnlineListsChanged -= OnResourceListChanged;
            this.dbRepository.Workspace.WorkspacesChanged -= OnResourceListChanged;
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

        private async void OnResourceListChanged(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                this.ThisImageMenus = GetThisImageMenus();
                this.SelectionMenus = GetSelectionMenus();
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
                for (var i = SelectedPictures.Count - 1; i >= 0; i--)
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

        private async void OnSwitchedToBrowsingContext(object? sender, Guid e)
        {
            if (this.dbRepository.ApplicationSettings.Get()?.DeselectPicturesOnContextSwitch == true)
            {
                OnDeselectAll();
            }
        }

        private async void OnSwitchedToBrowsingContext(object? sender, EventArgs e)
        {
            if (this.dbRepository.ApplicationSettings.Get()?.DeselectPicturesOnContextSwitch == true)
            {
                OnDeselectAll();
            }
        }

        private async Task OnSelectionApplyMetadataGroup(Guid id)
        {
            if (SelectedPictures.All(p => !p.IsLocal))
            {
                await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ToastNotLocal, "metadatagroup"));
            }
            else
            {
                foreach (var picture in SelectedPictures.Where(p => p.IsLocal).ToList())
                {
                    await this.OnApplyMetadataGroup(id, picture, false);
                }

                await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ToastMetadataGroupApplied, "metadatagroup"));
            }
        }

        private async Task OnSelectionSaveChanges()
        {
            if (SelectedPictures.All(p => !p.IsLocal))
            {
                await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ToastNotLocal, "metadatagroup"));
            }
            else
            {
                foreach (var picture in SelectedPictures.Where(p => p.IsLocal).ToList())
                {
                    await this.OnSaveChanges(picture);
                }
            }
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
                var picture = SelectedPictures.FirstOrDefault(p => p.Equals(e));
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
            var metadataGroups = this.dbRepository.MetadataGroup.GetAll();
            var defaultMetadataGroup = metadataGroups.FirstOrDefault(m => m.IsDefault);

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

            var workspaceMenus = new MenuItemViewModel();
            var allWorkspaces = this.dbRepository.Workspace.GetAll();
            var defaultWorkspace = allWorkspaces.FirstOrDefault(w => w.IsDefault);

            workspaceMenus.Header = Language.ActionWorkspaces;
            workspaceMenus.IsEnabled = allWorkspaces.Count > 0;
            workspaceMenus.Items = new List<MenuItemViewModel>() {
                {
                    new MenuItemViewModel()
                    {
                        Header = $"{Language.ActionAddToDefault} {(defaultWorkspace != null ? "(" + defaultWorkspace.Name + ")" : "")}",
                        Command = AddToDefaultWorkspaceCommand,
                        IsEnabled = defaultWorkspace != null
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionAddToEligible,
                        Command = AddToEligibleWorkspacesCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionAddTo,
                        Items = new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = AddToAllWorkspacesCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(allWorkspaces.Select(l => new MenuItemViewModel() { Header = l.Name, Command = AddToSpecificWorkspaceCommand, CommandParameter = l.Id })))
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
                        Command = RemoveFromAllWorkspaceCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionRemoveFrom,
                        Items = new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = RemoveFromAllWorkspaceCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(allWorkspaces.Select(l => new MenuItemViewModel() { Header = l.Name, Command = RemoveFromSpecificWorkspaceCommand,  CommandParameter = l.Id })))
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
                        Header = CurrentPicture.IsLocal ? string.Format(Language.ActionSaveChanges, CurrentPicture.LocalProviderType?.ToLower()) : Language.ActionSaveChangesGeneral,
                        Command = SavePictureChangesCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionApplyMetadataGroup,
                        Items = metadataGroups.Count > 0 ? new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = defaultMetadataGroup == null ? Language.Default : string.Format($"{Language.Default} ({defaultMetadataGroup.Name})"), Command = ApplyMetadataGroupCommand, CommandParameter = defaultMetadataGroup.Id, IsEnabled = defaultMetadataGroup != null } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(metadataGroups.Select(l => new MenuItemViewModel() { Header = l.Name, Command = ApplyMetadataGroupCommand, CommandParameter = l.Id }))
                        ) : null,
                        IsEnabled = metadataGroups.Count > 0
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
                        Header = Language.OpenPicture,
                        Command = OpenPictureCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionOpenInDefaultApplication,
                        Command = OpenInDefaultApplicationCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionOpenPictureWebsite,
                        Command = OpenPictureWebsiteCommand
                    }
                }
            };

            menus.Items = new List<MenuItemViewModel>()
            {
                {
                    onlineListMenus
                },
                {
                    workspaceMenus
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
            var allWorkspaces = this.dbRepository.Workspace.GetAll();
            var metadataGroups = this.dbRepository.MetadataGroup.GetAll();
            var defaultMetadataGroup = metadataGroups.FirstOrDefault(m => m.IsDefault);

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

            var workspaceMenus = new MenuItemViewModel();
            workspaceMenus.Header = Language.ActionWorkspaces;
            workspaceMenus.Items = new List<MenuItemViewModel>() {
                {
                    new MenuItemViewModel()
                    {
                        Header = $"{Language.ActionAddToDefault} ({allWorkspaces.FirstOrDefault(s => s.IsDefault)?.Name})",
                        Command = SelectionAddToDefaultWorkspaceCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionAddToEligible,
                        Command = SelectionAddToEligibleWorkspacesCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionAddTo,
                        Items = new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = SelectionAddToAllWorkspacesCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(allWorkspaces.Select(l => new MenuItemViewModel() { Header = l.Name, Command = SelectionAddToSpecificWorkspaceCommand, CommandParameter = l.Id })))
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
                        Command = SelectionRemoveFromAllWorkspaceCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionRemoveFrom,
                        Items = new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = SelectionRemoveFromAllWorkspaceCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(allWorkspaces.Select(l => new MenuItemViewModel() { Header = l.Name, Command = SelectionRemoveFromSpecificWorkspaceCommand,  CommandParameter = l.Id }))),
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
                        Header = Language.ActionSaveChangesGeneral,
                        Command = SelectionSavePictureChangesCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionApplyMetadataGroup,
                        Items = metadataGroups.Count > 0 ? new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = defaultMetadataGroup == null ? Language.Default : string.Format($"{Language.Default} ({defaultMetadataGroup.Name})"), Command = SelectionApplyMetadataGroupCommand, CommandParameter = defaultMetadataGroup.Id, IsEnabled = defaultMetadataGroup != null } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(metadataGroups.Select(l => new MenuItemViewModel() { Header = l.Name, Command = SelectionApplyMetadataGroupCommand, CommandParameter = l.Id }))
                        ) : null,
                        IsEnabled = metadataGroups.Count > 0
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
                    workspaceMenus
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
