using CoreLib.Helpers;
using SQLite;

namespace CoreLib.Storage
{
    class BaseStorage<T> where T : new()
    {
        protected readonly SQLiteAsyncConnection _database;
        protected BaseStorage(IDatabasePasswordProvider passwordProvider)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string databasePath = Path.Combine(folderPath, "AppStorage4.db");
            _database = new SQLiteAsyncConnection(databasePath);
            _database.ExecuteAsync($"PRAGMA key = '{passwordProvider.GetDatabasePassword()}';").Wait();
            _database.CreateTableAsync<T>().Wait();
        }
    }
}
