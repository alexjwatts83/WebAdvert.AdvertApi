using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdvertApi.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AdvertApi.HealthChecks
{
    public class StorageHealthCheck : IHealthCheck
    {
        private readonly IAdvertStorageService _storageService;
        public StorageHealthCheck(IAdvertStorageService storageService)
        {
            _storageService = storageService;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var isStorageOk = await _storageService.CheckHealthAsync();

            if (isStorageOk)
            {
                return await Task.FromResult(HealthCheckResult.Healthy("A healthy result."));
            }

            return await Task.FromResult(HealthCheckResult.Unhealthy("An unhealthy result."));
        }
    }
}
