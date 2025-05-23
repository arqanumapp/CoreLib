using CoreLib.Interfaces;
using CoreLib.Notifications.Events;
using CoreLib.Storage;
using MediatR;

namespace CoreLib.Notifications.Handlers
{
    public class AddNewDeviceNotificationHandler(IDeviceStorage deviceStorage, INotificationDisplayService notificationDisplayService) : INotificationHandler<AddNewDeviceNotificationEvent>
    {
        public Task Handle(AddNewDeviceNotificationEvent notificationEvent, CancellationToken cancellationToken)
        {
            notificationDisplayService.ShowNotificationAsync("");
            return Task.CompletedTask;
        }
    }
}
