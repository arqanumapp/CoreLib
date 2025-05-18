namespace CoreLib.Helpers
{
    public interface IDeviceInfoProvider
    {
        Task<string> GetDeviceName();
    }
}