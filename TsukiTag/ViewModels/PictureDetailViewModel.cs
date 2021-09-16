using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;
using System.Reactive.Concurrency;
using System.Reactive;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using TsukiTag.Converters;
using Avalonia.Controls;

namespace TsukiTag.ViewModels
{
    public class PictureDetailViewModel : ViewModelCollectionHandlerBase
    {
        private Picture picture;
        private readonly IPictureControl pictureControl;

        private ObservableCollection<MenuItemViewModel> onlineListMenus;
        private bool maximizedView;
        private bool fillView;
        private Bitmap image;

        public Picture Picture
        {
            get { return picture; }
            set { picture = value; }
        }

        public Bitmap Image
        {
            get { return image; }
            set { image = value; }
        }

        public bool MaximizedView
        {
            get { return maximizedView; }
            set
            {
                maximizedView = value;
                fillView = !value;

                this.RaisePropertyChanged(nameof(MaximizedView));
                this.RaisePropertyChanged(nameof(FillView));
            }
        }

        public bool FillView
        {
            get { return fillView; }
            set
            {
                fillView = value;
                maximizedView = !value;

                this.RaisePropertyChanged(nameof(MaximizedView));
                this.RaisePropertyChanged(nameof(FillView));
            }
        }

        public ObservableCollection<MenuItemViewModel> Menus
        {
            get { return onlineListMenus; }
            set
            {
                onlineListMenus = value;
                this.RaisePropertyChanged(nameof(Menus));
            }
        }

        public ReactiveCommand<Unit, Unit> ClosePictureCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SwitchDisplayModeCommand { get; set; }

        public PictureDetailViewModel(
            Picture picture,
            Bitmap image,
            IPictureControl pictureControl,
            IDbRepository dbRepository,
            INotificationControl notificationControl,
            IPictureWorker pictureWorker,
            IPictureProviderContext providerContext
        ) : base(dbRepository, pictureWorker, notificationControl, providerContext)
        {
            this.ClosePictureCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnClosePicture();
            });

