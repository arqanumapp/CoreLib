using CoreLib.Configurations;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Sockets
{
    internal class NotificationsListener(ApiConfiguration apiConfiguration)
    {
        private HubConnection? _connection;

        public bool IsConnected => _connection?.State == HubConnectionState.Connected;

        public async Task<bool> StartListeningAsync(string deviceId, byte[] signature/* ,Action<DeviceInfo> onNewDevice*/)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl($"{apiConfiguration.ApiBaseUrl}notificationshub")
                .WithAutomaticReconnect()
                .Build();

            //_connection.On<DeviceInfo>("NewDeviceConnected", onNewDevice);

            await _connection.StartAsync();

            if (_connection.State != HubConnectionState.Connected)
                return false;

            // Передаём deviceId и подпись в серверный метод Subscribe
            await _connection.InvokeAsync("Subscribe", deviceId, Convert.ToBase64String(signature));

            return true;
        }


        public async Task StopAsync()
        {
            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
    }

}
