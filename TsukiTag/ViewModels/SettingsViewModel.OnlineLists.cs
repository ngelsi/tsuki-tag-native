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
    public partial class SettingsViewModel
    {
        private ObservableCollection<OnlineList> onlineLists;

        public ObservableCollection<OnlineList> OnlineLists
        {
            get { return onlineLists; }
            set
            {
                onlineLists = value;
                this.RaisePropertyChanged(nameof(OnlineLists));
            }
        }

        public ReactiveCommand<Unit, Unit> AddNewListCommand { get; set; }
        public ReactiveCommand<Guid, Unit> SetToDefaultCommand { get; set; }
        public ReactiveCommand<Guid, Unit> RemoveListCommand { get; set; }
        public ReactiveCommand<Guid, Unit> AddTagsToAddCommand { get; set; }
        public ReactiveCommand<Guid, Unit> AddTagsToRemoveCommand { get; set; }
        public ReactiveCommand<Guid, Unit> AddOptionalConditionTagCommand { get; set; }
        public ReactiveCommand<Guid, Unit> AddMandatoryConditionTagCommand { get; set; }

        public async void OnNewListAdded()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                OnlineLists.Add(new OnlineList() { Id = Guid.NewGuid(), IsDefault = false, Name = "list " + (onlineLists.Count + 1) });
            });
        }

        public async void OnSetToDefault(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                foreach (var list in OnlineLists)
                {
                    list.IsDefault = list.Id == id;
                }
            });
        }

        public async void OnRemoveList(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var list = OnlineLists.FirstOrDefault(l => l.Id == id);
                if (list != null)
                {
                    OnlineLists.Remove(list);
                }
            });
        }

        public async void OnAddTagstoAdd(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var list = OnlineLists.FirstOrDefault(l => l.Id == id);
                if (list != null && !string.IsNullOrEmpty(list.CurrentTagToAdd))
                {
                    list.TagsToAdd = list.TagsToAdd == null ? new string[] { list.CurrentTagToAdd } : list.TagsToAdd.Append(list.CurrentTagToAdd).Distinct().ToArray();
                    list.CurrentTagToAdd = string.Empty;
                }
            });
        }

        public async void OnAddOptionalConditionTag(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var list = OnlineLists.FirstOrDefault(l => l.Id == id);
                if (list != null && !string.IsNullOrEmpty(list.CurrentOptionalConditionTag))
                {
                    list.OptionalConditionTags = list.OptionalConditionTags == null ? new string[] { list.CurrentOptionalConditionTag } : list.OptionalConditionTags.Append(list.CurrentOptionalConditionTag).Distinct().ToArray();
                    list.CurrentOptionalConditionTag = string.Empty;
                }
            });
        }

        public async void OnAddMandatoryConditionTag(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var list = OnlineLists.FirstOrDefault(l => l.Id == id);
                if (list != null && !string.IsNullOrEmpty(list.CurrentMandatoryConditionTag))
                {
                    list.MandatoryConditionTags = list.MandatoryConditionTags == null ? new string[] { list.CurrentMandatoryConditionTag } : list.MandatoryConditionTags.Append(list.CurrentMandatoryConditionTag).Distinct().ToArray();
                    list.CurrentMandatoryConditionTag = string.Empty;
                }
            });
        }

        public async void OnAddTagstoRemove(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var list = OnlineLists.FirstOrDefault(l => l.Id == id);
                if (list != null && !string.IsNullOrEmpty(list.CurrentTagToRemove))
                {
                    list.TagsToRemove = list.TagsToRemove == null ? new string[] { list.CurrentTagToRemove } : list.TagsToRemove.Append(list.CurrentTagToRemove).Distinct().ToArray();
                    list.CurrentTagToRemove = string.Empty;
                }
            });
        }

        public async void OnRemoveTagsToAdd(Guid id, string tag)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var list = OnlineLists.FirstOrDefault(l => l.Id == id);
                if (list != null && list.TagsToAdd.Contains(tag))
                {
                    list.TagsToAdd = list.TagsToAdd.Except(new string[] { tag }).ToArray();
                }
            });
        }

        public async void OnRemoveTagsToRemove(Guid id, string tag)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var list = OnlineLists.FirstOrDefault(l => l.Id == id);
                if (list != null && list.TagsToRemove.Contains(tag))
                {
                    list.TagsToRemove = list.TagsToRemove.Except(new string[] { tag }).ToArray();
                }
            });
        }

        public async void OnRemoveOptionalConditionTag(Guid id, string tag)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var list = OnlineLists.FirstOrDefault(l => l.Id == id);
                if (list != null && list.OptionalConditionTags.Contains(tag))
                {
                    list.OptionalConditionTags = list.OptionalConditionTags.Except(new string[] { tag }).ToArray();
                }
            });
        }

        public async void OnRemoveMandatoryConditionTag(Guid id, string tag)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var list = OnlineLists.FirstOrDefault(l => l.Id == id);
                if (list != null && list.MandatoryConditionTags.Contains(tag))
                {
                    list.MandatoryConditionTags = list.MandatoryConditionTags.Except(new string[] { tag }).ToArray();
                }
            });
        }
    }
}
