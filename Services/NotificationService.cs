using CoreLib.Configurations;
using CoreLib.Notifications.Dtos.Device;
using CoreLib.Notifications.Events;
using MediatR;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;

namespace CoreLib.Services
{

    internal interface INotificationService
    {
        bool IsConnected { get; }

        Task<bool> ConnectAsync(string deviceId, byte[] signature);
        Task DisconnectAsync();
    }
    internal class NotificationService(ApiConfiguration apiConfiguration, IMediator mediator) : INotificationService
    {
        private HubConnection? _connection;

        public bool IsConnected => _connection?.State == HubConnectionState.Connected;


        public async Task<bool> ConnectAsync(string deviceId, byte[] signature)
        {
            if (IsConnected)
                return true;

            _connection = new HubConnectionBuilder()
                .WithUrl($"{apiConfiguration.ApiBaseUrl}notificationshub")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<byte[]>("NewDeviceConnected", async messagePackData =>
            {
                var notification = MessagePackSerializer.Deserialize<AddNewDeviceNotification>(messagePackData);
                if (notification != null)
                {
                    await mediator.Publish(new AddNewDeviceNotificationEvent(notification));
                }
            });

            try
            {
                await _connection.StartAsync();

                if (!IsConnected)
                    return false;

                await _connection.InvokeAsync("Subscribe", deviceId, Convert.ToBase64String(signature));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_connection is not null)
            {
                try
                {
                    await _connection.StopAsync();
                    await _connection.DisposeAsync();
                }
                finally
                {
                    _connection = null;
                }
            }
        }
    }
}
