using CoreLib.Crypto;
using CoreLib.Helpers;
using CoreLib.Services;
using CoreLib.Services.Account;
using CoreLib.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLib
{
    public static class CoreExtensions
    {
        public static IServiceCollection AddCoreClient(this IServiceCollection services)
        {

            // Зависимости первого уровня
            services.AddSingleton<IDeviceService, DeviceService>();
            services.AddSingleton<IPreKeyService, PreKeyService>();

            services.AddSingleton<IAesGCMKey, AesGCMKey>();
            services.AddSingleton<IMLDsaKey, MLDsaKey>();
            services.AddSingleton<IMLKemKey, MLKemKey>();
            services.AddSingleton<IShakeGenerator, ShakeGenerator>();


            services.AddSingleton<IAccountStorage, AccountStorage>();
            services.AddSingleton<IDeviceStorage, DeviceStorage>();
            services.AddSingleton<IPreKeyStorage, PreKeyStorage>();

            services.AddSingleton<IProofOfWorkService, ProofOfWorkService>();


            // Основной сервис
            services.AddSingleton<CreateAccountService>();

            var provider = services.BuildServiceProvider();
            var devInfoProvider = provider.GetService<IDeviceInfoProvider>();
            if (devInfoProvider == null)
                throw new InvalidOperationException("You must register an implementation of IDeviceInfoProvider before calling AddEnigramClient().");

            return services;
        }
    }
}
