namespace ProfileAndPermissions.Domain.Classes
{
    public class ProfileConfiguration
    {
        public Dictionary<string, ProfileParameter> Parameter { get; set; } = [];
    }

    public class ProfileParameter
    {
        public string ProfileName { get; set; } = "";

        public Dictionary<string, string> Parameters { get; set; } = [];
    }
}
