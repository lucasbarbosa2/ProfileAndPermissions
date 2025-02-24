namespace ProfileAndPermissions.Domain.Request
{
    public class AddProfileRequest : ProfileRequestBase
    {
        public string ProfileName { get; set; } = string.Empty;
    }
}
