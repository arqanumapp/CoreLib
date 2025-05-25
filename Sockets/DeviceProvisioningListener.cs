using CoreLib.Configurations;
using Microsoft.AspNetCore.SignalR.Client;

namespace CoreLib.Sockets
{
    internal class DeviceProvisioningListener(ApiConfiguration apiConfiguration)
    {
        private HubConnection? _connection;

        public bool IsConnected => _connection?.State == HubConnectionState.Connected;

        public async Task<bool> StartListeningAsync(string channelId, Func<byte[], Task> onMessageReceived)
        {
            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl("https://localhost:7111/devicehub") 
                    .WithAutomaticReconnect()
                    .Build();

                _connection.On<byte[]>("ReceiveProvisioningResponse", async (data) =>
                {
                    await onMessageReceived(data);
                });

                await _connection.StartAsync();

                if (_connection.State != HubConnectionState.Connected)
                    return false;

                await _connection.InvokeAsync("SubscribeToProvisioningChannel", channelId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task StopAsync()
        {
            if (_connection is not null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
            }
        }
    }
}
