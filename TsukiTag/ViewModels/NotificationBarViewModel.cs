using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TsukiTag.Dependencies;
using TsukiTag.Models;

namespace TsukiTag.ViewModels
{
    public class NotificationBarViewModel : ViewModelBase
    {
        private readonly INotificationControl notificationControl;

        private bool isTooltipOpen;
        private ObservableCollection<ToastMessage> messages;
        private Timer timeoutTimer;

        public ReactiveCommand<string, Unit> CloseToastMessageCommand { get; }

        public bool IsTooltipOpen
        {
            get { return isTooltipOpen; }
            set { isTooltipOpen = value; this.RaisePropertyChanged(nameof(IsTooltipOpen)); }
        }

        public ObservableCollection<ToastMessage> Messages
        {
            get { return messages; }
            set { messages = value; this.RaisePropertyChanged(nameof(Messages)); }
        }

        public NotificationBarViewModel(
            INotificationControl notificationControl
        )
        {
            this.notificationControl = notificationControl;
            this.notificationControl.ToastMessageReceived += OnToastMessageReceived;

            this.timeoutTimer = new Timer(7000);
            this.timeoutTimer.Elapsed += (e, args) => OnToastMessageTimeout();
            this.timeoutTimer.AutoReset = true;

            Messages = new ObservableCollection<ToastMessage>();
            CloseToastMessageCommand = ReactiveCommand.CreateFromTask<string>(async (id) =>
            {
                await this.CloseMessage(id);
            });
        }

        ~NotificationBarViewModel()
        {
            this.notificationControl.ToastMessageReceived -= OnToastMessageReceived;
        }

        private async Task CloseMessage(string id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var message = Messages.FirstOrDefault(m => m.Id == id);
                if (message != null)
                {
                    Messages.Remove(message);
                    if (Messages.Count == 0)
                    {
                        IsTooltipOpen = false;
                    }
                }
            });
        }

        private async void OnToastMessageReceived(object? sender, ToastMessage e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                this.timeoutTimer.Stop();

                if(!messages.Any(m => m.Id == e.Id))
                {
                    Messages.Add(e);
                }
                else
                {
                    var message = messages.FirstOrDefault(m => m.Id == e.Id);
                    if(message != null)
                    {
                        var index = messages.IndexOf(message);
                        
                        Messages.Insert(index, e);
                        Messages.Remove(message);
                    }
                }

                IsTooltipOpen = true;

                this.timeoutTimer.Start();
            });
        }

        private async void OnToastMessageTimeout()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                Messages = new ObservableCollection<ToastMessage>();
                IsTooltipOpen = false;
            });
        }
    }
}
