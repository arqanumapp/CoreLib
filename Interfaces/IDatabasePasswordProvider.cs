namespace CoreLib.Interfaces
{
    public interface IDatabasePasswordProvider
    {
        Task<string> GetDatabasePassword();
    }
}
