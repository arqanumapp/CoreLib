using CoreLib.Interfaces;
using SQLite;

class BaseStorage<T> where T : new()
{
    protected readonly SQLiteAsyncConnection _database;
    private readonly Lazy<Task> _initialization;
    private readonly IDatabasePasswordProvider _passwordProvider;

    protected BaseStorage(IDatabasePasswordProvider passwordProvider)
    {
        _passwordProvider = passwordProvider;
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string databasePath = Path.Combine(folderPath, "AppStorage5.db");
        _database = new SQLiteAsyncConnection(databasePath);

        _initialization = new Lazy<Task>(InitializeAsync);
    }

    private async Task InitializeAsync()
    {
        string password = await _passwordProvider.GetDatabasePassword();
        await _database.ExecuteAsync($"PRAGMA key = '{password}';");
        await _database.CreateTableAsync<T>();
    }

    protected async Task EnsureInitializedAsync() => await _initialization.Value;
}
