using CoreLib.Models.Entitys;

namespace CoreLib.Storage
{
    public interface IAccountStorage
    {
        Task<bool> SaveAccountAsync(Account account);
        Task<Account?> GetAccountAsync(string id);
    }
    internal class AccountStorage : BaseStorage<Account>, IAccountStorage
    {
        public async Task<bool> SaveAccountAsync(Account account)
        {
            try
            {
                var result = await _database.InsertAsync(account);
                return result > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<Account?> GetAccountAsync(string id)
        {
            try
            {
                var account = await _database.Table<Account>().FirstOrDefaultAsync(x => x.Id == id);
                return account;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
