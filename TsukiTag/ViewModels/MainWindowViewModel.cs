using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;
using TsukiTag.Models.Repository;
using TsukiTag.Views;

namespace TsukiTag.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly INavigationControl navigationControl;
        private readonly INotificationControl notificationControl;
        private readonly IPictureWorker pictureWorker;
        private readonly IDbRepository dbRepository;

        private ProviderContext providerContext;
        private ContentControl currentContent;
        private ObservableCollection<MenuItemViewModel> mainWindowMenus;

        public ReactiveCommand<Unit, Unit> SwitchToSettingsCommand { get; }

        public ReactiveCommand<Unit, Unit> SwitchToOnlineBrowsingCommand { get; }

        public ReactiveCommand<Unit, Unit> SwitchToAllOnlineListBrowsingCommand { get; }

        public ReactiveCommand<Guid, Unit> SwitchToSpefificOnlineListBrowsingCommand { get; }

        public ReactiveCommand<Unit, Unit> SwitchToAllWorkspaceBrowsingCommand { get; }

        public ReactiveCommand<Guid, Unit> SwitchToSpefificWorkspaceBrowsingCommand { get; }

        public ReactiveCommand<Guid, Unit> ImportFilesToSpecifcWorkspacesCommand { get; }

        public ReactiveCommand<Unit, Unit> ImportFilesToAllWorkspacesCommand { get; }

        public ReactiveCommand<Guid, Unit> ImportFolderToSpecifcWorkspacesCommand { get; }

        public ReactiveCommand<Unit, Unit> ImportFolderToAllWorkspacesCommand { get; }

        public ReactiveCommand<Unit, Unit> ConvertFolderToLocalWorkspaceCommand { get; }

        public ObservableCollection<MenuItemViewModel> MainWindowMenus
        {
            get { return mainWindowMenus; }
            set
            {
                mainWindowMenus = value;
                this.RaisePropertyChanged(nameof(MainWindowMenus));
            }
        }

        public ContentControl CurrentContent
        {
            get { return currentContent; }
            set
            {
                this.RaiseAndSetIfChanged(ref currentContent, value);                ;
                this.RaisePropertyChanged(nameof(AllowImporting));
                this.OnResourceListsChanged(this, EventArgs.Empty);
            }
        }

        public bool AllowImporting
        {
            get { return !(CurrentContent is Settings); }
        }

        //Avalonia exception happens if we try to re-instate the entire provider
        //content. So for now to circumvent this, we just store the content in memory.
        //At least the benefit of having the browsing session not interrupted by visiting
        //the settings screen is there.
        public ProviderContext ProviderContext
        {
            get
            {
                if (providerContext == null)
                {
                    providerContext = new ProviderContext();
                }

                return providerContext;
            }
        }

        public MainWindowViewModel(
            INavigationControl navigationControl,
            IDbRepository dbRepository,
            IPictureWorker pictureWorker,
            INotificationControl notificationControl
        )
        {
            this.dbRepository = dbRepository;
            this.pictureWorker = pictureWorker;
            this.navigationControl = navigationControl;
            this.notificationControl = notificationControl;
            this.navigationControl.SwitchedToOnlineBrowsing += OnSwitchedToOnlineBrowsing;
            this.navigationControl.SwitchedToSettings += OnSwitchedToSettings;
            this.navigationControl.SwitchedToAllOnlineListBrowsing += OnSwitchedToAllOnlineListBrowsing;
            this.navigationControl.SwitchedToSpecificOnlineListBrowsing += OnSwitchedToSpecificOnlineListBrowsing;
            this.navigationControl.SwitchedToAllWorkspaceBrowsing += OnSwitchedToAllWorkspaceBrowsing;
            this.navigationControl.SwitchedToSpecificWorkspaceBrowsing += OnSwitchedToSpecificWorkspaceBrowsing;

            this.dbRepository.OnlineList.OnlineListsChanged += OnResourceListsChanged;
            this.dbRepository.Workspace.WorkspacesChanged += OnResourceListsChanged;

            this.SwitchToSettingsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.navigationControl.SwitchToSettings();
            });

            this.SwitchToOnlineBrowsingCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.navigationControl.SwitchToOnlineBrowsing();
            });

            this.SwitchToAllOnlineListBrowsingCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.navigationControl.SwitchToAllOnlineListBrowsing();
            });

            this.SwitchToSpefificOnlineListBrowsingCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                await this.navigationControl.SwitchToSpecificOnlineListBrowsing(id);
            });

            this.SwitchToAllWorkspaceBrowsingCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.navigationControl.SwitchToAllWorkspaceBrowsing();
            });

            this.SwitchToSpefificWorkspaceBrowsingCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                await this.navigationControl.SwitchToSpecificWorkspaceBrowsing(id);
            });

            this.ImportFilesToSpecifcWorkspacesCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                await this.OnImportFilesToSpecificWorkspace(id);
            });

            this.ImportFilesToAllWorkspacesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.OnImportFilesToAllWorkspaces();
            });

            this.ImportFolderToSpecifcWorkspacesCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                await this.OnImportFolderToSpecificWorkspace(id);
            });

            this.ImportFolderToAllWorkspacesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.OnImportFolderToAllWorkspaces();
            });

            this.ConvertFolderToLocalWorkspaceCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.OnConvertFolderToWorkspace();
            });

            CurrentContent = ProviderContext;
            MainWindowMenus = GetMainMenus();
        }

        ~MainWindowViewModel()
        {
            this.navigationControl.SwitchedToOnlineBrowsing -= OnSwitchedToOnlineBrowsing;
            this.navigationControl.SwitchedToSettings -= OnSwitchedToSettings;
            this.navigationControl.SwitchedToAllOnlineListBrowsing -= OnSwitchedToAllOnlineListBrowsing;
            this.navigationControl.SwitchedToSpecificOnlineListBrowsing -= OnSwitchedToSpecificOnlineListBrowsing;
            this.navigationControl.SwitchedToAllWorkspaceBrowsing -= OnSwitchedToAllWorkspaceBrowsing;
            this.navigationControl.SwitchedToSpecificWorkspaceBrowsing -= OnSwitchedToSpecificWorkspaceBrowsing;

            this.dbRepository.OnlineList.OnlineListsChanged -= OnResourceListsChanged;
            this.dbRepository.Workspace.WorkspacesChanged -= OnResourceListsChanged;
        }

        public async void Initialized()
        {
            this.navigationControl.SwitchToOnlineBrowsing();
        }

        public async void Closing()
        {
            this.dbRepository.ThumbnailStorage.CloseConnection();
        }

        private async void OnResourceListsChanged(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                MainWindowMenus = GetMainMenus();
            });
        }

        private void OnSwitchedToSettings(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                CurrentContent = new Settings();
            });
        }

        private void OnSwitchedToOnlineBrowsing(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                CurrentContent = ProviderContext;
            });
        }

        private void OnSwitchedToAllOnlineListBrowsing(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                CurrentContent = ProviderContext;
            });
        }

        private void OnSwitchedToSpecificOnlineListBrowsing(object? sender, Guid e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                CurrentContent = ProviderContext;
            });
        }

        private void OnSwitchedToAllWorkspaceBrowsing(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                CurrentContent = ProviderContext;
            });
        }

        private void OnSwitchedToSpecificWorkspaceBrowsing(object? sender, Guid e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                CurrentContent = ProviderContext;
            });
        }

        private async Task<bool> OnConvertFolderToWorkspace()
        {
            return await Task.Run<bool>(async () =>
            {
                var dialog = new OpenFolderDialog();
                var folder = await dialog.ShowAsync(App.MainWindow);

                if (!string.IsNullOrEmpty(folder))
                {
                    var allWorkspaces = this.dbRepository.Workspace.GetAll();
                    if (allWorkspaces.Any(w => w.FolderPath == folder))
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.SettingsWorkspaceSamePath), "workspace"));
                        return false;
                    }
                    else
                    {
                        var newWorkspace = new Workspace()
                        {
                            Id = Guid.NewGuid(),
                            FolderPath = folder,
                            FileNameTemplate = $"#md5#_#provider#.#extension#",
                            Name = new DirectoryInfo(folder).Name
                        };

                        this.dbRepository.Workspace.AddOrUpdate(newWorkspace);
                        var imageFiles = Directory.GetFiles(folder, "*.jpg")
                                        .Concat(Directory.GetFiles(folder, "*.jpeg"))
                                        .Concat(Directory.GetFiles(folder, "*.png")).ToArray();

                        if (imageFiles != null && imageFiles.Length > 0)
                        {
                            foreach (var image in imageFiles)
                            {
                                await this.notificationControl.SendToastMessage(ToastMessage.Uncloseable(string.Format(Language.ToastWorkspaceProcessingSingle, image, newWorkspace.Name), "workspace"));

                                var picture = await this.pictureWorker.CreatePictureMetadataFromLocalImage(image);
                                if (picture != null)
                                {
                                    this.dbRepository.WorkspacePicture.AddToWorkspace(newWorkspace.Id, picture.Picture, image);
                                }
                            }
                        }

                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ToastWorkspaceProcessed), "workspace"));
                    }
                }

                GC.Collect();
                return true;
            });
        }

        private async Task<bool> OnImportFolderToAllWorkspaces()
        {
            return await Task.Run<bool>(async () =>
            {
                var dialog = new OpenFolderDialog();
                var folder = await dialog.ShowAsync(App.MainWindow);

                if (!string.IsNullOrEmpty(folder))
                {
                    var imageFiles = Directory.GetFiles(folder, "*.jpg")
                                    .Concat(Directory.GetFiles(folder, "*.jpeg"))
                                    .Concat(Directory.GetFiles(folder, "*.png")).ToArray();

                    if (imageFiles != null && imageFiles.Length > 0)
                    {
                        await this.OnImportFilesToAllWorkspaces(imageFiles);
                    }
                }

                GC.Collect();
                return true;
            });
        }

        private async Task<bool> OnImportFolderToSpecificWorkspace(Guid id)
        {
            return await Task.Run<bool>(async () =>
            {
                var dialog = new OpenFolderDialog();
                var folder = await dialog.ShowAsync(App.MainWindow);

                if (!string.IsNullOrEmpty(folder))
                {
                    var imageFiles = Directory.GetFiles(folder, "*.jpg")
                                    .Concat(Directory.GetFiles(folder, "*.jpeg"))
                                    .Concat(Directory.GetFiles(folder, "*.png")).ToArray();

                    if (imageFiles != null && imageFiles.Length > 0)
                    {
                        await this.OnImportFilesToSpecificWorkspace(id, imageFiles);
                    }
                }

                GC.Collect();
                return true;
            });
        }

        private async Task<bool> OnImportFilesToSpecificWorkspace(Guid id, string[] files = null)
        {
            return await Task.Run<bool>(async () =>
            {
                var workspace = this.dbRepository.Workspace.Get(id);

                if (files == null)
                {
                    var dialog = new OpenFileDialog();
                    dialog.AllowMultiple = true;
                    dialog.Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = Language.ImageFiles, Extensions = { "jpg", "jpeg", "png" } } };

                    files = await dialog.ShowAsync(App.MainWindow);
                }

                if (files != null && files.Length > 0)
                {
                    foreach (var filePath in files)
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Uncloseable(string.Format(Language.ToastWorkspaceProcessingSingle, filePath, workspace.Name), "workspace"));
                        var picture = await this.pictureWorker.CreatePictureMetadataFromLocalImage(filePath);

                        if (picture != null)
                        {
                            var result = await this.pictureWorker.SaveWorkspacePicture(picture.Picture, workspace, picture.Image);
                            if (!string.IsNullOrEmpty(result))
                            {
                                await Task.Run(() => this.dbRepository.WorkspacePicture.AddToWorkspace(id, picture.Picture, result));
                            }
                            else
                            {
                                return false;
                            }                        
                        }
                    }

                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ToastWorkspaceProcessed), "workspace"));
                    return true;
                }

                return true;
            });
        }

        private async Task<bool> OnImportFilesToAllWorkspaces(string[] files = null)
        {
            return await Task.Run<bool>(async () =>
            {
                if (files == null)
                {
                    var dialog = new OpenFileDialog();
                    dialog.AllowMultiple = true;
                    dialog.Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = Language.ImageFiles, Extensions = { "jpg", "jpeg", "png" } } };

                    files = await dialog.ShowAsync(App.MainWindow);
                }

                if (files != null && files.Length > 0)
                {
                    foreach (var workspace in this.dbRepository.Workspace.GetAll())
                    {
                        var result = await OnImportFilesToSpecificWorkspace(workspace.Id, files);
                        if (!result)
                        {
                            await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionGenericError), "workspace"));
                            return false;
                        }
                    }

                    return true;
                }

                GC.Collect();
                return true;
            });
        }

        private ObservableCollection<MenuItemViewModel> GetMainMenus()
        {
            var menus = new MenuItemViewModel();
            var allLists = this.dbRepository.OnlineList.GetAll();
            var allWorkspaces = this.dbRepository.Workspace.GetAll();

            menus.Header = "Tsuki-tag";
            menus.Items = new List<MenuItemViewModel>()
            {
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.NavigationOnline,
                        Command = SwitchToOnlineBrowsingCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.NavigationAllOnlineLists,
                        Command = SwitchToAllOnlineListBrowsingCommand
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.NavigationSpecificOnlineList,
                        Items = new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = SwitchToAllOnlineListBrowsingCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(allLists.Select(l => new MenuItemViewModel() { Header = l.Name, Command = SwitchToSpefificOnlineListBrowsingCommand, CommandParameter = l.Id })))
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.NavigationAllWorkspaces,
                        Command = SwitchToAllWorkspaceBrowsingCommand,
                        IsEnabled = allWorkspaces.Count > 0
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.NavigationSpecificWorkspace,
                        IsEnabled = allWorkspaces.Count > 0,
                        Items = new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = SwitchToAllWorkspaceBrowsingCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(allWorkspaces.Select(l => new MenuItemViewModel() { Header = l.Name, Command = SwitchToSpefificWorkspaceBrowsingCommand, CommandParameter = l.Id })))
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
                        Header = Language.ActionImportImagesToWorkspaces,
                        IsEnabled = allWorkspaces.Count > 0 && AllowImporting,
                        Items = new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = ImportFilesToAllWorkspacesCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(allWorkspaces.Select(l => new MenuItemViewModel() { Header = l.Name, Command = ImportFilesToSpecifcWorkspacesCommand, CommandParameter = l.Id })))
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionImportFolderToWorkspace,
                        IsEnabled = allWorkspaces.Count > 0 && AllowImporting,
                        Items = new List<MenuItemViewModel>(
                            new List<MenuItemViewModel>() { { new MenuItemViewModel() { Header = Language.All, Command = ImportFolderToAllWorkspacesCommand } }, { new MenuItemViewModel() { Header = "-" } } }
                            .Concat(allWorkspaces.Select(l => new MenuItemViewModel() { Header = l.Name, Command = ImportFolderToSpecifcWorkspacesCommand, CommandParameter = l.Id })))
                    }
                },
                {
                    new MenuItemViewModel()
                    {
                        Header = Language.ActionCreateWorkspaceFromFolder,
                        Command = ConvertFolderToLocalWorkspaceCommand,
                        IsEnabled = AllowImporting
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
                        Header = Language.NavigationSettings,
                        Command = SwitchToSettingsCommand
                    }
                },
            };

            return new ObservableCollection<MenuItemViewModel>() { menus };
        }
    }
}

