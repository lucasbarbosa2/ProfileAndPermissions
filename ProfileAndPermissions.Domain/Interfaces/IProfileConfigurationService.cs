using ProfileAndPermissions.Domain.Classes;
using ProfileAndPermissions.Domain.Request;
using ProfileAndPermissions.Domain.Response;

namespace ProfileAndPermissions.Domain.Interfaces
{
    public interface IProfileConfigurationService
    {
        Task ToogleBoolConfiguration(string profile, string permission, CancellationToken cancellationToken);
        Dictionary<string, ProfileResponse> GetAllProfiles();
        ProfileResponse? GetProfile(string profileName);
        Task AddProfileAsync(AddProfileRequest request);
        Task<bool> UpdateProfileAsync(string profileName, UpdateProfileRequest request);
        Task<bool> DeleteProfileAsync(string profileName);
        Task<bool?> ValidateProfileAsync(string profileName, string action);
    }
}
