namespace ProfileAndPermissions.Domain.Response
{
    public class ProfileResponse
    {
        public string ProfileName { get; set; } = "";

        public Dictionary<string, string> Parameters { get; set; } = [];
    }
}
