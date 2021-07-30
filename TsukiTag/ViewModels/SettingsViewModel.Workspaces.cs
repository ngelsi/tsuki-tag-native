using Avalonia;
using Avalonia.Controls;
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
        private ObservableCollection<Workspace> workspaces;

        public ObservableCollection<Workspace> Workspaces
        {
            get { return workspaces; }
            set
            {
                workspaces = value;
                this.RaisePropertyChanged(nameof(Workspaces));
            }
        }

        public ReactiveCommand<Unit, Unit> AddNewWorkspaceCommand { get; set; }
        public ReactiveCommand<Guid, Unit> SetWorkspaceToDefaultCommand { get; set; }
        public ReactiveCommand<Guid, Unit> RemoveWorkspaceCommand { get; set; }
        public ReactiveCommand<Guid, Unit> AddWorkspaceTagsToAddCommand { get; set; }
        public ReactiveCommand<Guid, Unit> AddWorkspaceTagsToRemoveCommand { get; set; }
        public ReactiveCommand<Guid, Unit> AddWorkspaceOptionalConditionTagCommand { get; set; }
        public ReactiveCommand<Guid, Unit> AddWorkspaceMandatoryConditionTagCommand { get; set; }
        public ReactiveCommand<Guid, Unit> BrowseWorkspaceFolderCommand { get; set; }

        public async void OnNewWorkspaceAdded()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                Workspaces.Add(new Workspace()
                {
                    Id = Guid.NewGuid(),
                    IsDefault = workspaces == null || workspaces.Count == 0,
                    Name = "workspace " + (Workspaces.Count + 1),
                    FileNameTemplate = "#md5#_#provider#.#extension#"
                });
            });
        }

        public async void OnSetWorkspaceToDefault(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                foreach (var workspace in Workspaces)
                {
                    workspace.IsDefault = workspace.Id == id;
                }
            });
        }

        public async void OnRemoveWorkspace(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var workspace = Workspaces.FirstOrDefault(l => l.Id == id);
                if (workspace != null)
                {
                    Workspaces.Remove(workspace);
                }
            });
        }

        public async void OnWorkspaceAddTagstoAdd(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var workspace = Workspaces.FirstOrDefault(l => l.Id == id);
                if (workspace != null && !string.IsNullOrEmpty(workspace.CurrentTagToAdd))
                {
                    workspace.TagsToAdd = workspace.TagsToAdd == null ? new string[] { workspace.CurrentTagToAdd } : workspace.TagsToAdd.Append(workspace.CurrentTagToAdd).ToArray();
                    workspace.CurrentTagToAdd = string.Empty;
                }
            });
        }

        public async void OnWorkspaceAddOptionalConditionTag(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var workspace = Workspaces.FirstOrDefault(l => l.Id == id);
                if (workspace != null && !string.IsNullOrEmpty(workspace.CurrentOptionalConditionTag))
                {
                    workspace.OptionalConditionTags = workspace.OptionalConditionTags == null ? new string[] { workspace.CurrentOptionalConditionTag } : workspace.OptionalConditionTags.Append(workspace.CurrentOptionalConditionTag).ToArray();
                    workspace.CurrentOptionalConditionTag = string.Empty;
                }
            });
        }

        public async void OnWorkspaceAddMandatoryConditionTag(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var workspace = Workspaces.FirstOrDefault(l => l.Id == id);
                if (workspace != null && !string.IsNullOrEmpty(workspace.CurrentMandatoryConditionTag))
                {
                    workspace.MandatoryConditionTags = workspace.MandatoryConditionTags == null ? new string[] { workspace.CurrentMandatoryConditionTag } : workspace.MandatoryConditionTags.Append(workspace.CurrentMandatoryConditionTag).ToArray();
                    workspace.CurrentMandatoryConditionTag = string.Empty;
                }
            });
        }

        public async void OnWorkspaceAddTagstoRemove(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var workspace = Workspaces.FirstOrDefault(l => l.Id == id);
                if (workspace != null && !string.IsNullOrEmpty(workspace.CurrentTagToRemove))
                {
                    workspace.TagsToRemove = workspace.TagsToRemove == null ? new string[] { workspace.CurrentTagToRemove } : workspace.TagsToRemove.Append(workspace.CurrentTagToRemove).ToArray();
                    workspace.CurrentTagToRemove = string.Empty;
                }
            });
        }

        public async void OnWorkspaceRemoveTagsToAdd(Guid id, string tag)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var workspace = Workspaces.FirstOrDefault(l => l.Id == id);
                if (workspace != null && workspace.TagsToAdd.Contains(tag))
                {
                    workspace.TagsToAdd = workspace.TagsToAdd.Except(new string[] { tag }).ToArray();
                }
            });
        }

        public async void OnWorkspaceRemoveTagsToRemove(Guid id, string tag)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var workspace = Workspaces.FirstOrDefault(l => l.Id == id);
                if (workspace != null && workspace.TagsToRemove.Contains(tag))
                {
                    workspace.TagsToRemove = workspace.TagsToRemove.Except(new string[] { tag }).ToArray();
                }
            });
        }

        public async void OnWorkspaceRemoveOptionalConditionTag(Guid id, string tag)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var workspace = Workspaces.FirstOrDefault(l => l.Id == id);
                if (workspace != null && workspace.OptionalConditionTags.Contains(tag))
                {
                    workspace.OptionalConditionTags = workspace.OptionalConditionTags.Except(new string[] { tag }).ToArray();
                }
            });
        }

        public async void OnWorkspaceRemoveMandatoryConditionTag(Guid id, string tag)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var workspace = Workspaces.FirstOrDefault(l => l.Id == id);
                if (workspace != null && workspace.MandatoryConditionTags.Contains(tag))
                {
                    workspace.MandatoryConditionTags = workspace.MandatoryConditionTags.Except(new string[] { tag }).ToArray();
                }
            });
        }

        public async void OnWorkspaceBrowseFolder(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var dialog = new OpenFolderDialog();
                var result = await dialog.ShowAsync(App.MainWindow);

                if(!string.IsNullOrEmpty(result))
                {
                    var workspace = Workspaces.FirstOrDefault(l => l.Id == id);
                    if(workspace != null)
                    {
                        workspace.FolderPath = result;
                    }
                }
            });
        }
    }
}
