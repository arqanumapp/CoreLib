using SQLite;

namespace CoreLib.Storage
{
    class BaseStorage<T> where T : new()
    {
        protected readonly SQLiteAsyncConnection _database;
        protected BaseStorage()
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string databasePath = Path.Combine(folderPath, "LocalStorage.db");
            _database = new SQLiteAsyncConnection(databasePath);
            _database.CreateTableAsync<T>().Wait();
        }
    }
}
