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
using TsukiTag.Models.Repository;

namespace TsukiTag.ViewModels
{
    public class ViewModelCollectionHandlerBase : ViewModelBase
    {
        protected readonly IDbRepository dbRepository;
        protected readonly INotificationControl notificationControl;
        protected readonly IPictureWorker pictureWorker;

        public ReactiveCommand<Unit, Unit> AddToDefaultListCommand { get; protected set; }
        public ReactiveCommand<Guid, Unit> AddToSpecificListCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> AddToEligibleListsCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> AddToAllListsCommand { get; protected set; }
        public ReactiveCommand<Guid, Unit> RemoveFromSpecificListCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> RemoveFromAllListCommand { get; protected set; }

        public ReactiveCommand<Unit, Unit> AddToDefaultWorkspaceCommand { get; protected set; }
        public ReactiveCommand<Guid, Unit> AddToSpecificWorkspaceCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> AddToEligibleWorkspacesCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> AddToAllWorkspacesCommand { get; protected set; }
        public ReactiveCommand<Guid, Unit> RemoveFromSpecificWorkspaceCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> RemoveFromAllWorkspaceCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> OpenInDefaultApplicationCommand { get; protected set; }
        public ReactiveCommand<Guid, Unit> ApplyMetadataGroupCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> SavePictureChangesCommand{ get; protected set; }


        public ViewModelCollectionHandlerBase(
            IDbRepository dbRepository,
            IPictureWorker pictureWorker,
            INotificationControl notificationControl
        )
        {
            this.dbRepository = dbRepository;
            this.notificationControl = notificationControl;
            this.pictureWorker = pictureWorker;
        }

        public virtual void Reinitialize()
        {

        }

        protected async Task OnOpenInDefaultApplication(Picture picture)
        {
            await Task.Run(async () =>
            {
                this.pictureWorker.OpenPictureInDefaultApplication(picture, picture.LocalProviderId != null ? this.dbRepository.Workspace.Get(picture.LocalProviderId.Value) : null);
            });
        }

        protected async Task OnRemoveFromAllWorkspaces(Picture picture, bool notify = true)
        {
            await Task.Run(async () =>
            {
                var workspacePictures = this.dbRepository.WorkspacePicture.GetAllForPicture(picture.Md5);
                var workspaces = this.dbRepository.Workspace.GetAll().Where(w => workspacePictures.Any(ww => ww.ResourceListId == w.Id)).ToList();

                foreach (var workspacePicture in workspacePictures)
                {
                    if (this.dbRepository.WorkspacePicture.RemoveFromWorkspace(workspacePicture.ResourceListId, workspacePicture.Picture))
                    {
                        await this.pictureWorker.DeletePicture(workspacePicture, workspaces.FirstOrDefault(w => w.Id == workspacePicture.ResourceListId));
                    }
                    else
                    {
                        if (notify)
                        {
                            await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                        }

                        return;
                    }
                }

                if (notify)
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionRemoveFromAllSuccess, Language.Workspaces.ToLower()), "workspace"));
                }

