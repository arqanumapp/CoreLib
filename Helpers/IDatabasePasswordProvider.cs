namespace CoreLib.Helpers
{
    public interface IDatabasePasswordProvider
    {
        Task<string> GetDatabasePassword();
    }
}
