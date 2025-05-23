namespace CoreLib.Interfaces
{
    public interface IDeviceInfoProvider
    {
        Task<string> GetDeviceName();
    }
}