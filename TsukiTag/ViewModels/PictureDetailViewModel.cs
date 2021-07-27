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
    public class PictureDetailViewModel : ViewModelBase
    {
        private Picture picture;
        private readonly IPictureControl pictureControl;
        private readonly IDbRepository dbRepository;
        private readonly INotificationControl notificationControl;

        private ObservableCollection<MenuItemViewModel> onlineListMenus;
        private bool maximizedView;
        private bool fillView;

        public ReactiveCommand<Unit, Unit> AddToDefaultListCommand { get; }
        public ReactiveCommand<Guid, Unit> AddToSpecificListCommand { get; }
        public ReactiveCommand<Unit, Unit> AddToEligibleListsCommand { get; }
        public ReactiveCommand<Unit, Unit> AddToAllListsCommand { get; }

        public ReactiveCommand<Guid, Unit> RemoveFromSpecificListCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveFromAllListCommand { get; }

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
        )
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
               OnAddToDefaulList();
            });

            this.AddToSpecificListCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                OnAddToSpecificList(id);
            });

            this.AddToEligibleListsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnAddToEligibleLists();
            });

            this.AddToAllListsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnAddToAllLists();
            });

            this.RemoveFromSpecificListCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) =>
            {
                OnRemoveFromSpecificList(id);
            });

            this.RemoveFromAllListCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                OnRemoveFromAllLists();
            });

            this.fillView = true;

            this.pictureControl = pictureControl;
            this.dbRepository = dbRepository;
            this.notificationControl = notificationControl;

            this.picture = picture;
            this.onlineListMenus = GetOnlineListMenus();
        }

        ~PictureDetailViewModel()
        {

        }

        public async void OnSwitchDisplay()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                MaximizedView = !maximizedView;
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

        private ObservableCollection<MenuItemViewModel> GetOnlineListMenus()
        {
            var menus = new MenuItemViewModel();
            var allLists = this.dbRepository.OnlineList.GetAll();
            var eligibleLists = allLists.Where(l => l.IsEligible(Picture)).ToList();
            var containingLists = this.dbRepository.OnlineListPicture.GetAllForPicture(picture.Md5).Select(s => allLists.FirstOrDefault(l => l.Id == s.ListId)).ToList();

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

        private async void OnRemoveFromAllLists()
        {
            await Task.Run(async () =>
            {
                if(this.dbRepository.OnlineListPicture.RemoveFromAllLists(Picture))
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionRemoveFromAllSuccess, Language.OnlineLists.ToLower())));

                    RxApp.MainThreadScheduler.Schedule(async () =>
                    {
                        OnlineListMenus = GetOnlineListMenus();
                    });
                }                
                else
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                }
            });
        }

        private async void OnRemoveFromSpecificList(Guid id)
        {
            await Task.Run(async () =>
            {
                var list = this.dbRepository.OnlineList.Get(id);
                if(list != null)
                {
                    if (this.dbRepository.OnlineListPicture.RemoveFromList(id, Picture))
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionRemoveFromSuccess, Language.OnlineList.ToLower(), list.Name)));

                        RxApp.MainThreadScheduler.Schedule(async () =>
                        {
                            OnlineListMenus = GetOnlineListMenus();
                        });
                    }
                    else
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                    }
                }
                else
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                }
            });
        }

        private async void OnAddToAllLists()
        {
            await Task.Run(async () =>
            {
                if (this.dbRepository.OnlineListPicture.AddToAllLists(Picture))
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionAddToAllSuccess, Language.OnlineLists.ToLower())));

                    RxApp.MainThreadScheduler.Schedule(async () =>
                    {
                        OnlineListMenus = GetOnlineListMenus();
                    });
                }
                else
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                }
            });
        }

        private async void OnAddToEligibleLists()
        {
            await Task.Run(async () =>
            {
                if (this.dbRepository.OnlineListPicture.AddToAllEligible(Picture))
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionAddToEligibleSuccess, Language.OnlineLists.ToLower())));

                    RxApp.MainThreadScheduler.Schedule(async () =>
                    {
                        OnlineListMenus = GetOnlineListMenus();
                    });
                }
                else
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                }
            });
        }

        private async void OnAddToSpecificList(Guid id)
        {
            await Task.Run(async () =>
            {
                var list = this.dbRepository.OnlineList.Get(id);
                if (list != null)
                {
                    if (this.dbRepository.OnlineListPicture.AddToList(id, Picture))
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionAddToSuccess, Language.OnlineList.ToLower(), list.Name)));

                        RxApp.MainThreadScheduler.Schedule(async () =>
                        {
                            OnlineListMenus = GetOnlineListMenus();
                        });
                    }
                    else
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                    }
                }
                else
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                }
            });
        }

        private async void OnAddToDefaulList()
        {
            await Task.Run(async () =>
            {
                var list = this.dbRepository.OnlineList.GetDefault();
                if (list != null)
                {
                    if (this.dbRepository.OnlineListPicture.AddToList(list.Id, Picture))
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionAddToSuccess, Language.OnlineList.ToLower(), list.Name)));

                        RxApp.MainThreadScheduler.Schedule(async () =>
                        {
                            OnlineListMenus = GetOnlineListMenus();
                        });
                    }
                    else
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                    }
                }
                else
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                }
            });
        }
    }
}
