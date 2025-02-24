using Moq;
using ProfileAndPermissions.Application.Background;
using ProfileAndPermissions.Domain.Interfaces;

namespace ProfileAndPermissions.Tests.Services
{
    public class ToggleConfigurationServiceTests
    {
        private readonly Mock<IProfileConfigurationService> _mockProfileConfigurationService;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ToggleConfigurationServiceTests()
        {
            _mockProfileConfigurationService = new Mock<IProfileConfigurationService>();
            _cancellationTokenSource = new CancellationTokenSource();
        }
        [Fact]
        public async Task ExecuteAsync_ShouldCallToogleBoolConfiguration_AtLeastOnce()
        {
            var service = new ToggleConfigurationService(_mockProfileConfigurationService.Object);
            var token = _cancellationTokenSource.Token;

            var serviceTask = service.StartAsync(token);

            await Task.Delay(100, token);

            _cancellationTokenSource.Cancel();

            await serviceTask;

            _mockProfileConfigurationService.Verify(
                x => x.ToogleBoolConfiguration("Admin", "CanEdit", It.IsAny<CancellationToken>()),
                Times.AtLeastOnce()
            );
        }
        [Fact]
        public async Task ExecuteAsync_ShouldStopExecution_WhenCancellationRequested()
        {
            var service = new ToggleConfigurationService(_mockProfileConfigurationService.Object);
            var token = _cancellationTokenSource.Token;

            var serviceTask = service.StartAsync(token);

            _cancellationTokenSource.CancelAfter(100);

            await serviceTask;

            _mockProfileConfigurationService.Verify(
                x => x.ToogleBoolConfiguration("Admin", "CanEdit", It.IsAny<CancellationToken>()),
                Times.AtLeastOnce()
            );

            Assert.True(serviceTask.IsCompleted);
        }
    }
}
