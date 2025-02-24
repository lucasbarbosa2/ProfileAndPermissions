using Microsoft.Extensions.Hosting;
using ProfileAndPermissions.Domain.Interfaces;

namespace ProfileAndPermissions.Application.Background
{
    /// <summary>
    /// This background service is used to toggle a action from a profile on and off
    /// </summary>
    public class ToggleConfigurationService : BackgroundService
    {
        private readonly IProfileConfigurationService _profileConfigurationService;

        public ToggleConfigurationService(IProfileConfigurationService profileConfigurationService)
        {
            _profileConfigurationService = profileConfigurationService;
        }

        /// <summary>
        /// This method toggles a action from a profile on and off every 5 minutes
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _profileConfigurationService.ToogleBoolConfiguration("Admin", "CanEdit", cancellationToken);

                await Task.Delay(300000, cancellationToken);
            }
        }
    }
}
