using CoreLib.Configurations;
using CoreLib.Crypto;
using CoreLib.Helpers;
using CoreLib.Services;
using CoreLib.Services.Account;
using CoreLib.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLib
{
    public static class CoreExtensions
    {
        private static IServiceCollection AddConfigurations(this IServiceCollection services)
        {
            services.AddSingleton(new ApiConfiguration());
            return services;
        }

        private static IServiceCollection AddCryptoServices(this IServiceCollection services)
        {
            services.AddSingleton<IShakeGenerator, ShakeGenerator>();
            services.AddSingleton<IKeyDerivationService, KeyDerivationService>();
            services.AddSingleton<IMLDsaKey, MLDsaKey>();
            services.AddSingleton<IMLKemKey, MLKemKey>();
            services.AddSingleton<IAesGCMKey, AesGCMKey>();
            return services;
        }

        private static IServiceCollection AddStorageServices(this IServiceCollection services)
        {
            services.AddSingleton<IAccountStorage, AccountStorage>();
            services.AddSingleton<IDeviceStorage, DeviceStorage>();
            services.AddSingleton<IPreKeyStorage, PreKeyStorage>();
            return services;
        }

        private static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddTransient<IDeviceService, DeviceService>();
            services.AddTransient<IContactService, ContactService>();

            services.AddSingleton<IPreKeyService, PreKeyService>();

            services.AddTransient<IProofOfWorkService, ProofOfWorkService>();
            services.AddTransient<IAddDeviceService, AddDeviceService>();

            services.AddSingleton<CreateAccountService>();
            return services;
        }
        public static IServiceCollection AddArqanumCore(this IServiceCollection services)
        {
            services.AddHttpClient<IApiService, ApiService>();
            services.AddConfigurations();
            services.AddCryptoServices();
            services.AddStorageServices();
            services.AddBusinessServices();

            var provider = services.BuildServiceProvider();
            var devInfoProvider = provider.GetService<IDeviceInfoProvider>();
            if (devInfoProvider == null)
                throw new InvalidOperationException("You must register an implementation of IDeviceInfoProvider before calling AddArqanumCore().");

            return services;
        }
    }
}
