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
using TsukiTag.Models.Repository;

namespace TsukiTag.ViewModels
{
    public class OnlineNavigationBarViewModel : ViewModelBaseBrowserNavigationHandler
    {
        private bool safebooruEnabled;
        private bool gelbooruEnabled;
        private bool konachanEnabled;
        private bool danbooruEnabled;
        private bool yandereEnabled;
        private bool r34Enabled;

        private ObservableCollection<PreviousSession> previousSessions;
        private ObservableCollection<MenuItemViewModel> previousSessionMenuItems;

        public bool SafebooruEnabled
        {
            get { return safebooruEnabled; }
            set
            {
                safebooruEnabled = value;
                this.RaisePropertyChanged(nameof(SafebooruEnabled));
            }
        }

        public bool GelbooruEnabled
        {
            get { return gelbooruEnabled; }
            set
            {
                gelbooruEnabled = value;
                this.RaisePropertyChanged(nameof(GelbooruEnabled));
            }
        }

        public bool KonachanEnabled
        {
            get { return konachanEnabled; }
            set
            {
                konachanEnabled = value;
                this.RaisePropertyChanged(nameof(KonachanEnabled));
            }
        }

        public bool DanbooruEnabled
        {
            get { return danbooruEnabled; }
            set
            {
                danbooruEnabled = value;
                this.RaisePropertyChanged(nameof(DanbooruEnabled));
            }
        }

        public bool YandereEnabled
        {
            get { return yandereEnabled; }
            set
            {
                yandereEnabled = value;
                this.RaisePropertyChanged(nameof(YandereEnabled));
            }
        }
        
        public bool R34Enabled
        {
            get { return r34Enabled; }
            set
            {
                r34Enabled = value;
                this.RaisePropertyChanged(nameof(R34Enabled));
            }
        }

        public ReactiveCommand<Guid, Unit> SetFilterFromPreviousSessionCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> ClearPreviousSessionsCommand { get; protected set; }

        public ObservableCollection<PreviousSession> PreviousSessions
        {
            get { return previousSessions; }
            set
            {
                previousSessions = value;
                this.RaisePropertyChanged(nameof(PreviousSessions));
            }
        }

        public ObservableCollection<MenuItemViewModel> PreviousSessionMenuItems
        {
            get { return previousSessionMenuItems; }
            set
            {
                previousSessionMenuItems = value;
                this.RaisePropertyChanged(nameof(PreviousSessionMenuItems));
            }
        }

        public OnlineNavigationBarViewModel(
            IProviderFilterControl providerFilterControl,
            IDbRepository dbRepository
        ) : base(providerFilterControl, dbRepository)
        {
            this.SetFilterFromPreviousSessionCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                await this.SetFilterFromPreviousSession(id);
            });

