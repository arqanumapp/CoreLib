using CoreLib.Interfaces;
using CoreLib.Models.Entitys;

namespace CoreLib.Storage
{
    public interface IPreKeyStorage
    {
        Task<bool> SavePreKeyAsync(PreKey preKey);
        Task<PreKey?> GetPreKeyAsync(string id);
        Task<bool> AddRangeAsync(List<PreKey> preKeys);
    }
    internal class PreKeyStorage(IDatabasePasswordProvider passwordProvider) : BaseStorage<PreKey>(passwordProvider), IPreKeyStorage
    {
        public async Task<bool> SavePreKeyAsync(PreKey preKey)
        {
            try
            {
                await EnsureInitializedAsync();
                var result = await _database.InsertAsync(preKey);
                return result > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<PreKey?> GetPreKeyAsync(string id)
        {
            try
            {
                await EnsureInitializedAsync();
                var preKey = await _database.Table<PreKey>().FirstOrDefaultAsync(x => x.Id == id);
                return preKey;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> AddRangeAsync(List<PreKey> preKeys)
        {
            try
            {
                await EnsureInitializedAsync();
                var result = await _database.InsertAllAsync(preKeys);
                return result > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
