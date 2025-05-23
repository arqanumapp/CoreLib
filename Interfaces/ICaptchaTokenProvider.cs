namespace CoreLib.Interfaces
{
    public interface ICaptchaTokenProvider
    {
        Task<string> GetCaptchaTokenAsync();
    }
}
