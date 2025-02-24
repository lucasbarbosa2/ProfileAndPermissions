namespace ProfileAndPermissions.Domain.Request
{
    public class ProfileRequestBase
    {
        public Dictionary<string, string> Parameters { get; set; } = new();
    }

}
