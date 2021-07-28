using LiteDB;
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
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly IDbRepository dbRepository;
        private readonly INotificationControl notificationControl;

        public ReactiveCommand<Unit, Unit> SettingsCancelledCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SettingsSavedCommand { get; set; }

        public SettingsViewModel(
            IDbRepository dbRepository,
            INotificationControl notificationControl
        )
        {
            this.dbRepository = dbRepository;
            this.notificationControl = notificationControl;

            this.AddNewListCommand = ReactiveCommand.CreateFromTask(async () => { OnNewListAdded(); });
            this.SetToDefaultCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnSetToDefault(id); });
            this.RemoveListCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnRemoveList(id); });
            this.AddTagsToAddCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnAddTagstoAdd(id); });
            this.AddTagsToRemoveCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnAddTagstoRemove(id); });
            this.AddOptionalConditionTagCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnAddOptionalConditionTag(id); });
            this.AddMandatoryConditionTagCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnAddMandatoryConditionTag(id); });
            this.SettingsCancelledCommand = ReactiveCommand.CreateFromTask(async () => { OnSettingsCancelled(); });
            this.SettingsSavedCommand = ReactiveCommand.CreateFromTask(async () => { OnSettingsSaved(); });
        }

        public async void Initialize()
        {
            OnlineLists = new ObservableCollection<OnlineList>(this.dbRepository.OnlineList.GetAll());
        }

        public async void OnSettingsCancelled()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                this.Initialize();
            });
        }

        public async void OnSettingsSaved()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                try
                {
                    if (onlineLists.Any(l => string.IsNullOrEmpty(l.Name)))
                    {
                        this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.SettingsListNoName, "settingslistnoname"));
                    }
                    else if (onlineLists.Any(l => onlineLists.Any(ll => ll.Name == l.Name && ll.Id != l.Id)))
                    {
                        this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.SettingsListNotUnique, "settingslistnotunique"));
                    }
                    else
                    {
                        foreach (var list in onlineLists)
                        {
                            list.CurrentTagToAdd = string.Empty;
                            list.CurrentTagToRemove = string.Empty;
                            list.TagsToAdd = list.TagsToAdd == null ? null : list.TagsToAdd.Except(new string[] { string.Empty }).ToArray();
                            list.TagsToRemove = list.TagsToRemove == null ? null : list.TagsToRemove.Except(new string[] { string.Empty }).ToArray();
                        }


                        this.dbRepository.OnlineList.AddOrUpdate(onlineLists.ToList());
                        this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.SettingsSaved, "settingssaved"));
                        this.Initialize();
                    }
                }
                catch (Exception ex)
                {
                    this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.SettingsSaveError));
                }
            });
        }
    }
}
