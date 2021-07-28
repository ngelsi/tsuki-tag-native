using Avalonia.Controls;
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
using TsukiTag.Views;

namespace TsukiTag.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly INavigationControl navigationControl;
        private readonly IDbRepository dbRepository;

        private ProviderContext providerContext;
        private ContentControl currentContent;
        private ObservableCollection<MenuItemViewModel> mainWindowMenus;

        public ReactiveCommand<Unit, Unit> SwitchToSettingsCommand { get; }

        public ReactiveCommand<Unit, Unit> SwitchToOnlineBrowsingCommand { get; }

        public ReactiveCommand<Unit, Unit> SwitchToAllOnlineListBrowsingCommand { get; }

        public ReactiveCommand<Guid, Unit> SwitchToSpefificOnlineListBrowsingCommand { get; }

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
            set { this.RaiseAndSetIfChanged(ref currentContent, value); }
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
            IDbRepository dbRepository
        )
        {
            this.dbRepository = dbRepository;
            this.navigationControl = navigationControl;
            this.navigationControl.SwitchedToOnlineBrowsing += OnSwitchedToOnlineBrowsing;
            this.navigationControl.SwitchedToSettings += OnSwitchedToSettings;
            this.navigationControl.SwitchedToAllOnlineListBrowsing += OnSwitchedToAllOnlineListBrowsing;
            this.navigationControl.SwitchedToSpecificOnlineListBrowsing += OnSwitchedToSpecificOnlineListBrowsing;

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

            CurrentContent = ProviderContext;
            MainWindowMenus = GetMainMenus();
        }

        ~MainWindowViewModel()
        {
            this.navigationControl.SwitchedToOnlineBrowsing -= OnSwitchedToOnlineBrowsing;
            this.navigationControl.SwitchedToSettings -= OnSwitchedToSettings;
            this.navigationControl.SwitchedToAllOnlineListBrowsing -= OnSwitchedToAllOnlineListBrowsing;
            this.navigationControl.SwitchedToSpecificOnlineListBrowsing -= OnSwitchedToSpecificOnlineListBrowsing;
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


        private ObservableCollection<MenuItemViewModel> GetMainMenus()
        {
            var menus = new MenuItemViewModel();
            var allLists = this.dbRepository.OnlineList.GetAll();

            menus.Header = "TsukiTag";
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

