using CoreLib.Helpers;
using CoreLib.Models.Entitys;

namespace CoreLib.Services
{
    internal class AccountService(IDeviceInfoProvider deviceInfoProvider)
    {
        public async Task<bool> CreateAsync(string nickName)
        {
            try
            {
                Account account = new()
                {
                    NickName = nickName
                };
                var deviceService = new DeviceService();

                var (deviceData, DilitiumPrK) = await deviceService.CreateAsync(await deviceInfoProvider.GetDeviceName());

                var preKeyService = new PreKeyService();

                List<PreKey> preKeys = [];

                for (int i = 0; i < 50; i++)
                {
                    var preKey = await preKeyService.CreateAsync(DilitiumPrK, deviceData.Id);
                    preKeys.Add(preKey);
                }
                //TODO: Send data to server and save in local storage
                //Add proof of work?
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating account", ex);
            }
        }
    }
}