            this.SwitchDisplayModeCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnSwitchDisplay();
            });

            this.OpenInDefaultApplicationCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnOpenInDefaultApplication(Picture);
            });

            this.AddToDefaultListCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnAddToDefaulList(Picture);
            });

            this.AddToSpecificListCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                OnAddToSpecificList(id, Picture);
            });

            this.AddToEligibleListsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnAddToEligibleLists(Picture);
            });

            this.AddToAllListsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnAddToAllLists(Picture);
            });

            this.RemoveFromSpecificListCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                OnRemoveFromSpecificList(id, Picture);
            });

            this.RemoveFromAllListCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnRemoveFromAllLists(Picture);
            });

            this.AddToDefaultWorkspaceCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnAddToDefaultWorkspace(Picture, Image);
            });

            this.AddToSpecificWorkspaceCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                OnAddToSpecificWorkspace(id, Picture, Image);
            });

            this.AddToEligibleWorkspacesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnAddToEligibleWorkspaces(Picture, Image);
            });

            this.AddToAllWorkspacesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnAddToAllWorkspaces(Picture, Image);
            });

            this.RemoveFromSpecificWorkspaceCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                OnRemoveFromSpecificWorkspace(id, Picture);
            });

            this.RemoveFromAllWorkspaceCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnRemoveFromAllWorkspaces(Picture);
            });

            this.ApplyMetadataGroupCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                OnApplyMetadataGroup(id, Picture);
            });

            this.SavePictureChangesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnSaveChanges(Picture);
            });

            this.OpenPictureWebsiteCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnOpenPictureWebsite(Picture);
            });

            this.RedownloadPictureCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnRedownloadPicture(Picture);
            });

            this.CopyWebsiteUrlToClipboardCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnCopyPictureWebsiteUrlToClipboard(Picture);
            });

            this.SaveImageAsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnSavePictureAs(false);
            });

            this.SaveOriginalImageAsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnSavePictureAs(true);
            });

            this.fillView = true;

            this.pictureControl = pictureControl;
            this.picture = picture;
            this.image = image;
            this.onlineListMenus = GetMenus();

            this.dbRepository.OnlineList.OnlineListsChanged += OnResourceListsChanged;
            this.dbRepository.Workspace.WorkspacesChanged += OnResourceListsChanged;

            this.picture.PropertyChanged += OnPicturePropertiesChanged;
        }

        ~PictureDetailViewModel()
        {
            this.dbRepository.OnlineList.OnlineListsChanged -= OnResourceListsChanged;
            this.dbRepository.Workspace.WorkspacesChanged -= OnResourceListsChanged;
            this.picture.PropertyChanged -= OnPicturePropertiesChanged;

            this.Image?.Dispose();
            this.Image = null;
        }

        public async void OnSwitchDisplay()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                MaximizedView = !maximizedView;
            });
        }

        public async void OnInternalClose()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                this.dbRepository.OnlineList.OnlineListsChanged -= OnResourceListsChanged;
                this.dbRepository.Workspace.WorkspacesChanged -= OnResourceListsChanged;
                this.picture.PropertyChanged -= OnPicturePropertiesChanged;
            });
        }

        public async void OnClosePicture()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if (Picture != null)
                {
                    this.pictureControl.ClosePicture(picture);
                    OnInternalClose();
                }
            });
        }

        private async void OnPicturePropertiesChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnResourceListsChanged(sender, e);
        }

        private async void OnResourceListsChanged(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                this.Menus = GetMenus();
            });
        }

        private async void OnSavePictureAs(bool sourceImage)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if (Picture != null)
                {
                    var dialog = new SaveFileDialog() { DefaultExtension = picture.Extension, InitialFileName = $"{picture.Md5}.{picture.Extension}" };
                    var filePath = await dialog.ShowAsync(App.MainWindow);

                    if(!string.IsNullOrEmpty(filePath))
                    {
                        await OnSaveImageAs(Picture, filePath, sourceImage, Image, true);
                    }
                }
            });
        }

        private ObservableCollection<MenuItemViewModel> GetMenus()
        {
            var metadataGroups = this.dbRepository.MetadataGroup.GetAll();
            var defaultMetadataGroup = metadataGroups.FirstOrDefault(m => m.IsDefault);

            var imageMenus = new MenuItemViewModel();
            imageMenus.Header = Language.ActionImage;
            imageMenus.Items = new List<MenuItemViewModel>() {
                {
                    new MenuItemViewModel()
                    {
                        Header = Picture.IsLocal ? string.Format(Language.ActionSaveChanges, Picture.LocalProviderType?.ToLower()) : Language.ActionSaveChangesGeneral,
                        IsEnabled = Picture.IsLocal,
                        Command = SavePictureChangesCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionSaveImageAs,
                        Command = SaveImageAsCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionSaveOriginalImageAs,
                        Command = SaveOriginalImageAsCommand
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
                        Header = Language.ActionApplyMetadataGroup,
                        Items = metadataGroups.Count > 0 ? new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = defaultMetadataGroup == null ? Language.Default : string.Format($"{Language.Default} ({defaultMetadataGroup.Name})"), Command = ApplyMetadataGroupCommand, CommandParameter = defaultMetadataGroup.Id, IsEnabled = defaultMetadataGroup != null } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(metadataGroups.Select(l => new MenuItemViewModel() { Header = l.Name, Command = ApplyMetadataGroupCommand, CommandParameter = l.Id }))
                        ) : null,
                        IsEnabled = metadataGroups.Count > 0 && Picture.IsLocal
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
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionCopyWebsiteUrlToClipboard,
                        Command = CopyWebsiteUrlToClipboardCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionRedownload,
                        Command = RedownloadPictureCommand
                    }
                }
            };

            var onlineListMenus = new MenuItemViewModel();
            var allLists = this.dbRepository.OnlineList.GetAll();
            var eligibleLists = allLists.Where(l => l.IsEligible(Picture)).ToList();
            var containingLists = this.dbRepository.OnlineListPicture.GetAllForPicture(picture.Md5).Select(s => allLists.FirstOrDefault(l => l.Id == s.ResourceListId)).Where(s => s != null).ToList();

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
                        Items = eligibleLists.Count > 0 ? new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = AddToEligibleListsCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(eligibleLists.Select(l => new MenuItemViewModel() { Header = l.Name, Command = AddToSpecificListCommand, CommandParameter = l.Id }))
                        ) : null,
                        IsEnabled = eligibleLists.Count > 0
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
                        IsEnabled = containingLists.Count > 0,
                        Command = RemoveFromAllListCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionRemoveFrom,
                        Items = new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = RemoveFromAllListCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(containingLists.Select(l => new MenuItemViewModel() { Header = l.Name, Command = RemoveFromSpecificListCommand,  CommandParameter = l.Id }))),
                        IsEnabled = containingLists.Count > 0
                    }
                }
            };

            var workspaceMenus = new MenuItemViewModel();
            var allWorkspaces = this.dbRepository.Workspace.GetAll();
            var defaultWorkspace = allWorkspaces.FirstOrDefault(w => w.IsDefault);
            var eligibleWorkspaces = allWorkspaces.Where(l => l.IsEligible(Picture)).ToList();
            var containingWorkspaces = this.dbRepository.WorkspacePicture.GetAllForPicture(picture.Md5).Select(s => allWorkspaces.FirstOrDefault(l => l.Id == s.ResourceListId)).Where(s => s != null).ToList();

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
                        Items = eligibleWorkspaces.Count > 0 ? new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = AddToEligibleWorkspacesCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(eligibleWorkspaces.Select(l => new MenuItemViewModel() { Header = l.Name, Command = AddToSpecificWorkspaceCommand, CommandParameter = l.Id }))
                        ) : null,
                        IsEnabled = eligibleWorkspaces.Count > 0
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
                        IsEnabled = containingWorkspaces.Count > 0,
                        Command = RemoveFromAllWorkspaceCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionRemoveFrom,
                        Items = new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = RemoveFromAllWorkspaceCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(containingWorkspaces.Select(l => new MenuItemViewModel() { Header = l.Name, Command = RemoveFromSpecificWorkspaceCommand,  CommandParameter = l.Id }))),
                        IsEnabled = containingWorkspaces.Count > 0
                    }
                }
            };

            return new ObservableCollection<MenuItemViewModel>() { imageMenus, onlineListMenus, workspaceMenus };
        }

        public override void Reinitialize()
        {
            base.Reinitialize();

            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                Menus = GetMenus();
            });
        }
    }
}
