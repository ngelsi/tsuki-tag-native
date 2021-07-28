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
    public class ViewModelCollectionHandlerBase : ViewModelBase
    {
        protected readonly IDbRepository dbRepository;
        protected readonly INotificationControl notificationControl;

        public ReactiveCommand<Unit, Unit> AddToDefaultListCommand { get; protected set; }
        public ReactiveCommand<Guid, Unit> AddToSpecificListCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> AddToEligibleListsCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> AddToAllListsCommand { get; protected set; }
        public ReactiveCommand<Guid, Unit> RemoveFromSpecificListCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> RemoveFromAllListCommand { get; protected set; }

        public ViewModelCollectionHandlerBase(
            IDbRepository dbRepository,
            INotificationControl notificationControl
        )
        {
            this.dbRepository = dbRepository;
            this.notificationControl = notificationControl;
        }

        public virtual void Reinitialize()
        {

        }

        protected async Task OnRemoveFromAllLists(Picture picture, bool notify = true)
        {
            await Task.Run(async () =>
            {
                if (this.dbRepository.OnlineListPicture.RemoveFromAllLists(picture))
                {
                    if (notify)
                    {
                        await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionRemoveFromAllSuccess, Language.OnlineLists.ToLower())));
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
                            await this.notificationControl.SendToastMessage(ToastMessage.Closeable(string.Format(Language.ActionRemoveFromSuccess, Language.OnlineList.ToLower(), list.Name)));
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
    }
}
