using Microsoft.Extensions.Logging;
using Moq;
using ProfileAndPermissions.Application.Services;
using ProfileAndPermissions.Domain.Request;

namespace ProfileAndPermissions.Tests.Services
{
    public class ProfileConfigurationServiceTests
    {
        private readonly Mock<ILogger<ProfileConfigurationService>> _mockLogger;
        private readonly ProfileConfigurationService _service;

        public ProfileConfigurationServiceTests()
        {
            _mockLogger = new Mock<ILogger<ProfileConfigurationService>>();
            _service = new ProfileConfigurationService(_mockLogger.Object);
        }

        [Fact]
        public void GetAllProfiles_ShouldReturnProfiles()
        {
            var result = _service.GetAllProfiles();

            Assert.NotNull(result);
            Assert.Contains("Admin", result.Keys);
            Assert.Equal("Admin", result["Admin"].ProfileName);
            Assert.Equal("true", result["Admin"].Parameters["CanEdit"]);
            Assert.Equal("true", result["Admin"].Parameters["CanDelete"]);
        }

        [Fact]
        public void GetProfile_ShouldReturnProfile()
        {
            var result = _service.GetProfile("Admin");

            Assert.NotNull(result);
            Assert.Contains("Admin", result.ProfileName);
            Assert.Equal("Admin", result.ProfileName);
            Assert.Equal("true", result.Parameters["CanEdit"]);
            Assert.Equal("true", result.Parameters["CanDelete"]);
        }

        [Fact]
        public void GetProfile_ShouldNotReturnProfile_IfDoesntExist()
        {
            var result = _service.GetProfile("NonExisting");

            Assert.Null(result);
        }

        [Fact]
        public async Task AddProfileAsync_ShouldAddProfile_WhenProfileDoesNotExist()
        {
            var request = new AddProfileRequest
            {
                ProfileName = "Guest",
                Parameters = new Dictionary<string, string> { { "CanView", "true" } }
            };

            await _service.AddProfileAsync(request);

            var newProfile = _service.GetProfile("Guest");

            Assert.NotNull(newProfile);
            Assert.Equal(newProfile.ProfileName, request.ProfileName);
            Assert.Single(newProfile.Parameters);
        }

        [Fact]
        public async Task AddProfileAsync_ShouldThrowException_WhenValueIsNotBoolean()
        {
            var request = new AddProfileRequest { ProfileName = "Guest", Parameters = new Dictionary<string, string> { { "CanView", "notBoolean" } } };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddProfileAsync(request));

            Assert.Equal("Inputs are invalid", ex.Message);
        }

        [Fact]
        public async Task AddProfileAsync_ShouldThrowException_WhenProfileAlreadyExists()
        {
            var request = new AddProfileRequest { ProfileName = "Admin", Parameters = new Dictionary<string, string> { { "CanView", "true" } } };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddProfileAsync(request));

            Assert.Equal("Profile 'Admin' already exists.", ex.Message);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldNotUpdateProfile_WhenProfileDoesNotExist()
        {
            var profileName = "Guest";
            var request = new UpdateProfileRequest { Parameters = new Dictionary<string, string> { { "CanView", "true" } } };

            await _service.UpdateProfileAsync(profileName, request);

            var newProfile = _service.GetProfile(profileName);

            Assert.Null(newProfile);

            _mockLogger.Verify(
                logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Profile '{profileName}' not found.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
            );
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldThrowException_WhenValueIsNotBoolean()
        {
            var profileName = "Admin";

            var request = new UpdateProfileRequest { Parameters = new Dictionary<string, string> { { "CanView", "notBoolean" } } };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateProfileAsync(profileName, request));

            Assert.Equal("Inputs are invalid", ex.Message);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldUpdateProfile_WhenProfileExists()
        {
            var profileName = "Admin";
            var request = new UpdateProfileRequest { Parameters = new Dictionary<string, string> { { "CanView", "false" } } };

            await _service.UpdateProfileAsync(profileName, request);

            var updatedProfile = _service.GetProfile(profileName);

            Assert.NotNull(updatedProfile);
            Assert.Equal(updatedProfile.ProfileName, profileName);
            Assert.Equal("false", updatedProfile.Parameters["CanView"]);
        }

        [Fact]
        public async Task DeleteProfileAsync_ShouldNotDeleteProfile_WhenProfileDoesNotExist()
        {
            var profileName = "Guest";

            var allProfiles = _service.GetAllProfiles();

            Assert.Equal(2, allProfiles.Count());
            
            await _service.DeleteProfileAsync(profileName);

            var afterDeleteAllProfiles = _service.GetAllProfiles();

            Assert.Equal(2, afterDeleteAllProfiles.Count());

            _mockLogger.Verify(
               logger => logger.Log(
                   LogLevel.Warning,
               It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Profile '{profileName}' not found.")),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()
               ),
           Times.Once
           );
        }

        [Fact]
        public async Task DeleteProfileAsync_ShouldDeleteProfile_WhenProfileExists()
        {
            var profileName = "Admin";

            await _service.DeleteProfileAsync(profileName);

            var deleteProfile = _service.GetProfile(profileName);

            Assert.Null(deleteProfile);

            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Profile '{profileName}' deleted successfully.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
            );
        }

        [Fact]
        public async Task ToggleBoolConfiguration_ShouldToggleValue_WhenPermissionExists()
        {
            string profileName = "Admin";
            string permission = "CanEdit";

            var before = await _service.ValidateProfileAsync(profileName, permission);

            await _service.ToogleBoolConfiguration(profileName, permission, CancellationToken.None);

            var after = await _service.ValidateProfileAsync(profileName, permission);

            Assert.NotEqual(before, after);
        }

        [Fact]
        public async Task ToggleBoolConfiguration_ShouldLogError_WhenPermissionDoesNotExist()
        {
            string profileName = "Admin";
            string permission = "PermissionNotExisting";

            await _service.ToogleBoolConfiguration(profileName, permission, CancellationToken.None);

            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"No profile parameter with name {permission}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
            );
        }

        [Fact]
        public async Task ToggleBoolConfiguration_ShouldLogError_WhenProfileDoesNotExist()
        {
            string profileName = "ProfileDoesNotExist";
            string permission = "CanEdit";

            await _service.ToogleBoolConfiguration(profileName, permission, CancellationToken.None);

            _mockLogger.Verify(
                logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"No profile with name {profileName}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
            );
        }

        [Fact]
        public async Task ValidProfileAsync_ShouldReturnInformation()
        {
            var profileName = "Admin";
            var actionName = "CanEdit";

            var result = await _service.ValidateProfileAsync(profileName, actionName);

            Assert.True(result);
        }

        [Fact]
        public async Task ValidProfileAsync_ShouldReturnFalse_AndLog_WhenActionDoesntExists()
        {
            var profileName = "Admin";
            var actionName = "nonExisting";

            var result = await _service.ValidateProfileAsync(profileName, actionName);

            Assert.Null(result);

            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Validation failed for profile '{profileName}' and action '{actionName}'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
            );
        }
    }
}
