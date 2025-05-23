using CoreLib.Helpers;
using SQLite;

namespace CoreLib.Storage
{
    public class DatabaseProvider
    {
        public SQLiteAsyncConnection Database { get; private set; }

        public DatabaseProvider(IDatabasePasswordProvider passwordProvider)
        {
            InitializeAsync(passwordProvider).GetAwaiter().GetResult();
        }

        private async Task InitializeAsync(IDatabasePasswordProvider passwordProvider)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string databasePath = Path.Combine(folderPath, "AppStorage5.db");

            Database = new SQLiteAsyncConnection(databasePath);

            string password = await passwordProvider.GetDatabasePassword();
            await Database.ExecuteAsync($"PRAGMA key = '{password}';");
        }
    }
}
