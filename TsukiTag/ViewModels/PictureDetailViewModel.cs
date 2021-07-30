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

namespace TsukiTag.ViewModels
{
    public class PictureDetailViewModel : ViewModelCollectionHandlerBase
    {
        private Picture picture;
        private readonly IPictureControl pictureControl;

        private ObservableCollection<MenuItemViewModel> onlineListMenus;
        private bool maximizedView;
        private bool fillView;

        public Picture Picture
        {
            get { return picture; }
            set { picture = value; }
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

        public ObservableCollection<MenuItemViewModel> OnlineListMenus
        {
            get { return onlineListMenus; }
            set
            {
                onlineListMenus = value;
                this.RaisePropertyChanged(nameof(OnlineListMenus));
            }
        }

        public ReactiveCommand<Unit, Unit> ClosePictureCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SwitchDisplayModeCommand { get; set; }

        public PictureDetailViewModel(
            Picture picture,
            IPictureControl pictureControl,
            IDbRepository dbRepository,
            INotificationControl notificationControl
        ) : base(dbRepository, notificationControl)
        {
            this.ClosePictureCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnClosePicture();
            });

            this.SwitchDisplayModeCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnSwitchDisplay();
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

            this.fillView = true;

            this.pictureControl = pictureControl;
            this.picture = picture;
            this.onlineListMenus = GetOnlineListMenus();

            this.dbRepository.OnlineList.OnlineListsChanged += OnOnlineListsChanged;
        }

        ~PictureDetailViewModel()
        {
            this.dbRepository.OnlineList.OnlineListsChanged -= OnOnlineListsChanged;
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
                this.dbRepository.OnlineList.OnlineListsChanged -= OnOnlineListsChanged;
            });
        }

        public async void OnClosePicture()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if (Picture != null)
                {
                    this.pictureControl.ClosePicture(picture);
                }
            });
        }

        private async void OnOnlineListsChanged(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                this.OnlineListMenus = GetOnlineListMenus();
            });
        }

        private ObservableCollection<MenuItemViewModel> GetOnlineListMenus()
        {
            var menus = new MenuItemViewModel();
            var allLists = this.dbRepository.OnlineList.GetAll();
            var eligibleLists = allLists.Where(l => l.IsEligible(Picture)).ToList();
            var containingLists = this.dbRepository.OnlineListPicture.GetAllForPicture(picture.Md5).Select(s => allLists.FirstOrDefault(l => l.Id == s.ListId)).Where(s => s != null).ToList();

            menus.Header = Language.ActionOnlineLists;
            menus.Items = new List<MenuItemViewModel>() {
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

            return new ObservableCollection<MenuItemViewModel>() { menus };
        }

        public override void Reinitialize()
        {
            base.Reinitialize();

            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                OnlineListMenus = GetOnlineListMenus();
            });
        }
    }
}
