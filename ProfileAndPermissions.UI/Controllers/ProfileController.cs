using Microsoft.AspNetCore.Mvc;
using ProfileAndPermissions.Domain.Interfaces;
using ProfileAndPermissions.Domain.Request;
using ProfileAndPermissions.Domain.Response;

namespace ProfileAndPermissions.UI.Controllers
{
    /// <summary>
    /// This controller is used manipulate Profile and Actions data
    /// </summary>
    [ApiController]
    [Route("api/profiles")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileConfigurationService _profileConfigurationService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IProfileConfigurationService profileConfigurationService, ILogger<ProfileController> logger)
        {
            _profileConfigurationService = profileConfigurationService;
            _logger = logger;
        }

        /// <summary>
        /// Returns all profiles and their parameters.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(Dictionary<string, ProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult  GetAllProfiles()
        {
            try
            {
                var profiles = _profileConfigurationService.GetAllProfiles();
                return Ok(profiles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when retrieving profiles data. Ex: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns profile data
        /// </summary>
        [HttpGet("{profileName}")]
        [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetProfile(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                return BadRequest("Profile name is required.");

            try
            {
                var profile = _profileConfigurationService.GetProfile(profileName);
                if (profile == null)
                    return NotFound($"Profile '{profileName}' not found.");

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when getting profile {profileName} data. Ex: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a new profile.
        /// </summary>
        /// <remarks>
        /// **Example Request:**
        ///
        ///     POST /api/profiles
        ///     {
        ///         "profileName": "Guest",
        ///         "parameters": {
        ///             "CanView": "true",
        ///             "CanEdit": "false"
        ///         }
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProfile([FromBody] AddProfileRequest profile)
        {
            if (profile == null)
                return BadRequest("Profile data is required.");
            if (string.IsNullOrWhiteSpace(profile.ProfileName))
                return BadRequest("Profile name is required.");

            try
            {
                await _profileConfigurationService.AddProfileAsync(profile);
                return Ok("Profile added succesfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when adding profile {profile.ProfileName} data. Ex: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates parameters of an existing profile.
        /// </summary>
        /// <remarks>
        /// **Example Request:**
        ///
        ///     PUT /api/profiles
        ///     {
        ///         "parameters": {
        ///             "CanView": "true",
        ///             "CanEdit": "false"
        ///         }
        ///     }
        ///     
        /// </remarks>
        [HttpPut("{profileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfile(string profileName, [FromBody] UpdateProfileRequest profile)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                return BadRequest("Profile name is required.");
            if (profile == null)
                return BadRequest("Profile data is required.");

            try
            {
                var existingProfile = _profileConfigurationService.GetProfile(profileName);
                if (existingProfile == null)
                    return NotFound($"Profile '{profileName}' not found.");

                await _profileConfigurationService.UpdateProfileAsync(profileName, profile);
                return Ok("Profile updated succesfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when updating profile {profileName} data. Ex: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a profile.
        /// </summary>
        [HttpDelete("{profileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProfile(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                return BadRequest("Profile name is required.");

            try
            {
                var existingProfile = _profileConfigurationService.GetProfile(profileName);
                if (existingProfile == null)
                    return NotFound($"Profile '{profileName}' not found.");

                await _profileConfigurationService.DeleteProfileAsync(profileName);
                return Ok("Profile deleted succesfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when deleting profile {profileName}. Ex: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates if a profile has permission for a specific action.
        /// </summary>
        [HttpGet("{profileName}/validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateProfile(string profileName, [FromQuery] string action)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                return BadRequest("Profile name is required.");
            if (string.IsNullOrWhiteSpace(action))
                return BadRequest("Action is required.");

            try
            {
                bool? isValid = await _profileConfigurationService.ValidateProfileAsync(profileName, action);

                if(isValid.HasValue)
                    return Ok(new { profileName, action, isValid });
                else
                    return BadRequest($"Action: {action} does not exist for profile: {profileName}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when validating profile {profileName} action {action}. Ex: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