            this.ClearPreviousSessionsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.ClearPreviousSessions();
            });

            this.GetPreviousSessions();
        }

        protected override async void ConfigureFilters()
        {
            base.ConfigureFilters();

            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var currentFilter = await this.providerFilterControl.GetCurrentFilter();
                if (currentFilter != null)
                {
                    SafebooruEnabled = currentFilter.Providers.Contains(Provider.Safebooru.Name);
                    GelbooruEnabled = currentFilter.Providers.Contains(Provider.Gelbooru.Name);
                    KonachanEnabled = currentFilter.Providers.Contains(Provider.Konachan.Name);
                    DanbooruEnabled = currentFilter.Providers.Contains(Provider.Danbooru.Name);
                    YandereEnabled = currentFilter.Providers.Contains(Provider.Yandere.Name);
                    R34Enabled = currentFilter.Providers.Contains(Provider.R34.Name);
                }
            });
        }

        protected override void OnFilterChanged(object? sender, EventArgs e)
        {
            base.OnFilterChanged(sender, e);
            this.ConfigurePreviousSessions();
        }

        protected override void OnPageChanged(object? sender, int e)
        {
            base.OnPageChanged(sender, e);
            this.ConfigurePreviousSessions();
        }

        protected virtual async Task ClearPreviousSessions()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                this.dbRepository.PreviousSession.Clear();
                this.ConfigurePreviousSessions();
            });
        }

        protected virtual async Task SetFilterFromPreviousSession(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var session = PreviousSessions.FirstOrDefault(s => s.Id == id);
                if (session != null)
                {
                    await this.providerFilterControl.SetFilter(session.Tags, session.ExcludedTags, session.Page);
                }
            });
        }

        protected virtual async void ConfigurePreviousSessions()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                PreviousSessions = new ObservableCollection<PreviousSession>(dbRepository.PreviousSession.GetAll());

                var currentFilter = await this.providerFilterControl.GetCurrentFilter();
                var currentSession = PreviousSessions?.FirstOrDefault();

                if (currentFilter != null && currentFilter.Session == ProviderSession.OnlineProviderSession)
                {
                    var newSession = false;
                    if (currentSession != null)
                    {
                        if ((currentFilter.Tags.All(t => currentSession.Tags.Contains(t)) &&
                           currentFilter.ExcludedTags.All(t => currentSession.ExcludedTags.Contains(t))) &&
                           (currentSession.Tags.All(t => currentFilter.Tags.Contains(t)) &&
                           currentSession.ExcludedTags.All(t => currentFilter.ExcludedTags.Contains(t))))
                        {
                            currentSession.Page = currentFilter.Page;
                            this.dbRepository.PreviousSession.AddOrUpdate(this.previousSessions.ToList());
                        }
                        else if (currentFilter.Tags.Count == 0 && currentFilter.ExcludedTags.Count == 0 &&
                            currentSession.Tags?.Count() == 0 && currentSession.ExcludedTags?.Count() == 0)
                        {
                            currentSession.Page = currentFilter.Page;
                            this.dbRepository.PreviousSession.AddOrUpdate(this.previousSessions.ToList());
                        }
                        else
                        {
                            newSession = true;
                        }
                    }
                    else
                    {
                        newSession = true;
                    }

                    if (newSession)
                    {
                        if (this.previousSessions == null)
                        {
                            this.previousSessions = new ObservableCollection<PreviousSession>();
                        }

                        this.previousSessions.Add(new PreviousSession()
                        {
                            Added = DateTime.Now,
                            Id = Guid.NewGuid(),
                            Tags = currentFilter.Tags.ToArray(),
                            ExcludedTags = currentFilter.ExcludedTags.ToArray(),
                            Page = currentFilter.Page
                        });

                        this.dbRepository.PreviousSession.AddOrUpdate(previousSessions.ToList());
                    }

                    this.GetPreviousSessions();
                }
            });
        }

        protected virtual async void GetPreviousSessions()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                PreviousSessions = new ObservableCollection<PreviousSession>(dbRepository.PreviousSession.GetAll());
                PreviousSessionMenuItems = GetPreviousSessionMenus();
            });
        }

        protected virtual ObservableCollection<MenuItemViewModel> GetPreviousSessionMenus()
        {
            var menus = new MenuItemViewModel();

            menus.Header = Language.PreviousSessions;
            menus.HasIcon = true;
            menus.Icon = "fa-undo";
            menus.Items = new List<MenuItemViewModel>();

            menus.Items.Add(new MenuItemViewModel()
            {
                Header = Language.Clear,
                Command = ClearPreviousSessionsCommand
            });

            menus.Items.Add(new MenuItemViewModel()
            {
                Header = "-"
            });

            foreach(var s in PreviousSessions.ToList())
            {
                menus.Items.Add(new MenuItemViewModel()
                {
                    Header = s.Name,
                    Command = SetFilterFromPreviousSessionCommand,
                    CommandParameter = s.Id
                });
            }
            
            return new ObservableCollection<MenuItemViewModel>() { menus };
        }

    }
}
