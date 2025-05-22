namespace CoreLib.Notifications
{
    public interface INotificationDisplayService
    {
        Task ShowNotificationAsync(string data);
    }
}
