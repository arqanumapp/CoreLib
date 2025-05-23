using CoreLib.Interfaces;
using CoreLib.Models.Entitys;

namespace CoreLib.Storage
{
    public interface IAccountStorage
    {
        Task<bool> SaveAccountAsync(Account account);
        Task<Account?> GetAccountAsync();
    }
    internal class AccountStorage(IDatabasePasswordProvider passwordProvider) : BaseStorage<Account>(passwordProvider), IAccountStorage
    {
        public async Task<bool> SaveAccountAsync(Account account)
        {
            try
            {
                await EnsureInitializedAsync();
                var result = await _database.InsertAsync(account);
                return result > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<Account?> GetAccountAsync()
        {
            try
            {
                await EnsureInitializedAsync();
                var account = await _database.Table<Account>().FirstOrDefaultAsync();
                return account;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
