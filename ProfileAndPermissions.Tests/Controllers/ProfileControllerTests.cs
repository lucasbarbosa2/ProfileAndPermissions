using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProfileAndPermissions.Domain.Interfaces;
using ProfileAndPermissions.Domain.Request;
using ProfileAndPermissions.Domain.Response;
using ProfileAndPermissions.UI.Controllers;

namespace ProfileAndPermissions.Tests.Controllers
{
    public class ProfileControllerTests
    {
        private readonly Mock<IProfileConfigurationService> _mockService;
        private readonly Mock<ILogger<ProfileController>> _mockLogger;
        private readonly ProfileController _controller;

        public ProfileControllerTests()
        {
            _mockLogger = new Mock<ILogger<ProfileController>>();
            _mockService = new Mock<IProfileConfigurationService>();
            _controller = new ProfileController(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetProfiles_ShouldReturnOk_WhenProfilesExist()
        {
            var response = new Dictionary<string, ProfileResponse>
            {
                {"Admin", new() { ProfileName = "Admin", Parameters = new() { { "CanEdit", "true" } } } }
            };

            _mockService.Setup(s => s.GetAllProfiles()).Returns(response);

            var result = _controller.GetAllProfiles();

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(200, objResult.StatusCode);
            Assert.Equal("Admin", response["Admin"].ProfileName);
            Assert.Single(response["Admin"].Parameters);
        }

        [Fact]
        public void GetProfiles_ShouldLogError_WhenExceptionOccurs()
        {
            var expectedException = new Exception("Error");

            _mockService.Setup(s => s.GetAllProfiles()).Throws(expectedException);

            var result = _controller.GetAllProfiles();

            var objectResult = result as ObjectResult;

            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);

            _mockLogger.Verify(
                logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Error when retrieving profiles data. Ex: {expectedException.Message}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
            );
        }

        [Fact]
        public void GetProfile_ShouldReturnOk_WhenProfileExist()
        {
            var profileName = "Admin";
            var response = new ProfileResponse
            {
               ProfileName = profileName, Parameters = new() { { "CanEdit", "true" } }
            };

            _mockService.Setup(s => s.GetProfile(profileName)).Returns(response);

            var result = _controller.GetProfile(profileName);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(200, objResult.StatusCode);
        }

        [Fact]
        public void GetProfile_ShouldReturnBadRequest_WhenInputIsInvalid()
        {
            var profileName = "";

            var result = _controller.GetProfile(profileName);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(400, objResult.StatusCode);
        }

        [Fact]
        public void GetProfile_ShouldReturnBadRequest_WhenProfileDoesntExist()
        {
            var profileName = "profileNonExisting";

            _mockService.Setup(s => s.GetProfile(profileName)).Returns<ProfileResponse?>(default);

            var result = _controller.GetProfile(profileName);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(404, objResult.StatusCode);
        }

        [Fact]
        public void GetProfile_ShouldLogError_WhenExceptionOccurs()
        {
            var profileName = "Admin";
            var expectedException = new Exception("Error");

            _mockService.Setup(s => s.GetProfile(profileName)).Throws(expectedException);

            var result = _controller.GetProfile(profileName);

            var objectResult = result as ObjectResult;

            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);
            _mockLogger.Verify(
                logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Error when getting profile {profileName} data. Ex: {expectedException.Message}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
            );
        }

        [Fact]
        public async Task AddProfile_ShouldReturnOk_WhenDataIsValid()
        {
            string profileName = "Admin";

            var request = new AddProfileRequest { ProfileName = profileName, Parameters = new() { { "CanView" , "true" } } };

            _mockService.Setup(s => s.AddProfileAsync(request));

            var result = await _controller.AddProfile(request);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(200, objResult.StatusCode);
        }

        [Fact]
        public async Task AddProfile_ShouldReturnBadRequest_WhenDataIsNull()
        {
            var result = await _controller.AddProfile(null);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(400, objResult.StatusCode);
        }

        [Fact]
        public async Task AddProfile_ShouldReturnBadRequest_WhenProfileIsEmpty()
        {
            string profileName = "";

            var request = new AddProfileRequest { ProfileName = profileName, Parameters = new() { { "CanView", "true" } } };

            var result = await _controller.AddProfile(request);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(400, objResult.StatusCode);
        }


