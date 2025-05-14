using CoreLib.Models.Entitys;

namespace CoreLib.Storage
{
    public interface IDeviceStorage
    {
        Task<bool> SaveDeviceAsync(Device device);
        Task<Device?> GetDeviceAsync(string id);
    }
    internal class DeviceStorage : BaseStorage<Device>, IDeviceStorage
    {
        public async Task<bool> SaveDeviceAsync(Device device)
        {
            try
            {
                var result = await _database.InsertAsync(device);
                return result > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<Device?> GetDeviceAsync(string id)
        {
            try
            {
                var device = await _database.Table<Device>().FirstOrDefaultAsync(x => x.Id == id);
                return device;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
