using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.Services.Cache;

namespace ESD_EDI_BE.Services.Common
{
    public class InitializeCacheService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISysCacheService _sysCacheService;
        public InitializeCacheService(
            IServiceProvider serviceProvider
            , ISysCacheService sysCacheService
        )
        {
            _serviceProvider = serviceProvider;
            _sysCacheService = sysCacheService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                _sysCacheService.SetAvailableTokensToRedis();
                _sysCacheService.SetRoleMenusToRedis();
                _sysCacheService.SetRoleMissingMenuPermissionsToRedis();
                _sysCacheService.SetRoleMenuPermissionsToRedis();
                _sysCacheService.SetPoliciesToRedis();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