                Reinitialize();
            });
        }

        protected async Task OnRemoveFromAllLists(Picture picture, bool notify = true)
        {
            await Task.Run(async () =>
            {
                if (this.dbRepository.OnlineListPicture.RemoveFromAllLists(picture))
                {
                    if (notify)
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionRemoveFromAllSuccess, Language.OnlineLists.ToLower()), "onlinelist"));
                    }

                    Reinitialize();
                }
                else
                {
                    if (notify)
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                    }
                }
            });
        }

        protected async Task OnRemoveFromSpecificWorkspace(Guid id, Picture picture, bool notify = true)
        {
            await Task.Run(async () =>
            {
                var workspace = this.dbRepository.Workspace.Get(id);
                if (workspace != null)
                {
                    var workspacePicture = this.dbRepository.WorkspacePicture.GetAllForPicture(picture.Md5).FirstOrDefault(i => i.ResourceListId == id);
                    if (workspacePicture != null)
                    {
                        if (this.dbRepository.WorkspacePicture.RemoveFromWorkspace(id, picture))
                        {
                            await this.pictureWorker.DeletePicture(workspacePicture, workspace);
                        }

                        if (notify)
                        {
                            await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionRemoveFromSuccess, Language.Workspace.ToLower(), workspace.Name), "workspace"));
                        }

                        Reinitialize();
                        return;
                    }
                }

                if (notify)
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                }
            });
        }

        protected async Task OnRemoveFromSpecificList(Guid id, Picture picture, bool notify = true)
        {
            await Task.Run(async () =>
            {
                var list = this.dbRepository.OnlineList.Get(id);
                if (list != null)
                {
                    if (this.dbRepository.OnlineListPicture.RemoveFromList(id, picture))
                    {
                        if (notify)
                        {
                            await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionRemoveFromSuccess, Language.OnlineList.ToLower(), list.Name), "onlinelist"));
                        }

                        Reinitialize();
                    }
                    else
                    {
                        if (notify)
                        {
                            await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                        }
                    }
                }
                else
                {
                    if (notify)
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                    }
                }
            });
        }

        protected async Task<bool> OnAddToAllWorkspaces(Picture picture, bool notify = true)
        {
            return await OnAddToWorkspaces(this.dbRepository.Workspace.GetAll(), picture, true);
        }

        protected async Task OnAddToAllLists(Picture picture, bool notify = true)
        {
            await Task.Run(async () =>
            {
                if (this.dbRepository.OnlineListPicture.AddToAllLists(picture))
                {
                    if (notify)
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionAddToAllSuccess, Language.OnlineLists.ToLower())));
                    }

                    Reinitialize();
                }
                else
                {
                    if (notify)
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                    }
                }
            });
        }

        protected async Task<bool> OnAddToEligibleWorkspaces(Picture picture, bool notify = true)
        {
            return await OnAddToWorkspaces(this.dbRepository.Workspace.GetAll().Where(w => w.IsEligible(picture)).ToList(), picture, notify);
        }

        protected async Task OnAddToEligibleLists(Picture picture, bool notify = true)
        {
            await Task.Run(async () =>
            {
                if (this.dbRepository.OnlineListPicture.AddToAllEligible(picture))
                {
                    if (notify)
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionAddToEligibleSuccess, Language.OnlineLists.ToLower())));
                    }

                    Reinitialize();
                }
                else
                {
                    if (notify)
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                    }
                }
            });
        }

        protected async Task<bool> OnImportToSpecificWorkspace(Guid id, string filePath, bool notify = true, bool reinitialize = true)
        {
            return await Task.Run<bool>(async () =>
            {
                var workspace = this.dbRepository.Workspace.Get(id);

                if (notify)
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Uncloseable(string.Format(Language.ToastWorkspaceProcessingSingle, filePath, workspace.Name), "workspace"));
                }

                var picture = await this.pictureWorker.CreatePictureMetadataFromLocalImage(filePath);

                if (picture != null)
                {
                    return await OnAddToSpecificWorkspace(id, picture, notify, reinitialize);
                }

                if (notify)
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError, "workspace"));
                }

                return false;
            });
        }

        protected async Task<bool> OnAddToSpecificWorkspace(Guid id, Picture picture, bool notify = true, bool reinitialize = true)
        {
            return await Task.Run<bool>(async () =>
            {
                var workspace = this.dbRepository.Workspace.Get(id);

                await this.notificationControl.SendToastMessage(ToastMessage.Uncloseable(string.Format(Language.ToastWorkspaceProcessingSingle, picture.Id, workspace.Name), "workspace"));


                var pictureClone = picture.MetadatawiseClone();
                pictureClone.PreviewImage = picture.PreviewImage;
                pictureClone.SampleImage = picture.SampleImage;

                pictureClone = workspace.ProcessPicture(picture);

                var filePath = await this.pictureWorker.SaveWorkspacePicture(pictureClone, workspace);
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.dbRepository.WorkspacePicture.AddToWorkspace(id, pictureClone, filePath);

                    if (notify)
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionAddToSuccess, Language.Workspace.ToLower(), workspace.Name), "workspace"));
                    }

                    if (reinitialize)
                    {
                        Reinitialize();
                    }

                    return true;
                }
                else
                {
                    if (notify)
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError, "workspace"));
                    }

                    return false;
                }
            });
        }

        protected async Task OnAddToSpecificList(Guid id, Picture picture, bool notify = true)
        {
            await Task.Run(async () =>
            {
                var list = this.dbRepository.OnlineList.Get(id);
                if (list != null)
                {
                    if (this.dbRepository.OnlineListPicture.AddToList(id, picture))
                    {
                        if (notify)
                        {
                            await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionAddToSuccess, Language.OnlineList.ToLower(), list.Name)));
                        }

                        Reinitialize();
                    }
                    else
                    {
                        if (notify)
                        {
                            await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                        }
                    }
                }
                else
                {
                    if (notify)
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                    }
                }
            });
        }

        protected async Task<bool> OnAddToDefaultWorkspace(Picture picture, bool notify = true)
        {
            var workspace = this.dbRepository.Workspace.GetDefault();
            if (workspace != null)
            {
                return await OnAddToSpecificWorkspace(workspace.Id, picture, notify);
            }
            else
            {
                return false;
            }
        }

        protected async Task OnAddToDefaulList(Picture picture, bool notify = true)
        {
            await Task.Run(async () =>
           {
               var list = this.dbRepository.OnlineList.GetDefault();
               if (list != null)
               {
                   if (this.dbRepository.OnlineListPicture.AddToList(list.Id, picture))
                   {
                       if (notify)
                       {
                           await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionAddToSuccess, Language.OnlineList.ToLower(), list.Name)));
                       }

                       Reinitialize();
                   }
                   else
                   {
                       if (notify)
                       {
                           await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                       }
                   }
               }
               else
               {
                   if (notify)
                   {
                       await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ActionGenericError));
                   }
               }
           });
        }

        protected async Task<bool> OnAddToWorkspaces(List<Workspace> workspaces, Picture picture, bool notify = true)
        {
            return await Task.Run<bool>(async () =>
            {
                for (var i = 0; i < workspaces.Count; i++)
                {
                    var workspace = workspaces[i];

                    if (notify)
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Uncloseable(string.Format(Language.ToastWorkspaceProcessing, picture.Id, workspace.Name, i + 1, workspaces.Count), "workspace"));
                    }

                    if (!await OnAddToSpecificWorkspace(workspace.Id, picture, false, false))
                    {
                        if (notify)
                        {
                            await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ToastWorkspaceProcessError, "workspace"));
                        }

                        return false;
                    }
                }

                if (notify)
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ToastWorkspaceProcessed, "workspace"));
                }

                Reinitialize();
                return true;
            });
        }

        protected async Task<bool> OnSaveChanges(Picture picture, bool notify = true)
        {
            return await Task.Run<bool>(async () =>
            {

                try
                {
                    if(picture.IsLocal && picture.LocalProviderId != null)
                    {
                        if(picture.IsWorkspace)
                        {
                            return await OnAddToSpecificWorkspace(picture.LocalProviderId.Value, picture, notify, true);
                        }
                        else
                        {
                            await OnAddToSpecificList(picture.LocalProviderId.Value, picture, notify);
                            return true;
                        }
                    }

                    return false;
                }
                catch(Exception)
                {
                    return false;
                }
            });
        }

        protected async Task<bool> OnApplyMetadataGroup(Guid id, Picture picture, bool notify = true)
        {
            return await Task.Run<bool>(async () =>
            {
                try
                {
                    var metadataGroup = this.dbRepository.MetadataGroup.Get(id);
                    if (metadataGroup != null)
                    {
                        picture = metadataGroup.ProcessPicture(picture);

                        if (notify)
                        {
                            await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ToastMetadataGroupAppliedSingle, "metadatagroup"));
                        }

                        return true;
                    }
                }
                catch (Exception)
                {
                }

                if (notify)
                {
                    await this.notificationControl.SendToastMessage(ToastMessage.Closeable(Language.ToastMetadataGroupApplyError, "metadatagroup"));
                }

                return false;
            });
        }
    }
}
