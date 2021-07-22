using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models;

namespace TsukiTag.Dependencies
{
    public interface INotificationControl
    {
        event EventHandler<ToastMessage> ToastMessageReceived;

        Task SendToastMessage(ToastMessage message);
    }

    public class NotificationControl : INotificationControl
    {
        public event EventHandler<ToastMessage> ToastMessageReceived;

        public async Task SendToastMessage(ToastMessage message)
        {
            await Task.Run(() =>
            {
                ToastMessageReceived?.Invoke(this, message);
            });
        }
    }
}
