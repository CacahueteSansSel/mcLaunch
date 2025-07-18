namespace mcLaunch.Launchsite.Models.Auth;

public class XblXstsRequest
{
    public XblXstsRequest(string token)
    {
        TokenType = "JWT";
        RelyingParty = "rp://api.minecraftservices.com/";
        Properties = new ModelProperties
        {
            SandboxId = "RETAIL",
            UserTokens = new[] { token }
        };
    }

    public string TokenType { get; set; }
    public string RelyingParty { get; set; }
    public ModelProperties Properties { get; set; }

    public class ModelProperties
    {
        public string SandboxId { get; set; }
        public string[] UserTokens { get; set; }
    }
}