        [Fact]
        public async Task AddProfile_ShouldReturnBadRequest_WhenProfileIsNull()
        {
            var request = new AddProfileRequest { ProfileName = null, Parameters = new() { { "CanView", "true" } } };

            var result = await _controller.AddProfile(request);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(400, objResult.StatusCode);
        }

        [Fact]
        public async Task AddProfile_ShouldReturnInternalError_WhenExceptionOccurs()
        {
            string profileName = "Admin";

            var request = new AddProfileRequest { ProfileName = profileName, Parameters = new() { { "CanView", "true" } } };

            var expectedException = new Exception("Error");

            _mockService.Setup(s => s.AddProfileAsync(request)).Throws(expectedException);

            var result = await _controller.AddProfile(request);

            var objectResult = result as ObjectResult;

            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);

            _mockLogger.Verify(
                logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Error when adding profile {profileName} data. Ex: {expectedException.Message}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
            );
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnOk_WhenDataIsValid()
        {
            string profileName = "Admin";
            var parameters = new Dictionary<string, string>() { { "CanView", "true" } };
            var request = new UpdateProfileRequest { Parameters = parameters };
            var response = new ProfileResponse { ProfileName = profileName, Parameters = parameters };

            _mockService.Setup(s => s.GetProfile(profileName)).Returns(response);
            _mockService.Setup(s => s.UpdateProfileAsync(profileName, request));

            var result = await _controller.UpdateProfile(profileName, request);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(200, objResult.StatusCode);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnNotFound_WhenProfileDoesNotExist()
        {
            string profileName = "Admin";
            var parameters = new Dictionary<string, string>() { { "CanView", "true" } };
            var request = new UpdateProfileRequest { Parameters = parameters };
            var response = new ProfileResponse { ProfileName = profileName, Parameters = parameters };

            _mockService.Setup(s => s.GetProfile(profileName)).Returns<ProfileResponse>(null);
            _mockService.Setup(s => s.UpdateProfileAsync(profileName, request));

            var result = await _controller.UpdateProfile(profileName, request);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(404, objResult.StatusCode);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnBadRequest_WhenDataIsNull()
        {
            var result = await _controller.UpdateProfile("Admin", null);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(400, objResult.StatusCode);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnBadRequest_WhenProfileIsEmpty()
        {
            var request = new UpdateProfileRequest { Parameters = new() { { "CanView", "true" } } };

            var result = await _controller.UpdateProfile("", request);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(400, objResult.StatusCode);
        }


        [Fact]
        public async Task UpdateProfile_ShouldReturnBadRequest_WhenProfileIsNull()
        {
            var request = new UpdateProfileRequest { Parameters = new() { { "CanView", "true" } } };

            var result = await _controller.UpdateProfile(null, request);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(400, objResult.StatusCode);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnInternalError_WhenExceptionOccurs()
        {
            string profileName = "Admin";

            var request = new UpdateProfileRequest { Parameters = new() { { "CanView", "true" } } };

            var expectedException = new Exception("Error");

            _mockService.Setup(s => s.GetProfile(profileName)).Throws(expectedException);

            var result = await _controller.UpdateProfile(profileName, request);

            var objectResult = result as ObjectResult;

            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);

            _mockLogger.Verify(
                logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Error when updating profile {profileName} data. Ex: {expectedException.Message}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
            );
        }

        [Fact]
        public async Task DeleteProfile_ShouldReturnOk_WhenDeleteOccurs()
        {
            string profileName = "Admin";
            var parameters = new Dictionary<string, string>() { { "CanView", "true" } };
            var response = new ProfileResponse { ProfileName = profileName, Parameters = parameters };

            _mockService.Setup(s => s.GetProfile(profileName)).Returns(response);
            _mockService.Setup(s => s.DeleteProfileAsync(profileName)).ReturnsAsync(true);


            var result = await _controller.DeleteProfile(profileName);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(200, objResult.StatusCode);
        }

        [Fact]
        public async Task DeleteProfile_ShouldReturnNotFound_WhenProfileDoesNotExist()
        {
            string profileName = "NonExisting";

            _mockService.Setup(s => s.GetProfile(profileName)).Returns<ProfileResponse>(null);

            var result = await _controller.DeleteProfile(profileName);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(404, objResult.StatusCode);
        }

        [Fact]
        public async Task DeleteProfile_ShouldReturnBadRequest_WhenProfileIsEmpty()
        {
            string profileName = "";

            _mockService.Setup(s => s.GetProfile(profileName)).Returns<ProfileResponse>(null);

            var result = await _controller.DeleteProfile(profileName);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(400, objResult.StatusCode);
        }


        [Fact]
        public async Task DeleteProfile_ShouldReturnBadRequest_WhenProfileIsNull()
        {
            string profileName = null;

            _mockService.Setup(s => s.GetProfile(profileName)).Returns<ProfileResponse>(null);

            var result = await _controller.DeleteProfile(profileName);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(400, objResult.StatusCode);
        }

        [Fact]
        public async Task DeleteProfile_ShouldReturnInternalError_WhenExceptionOccurs()
        {
            string profileName = "Admin";

            var expectedException = new Exception("Error");

            _mockService.Setup(s => s.GetProfile(profileName)).Throws(expectedException);

            var result = await _controller.DeleteProfile(profileName);

            var objectResult = result as ObjectResult;

            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);

            _mockLogger.Verify(
                logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Error when deleting profile {profileName}. Ex: {expectedException.Message}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
            );
        }

        [Fact]
        public async Task ValidateProfile_ShouldReturnOk_WhenactionIsValid()
        {
            string profileName = "Admin";
            string action = "CanEdit";
            bool expectedValidation = true;

            _mockService.Setup(s => s.ValidateProfileAsync(profileName, action))
                .ReturnsAsync(expectedValidation);

            var result = await _controller.ValidateProfile(profileName, action);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(200, objResult.StatusCode);
            var isValid = objResult?.Value?.GetType()?.GetProperty("isValid")?.GetValue(objResult?.Value);
            Assert.NotNull(isValid);
            Assert.Equal(expectedValidation,(bool)isValid);
        }

        [Fact]
        public async Task ValidateProfile_ShouldReturnBadRequest_WhenActionDoesNotExist()
        {
            string profileName = "Admin";
            string action = "nonExistingAction";

            _mockService.Setup(s => s.ValidateProfileAsync(profileName, action)).ReturnsAsync((bool?)null);

            var result = await _controller.ValidateProfile(profileName, action);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(400, objResult.StatusCode);
        }

        [Fact]
        public async Task ValidateProfile_ShouldReturnBadRequest_WhenProfileIsNull()
        {
            string profileName = null;
            string action = "CanEdit";

            var result = await _controller.ValidateProfile(profileName, action);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(400, objResult.StatusCode);
        }

        [Fact]
        public async Task ValidateProfile_ShouldReturnBadRequest_WhenProfileIsEmpty()
        {
            string profileName = "";
            string action = "CanEdit";

            var result = await _controller.ValidateProfile(profileName, action);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(400, objResult.StatusCode);
        }

        [Fact]
        public async Task ValidateProfile_ShouldReturnBadRequest_WhenActionIsNull()
        {
            string profileName = "Admin";
            string action = null;

            var result = await _controller.ValidateProfile(profileName, action);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(400, objResult.StatusCode);
        }

        [Fact]
        public async Task ValidateProfile_ShouldReturnBadRequest_WhenActionIsEmpty()
        {
            string profileName = "Admin";
            string action = "";

            var result = await _controller.ValidateProfile(profileName, action);

            var objResult = result as ObjectResult;
            Assert.NotNull(objResult);
            Assert.Equal(400, objResult.StatusCode);
        }

        [Fact]
        public async Task ValidateProfile_ShouldReturnInternalError_WhenExceptionOccurs()
        {
            string profileName = "Admin";
            string action = "CanEdit";

            var expectedException = new Exception("Error");

            _mockService.Setup(s => s.ValidateProfileAsync(profileName, action)).Throws(expectedException);

            var result = await _controller.ValidateProfile(profileName, action);

            var objectResult = result as ObjectResult;

            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);

            _mockLogger.Verify(
                logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Error when validating profile {profileName} action {action}. Ex: {expectedException.Message}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
            );
        }
    }
}
