using CoreLib.Configurations;
using CoreLib.Crypto;
using CoreLib.Helpers;
using CoreLib.Notifications;
using CoreLib.Notifications.Handlers;
using CoreLib.Services;
using CoreLib.Services.Account;
using CoreLib.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLib
{
    public static class CoreExtensions
    {
        private static IServiceCollection AddConfigurations(this IServiceCollection services)
        {
            services.AddSingleton(new ApiConfiguration());
            services.AddHttpClient<IApiService, ApiService>();
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
        private static IServiceCollection AddNotificatioServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(AddNewDeviceNotificationHandler).Assembly);
            });

            services.AddSingleton<INotificationService, NotificationService>();

            return services;
        }

        private static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddTransient<IDeviceService, DeviceService>();
            services.AddTransient<IContactService, ContactService>();

            services.AddSingleton<IPreKeyService, PreKeyService>();

            services.AddTransient<IProofOfWorkService, ProofOfWorkService>();
            services.AddTransient<IAddDeviceService, AddDeviceService>();

            services.AddTransient<CreateAccountService>();
            return services;
        }
        public static IServiceCollection AddArqanumCore(this IServiceCollection services)
        {
            services.AddConfigurations();
            services.AddCryptoServices();
            services.AddStorageServices();
            services.AddBusinessServices();
            services.AddNotificatioServices();
            var provider = services.BuildServiceProvider();

            _ = provider.GetService<IDatabasePasswordProvider>()
               ?? throw new InvalidOperationException("You must register an implementation of IDatabasePasswordProvider before calling AddArqanumCore().");

            _ = provider.GetService<IDeviceInfoProvider>()
                ?? throw new InvalidOperationException("You must register an implementation of IDeviceInfoProvider before calling AddArqanumCore().");

            _ = provider.GetService<INotificationDisplayService>()
                ?? throw new InvalidOperationException("You must register an implementation of INotificationDisplayService before calling AddArqanumCore()."); ;


            return services;
        }
    }
}
