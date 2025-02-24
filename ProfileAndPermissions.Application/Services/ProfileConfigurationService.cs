using Microsoft.Extensions.Logging;
using ProfileAndPermissions.Domain.Classes;
using ProfileAndPermissions.Domain.Interfaces;
using ProfileAndPermissions.Domain.Request;
using ProfileAndPermissions.Domain.Response;

namespace ProfileAndPermissions.Application.Services
{
    /// <summary>
    /// This service manipulates profile permission data
    /// </summary>
    public class ProfileConfigurationService : IProfileConfigurationService
    {
        private readonly ProfileConfiguration _profileConfiguration = new();
        private readonly ILogger<ProfileConfigurationService> _logger;

        public ProfileConfigurationService(ILogger<ProfileConfigurationService> logger)
        {
            _logger = logger;
            _profileConfiguration.Parameter = new()
            {
                {
                    "Admin",
                    new()
                    {
                        ProfileName = "Admin",
                        Parameters =
                        {
                            { "CanEdit", "true"},
                            { "CanDelete", "true" }
                        }
                    }
                },
                {
                    "User",
                    new()
                    {
                        ProfileName = "User",
                        Parameters =
                        {
                            { "CanEdit", "false"},
                            { "CanDelete", "false" }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Toggles a specific profile/permission on and off
        /// </summary>
        /// <param name="profile">The profile name</param>
        /// <param name="permission">The action to be toggled</param>
        /// <param name="cancellationToken">The cancellation token</param>
        public async Task ToogleBoolConfiguration(string profile, string permission, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (_profileConfiguration.Parameter.TryGetValue(profile, out ProfileParameter? profileParameter))
                    {
                        if (profileParameter.Parameters.TryGetValue(permission, out string? value))
                        {
                            if (bool.TryParse(value, out bool result))
                            {
                                profileParameter.Parameters[permission] = $"{!result}".ToLower();
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"No profile parameter with name {permission}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"No profile with name {profile}");
                    }
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when toggling configuration {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Returns all profiles information
        /// </summary>
        /// <returns>A dictionary with the key being the profiles and value being the permissions</returns>
        public Dictionary<string, ProfileResponse> GetAllProfiles()
        {
            var result = new Dictionary<string, ProfileResponse>();

            foreach (var kvp in _profileConfiguration.Parameter)
            {
                var profileResponse = new ProfileResponse
                {
                    ProfileName = kvp.Value.ProfileName,
                    Parameters = new Dictionary<string, string>(kvp.Value.Parameters)
                };

                result.Add(kvp.Key, profileResponse);
            }

            return result;
        }

        /// <summary>
        /// Gets the profile information from a profileName
        /// </summary>
        /// <param name="profileName">The profile name</param>
        /// <returns></returns>
        public ProfileResponse? GetProfile(string profileName)
        {
            if (_profileConfiguration.Parameter.TryGetValue(profileName, out var profile))
            {
                return new ProfileResponse
                {
                    ProfileName = profile.ProfileName,
                    Parameters = new Dictionary<string, string>(profile.Parameters)
                };
            }

            return null;
        }


        
        /// <summary>
        /// Adds a new profile
        /// </summary>
        /// <param name="request">The request containing the profile's name and permissions</param>
        /// <exception cref="InvalidOperationException">Throws if profile already exists</exception>
        /// <exception cref="ArgumentException">Throws if permission values are not bool</exception>
        public async Task AddProfileAsync(AddProfileRequest request)
        {
            var newProfile = new ProfileParameter { ProfileName = request.ProfileName, Parameters = request.Parameters };

            if (_profileConfiguration.Parameter.ContainsKey(newProfile.ProfileName))
            {
                _logger.LogWarning($"Profile '{newProfile.ProfileName}' already exists.");
                throw new InvalidOperationException($"Profile '{newProfile.ProfileName}' already exists.");
            }

            if (!IsBool(newProfile.Parameters.Values))
                throw new ArgumentException($"Inputs are invalid");

            await Task.Run(() =>
            {
                _profileConfiguration.Parameter[newProfile.ProfileName] = newProfile;
                _logger.LogInformation($"Profile '{newProfile.ProfileName}' added successfully.");
            });
        }

        /// <summary>
        /// Updates a specific profile
        /// </summary>
        /// <param name="profileName">The profile name</param>
        /// <param name="request">The request containing the profile's name and permissions</param>
        /// <returns>True if success, false otherwise</returns>
        /// <exception cref="ArgumentException">Throws if permission values are not bool</exception>
        public async Task<bool> UpdateProfileAsync(string profileName, UpdateProfileRequest request)
        {
            var updatedProfile = new ProfileParameter { ProfileName = profileName, Parameters = request.Parameters };

            return await Task.Run(() =>
            {
                if (!_profileConfiguration.Parameter.ContainsKey(profileName))
                {
                    _logger.LogWarning($"Profile '{profileName}' not found.");
                    return false;
                }

                if(!IsBool(updatedProfile.Parameters.Values))
                    throw new ArgumentException($"Inputs are invalid");

                _profileConfiguration.Parameter[profileName] = updatedProfile;
                _logger.LogInformation($"Profile '{profileName}' updated successfully.");
                return true;
            });
        }

        /// <summary>
        /// Delets a profile
        /// </summary>
        /// <param name="profileName">The profile name</param>
        /// <returns>True if success, false otherwise</returns>
        public async Task<bool> DeleteProfileAsync(string profileName)
        {
            return await Task.Run(() =>
            {
                if (!_profileConfiguration.Parameter.Remove(profileName))
                {
                    _logger.LogWarning($"Profile '{profileName}' not found.");
                    return false;
                }

                _logger.LogInformation($"Profile '{profileName}' deleted successfully.");
                return true;
            });
        }

        /// <summary>
        /// Validates if a profile has permission for a specific action.
        /// </summary>
        /// <param name="profileName">The profile name</param>
        /// <param name="action">The action name</param>
        /// <returns>True if has access, false otherwise</returns>
        public async Task<bool?> ValidateProfileAsync(string profileName, string action)
        {
            return await Task.Run(() =>
            {
                if (_profileConfiguration.Parameter.TryGetValue(profileName, out var profile))
                {
                    if (profile.Parameters.TryGetValue(action, out var permission))
                    {
                        return bool.TryParse(permission, out var isAllowed) ? (bool?)isAllowed : null;
                    }
                }

                _logger.LogWarning($"Validation failed for profile '{profileName}' and action '{action}'.");
                return null;
            });
        }

        /// <summary>
        /// Method used to check if inputs are booleans
        /// </summary>
        /// <param name="values">The values</param>
        /// <returns>True if all inputs are valid, false otherwise</returns>
        private bool IsBool(IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                if (!bool.TryParse(value, out _))
                {
                    _logger.LogWarning($"{value} is not a valid input");
                    return false;
                }
            }

            return true;
        }
    }
}

