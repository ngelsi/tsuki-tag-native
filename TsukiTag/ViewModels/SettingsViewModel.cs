using LiteDB;
using ReactiveUI;
using Serilog;
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

            this.metadataGroups = new ObservableCollection<MetadataGroup>();

            this.AddNewListCommand = ReactiveCommand.CreateFromTask(async () => { OnNewListAdded(); });
            this.SetToDefaultCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnSetToDefault(id); });
            this.RemoveListCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnRemoveList(id); });
            this.AddTagsToAddCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnAddTagstoAdd(id); });
            this.AddTagsToRemoveCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnAddTagstoRemove(id); });
            this.AddOptionalConditionTagCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnAddOptionalConditionTag(id); });
            this.AddMandatoryConditionTagCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnAddMandatoryConditionTag(id); });

            this.AddNewWorkspaceCommand = ReactiveCommand.CreateFromTask(async () => { OnNewWorkspaceAdded(); });
            this.SetWorkspaceToDefaultCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnSetWorkspaceToDefault(id); });
            this.RemoveWorkspaceCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnRemoveWorkspace(id); });
            this.AddWorkspaceTagsToAddCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnWorkspaceAddTagstoAdd(id); });
            this.AddWorkspaceTagsToRemoveCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnWorkspaceAddTagstoRemove(id); });
            this.AddWorkspaceOptionalConditionTagCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnWorkspaceAddOptionalConditionTag(id); });
            this.AddWorkspaceMandatoryConditionTagCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnWorkspaceAddMandatoryConditionTag(id); });
            this.BrowseWorkspaceFolderCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnWorkspaceBrowseFolder(id); });

            this.AddBlacklistTagCommand = ReactiveCommand.CreateFromTask(async () => { OnAddBlacklistTag(); });

            this.AddNewMetadataGroupCommand = ReactiveCommand.CreateFromTask(async () => { OnNewMetadataGroupAdded(); });
            this.SetMetadataGroupToDefaultCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnSetMetadataGroupToDefault(id); });
            this.RemoveMetadataGroupCommand = ReactiveCommand.CreateFromTask<Guid>(async (id) => { OnRemoveMetadataGroup(id); });

            this.SettingsCancelledCommand = ReactiveCommand.CreateFromTask(async () => { OnSettingsCancelled(); });
            this.SettingsSavedCommand = ReactiveCommand.CreateFromTask(async () => { OnSettingsSaved(); });
        }

        public async void Initialize()
        {
            OnlineLists = new ObservableCollection<OnlineList>(this.dbRepository.OnlineList.GetAll());
            Workspaces = new ObservableCollection<Workspace>(this.dbRepository.Workspace.GetAll());
            ApplicationSettings = this.dbRepository.ApplicationSettings.Get();
            MetadataGroups = new ObservableCollection<MetadataGroup>(this.dbRepository.MetadataGroup.GetAll());
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
                    else if (workspaces.Any(w => string.IsNullOrEmpty(w.Name)))
                    {
                        this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.SettingsWorkspaceNoName, "settingsworkspacenoname"));
                    }
                    else if (workspaces.Any(w => string.IsNullOrEmpty(w.FolderPath)))
                    {
                        this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.SettingsWorkspaceNoPath, "settingsworkspacenopath"));
                    }
                    else if (workspaces.Any(l => workspaces.Any(ll => ll.Name == l.Name && ll.Id != l.Id)))
                    {
                        this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.SettingsWorkspaceNotUnique, "settingsworkspacenotunique"));
                    }
                    else if (workspaces.Any(l => workspaces.Any(ll => ll.FolderPath == l.FolderPath && ll.Id != l.Id)))
                    {
                        this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.SettingsWorkspaceSamePath, "settingsworkspacesamepath"));
                    }
                    else if (metadataGroups.Any(w => string.IsNullOrEmpty(w.Name)))
                    {
                        this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.SettingsMetadataGroupNoName, "settingsmetadatagroupnoname"));
                    }
                    else if (metadataGroups.Any(l => metadataGroups.Any(ll => ll.Name == l.Name && ll.Id != l.Id)))
                    {
                        this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.SettingsMetadataGroupNotUnique, "settingsmetadatagroupnotunique"));
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

                        var illegalCharacters = System.IO.Path.GetInvalidFileNameChars();
                        foreach (var workspace in workspaces)
                        {
                            workspace.CurrentTagToAdd = string.Empty;
                            workspace.CurrentTagToRemove = string.Empty;
                            workspace.TagsToAdd = workspace.TagsToAdd == null ? null : workspace.TagsToAdd.Except(new string[] { string.Empty }).ToArray();
                            workspace.TagsToRemove = workspace.TagsToRemove == null ? null : workspace.TagsToRemove.Except(new string[] { string.Empty }).ToArray();
                            workspace.FileNameTemplate = new string(workspace.FileNameTemplate.Where(c => c == '#' || !illegalCharacters.Contains(c)).ToArray());

                            if (!workspace.FileNameTemplate.EndsWith(".#extension#", StringComparison.OrdinalIgnoreCase))
                            {
                                workspace.FileNameTemplate += ".#extension#";
                            }
                        }

                        this.dbRepository.OnlineList.AddOrUpdate(onlineLists.ToList());
                        this.dbRepository.Workspace.AddOrUpdate(workspaces.ToList());
                        this.dbRepository.ApplicationSettings.Set(ApplicationSettings);
                        this.dbRepository.MetadataGroup.AddOrUpdate(MetadataGroups.ToList());

                        this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.SettingsSaved, "settingssaved"));
                        this.Initialize();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Exception occurred while saving settings");
                    this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.SettingsSaveError));
                }
            });
        }
    }
}
