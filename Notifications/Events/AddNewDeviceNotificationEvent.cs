using CoreLib.Notifications.Dtos.Device;
using MediatR;

namespace CoreLib.Notifications.Events
{
    public class AddNewDeviceNotificationEvent(AddNewDeviceNotification notification) : INotification
    {
        public AddNewDeviceNotification Notification { get; } = notification;
    }
}
