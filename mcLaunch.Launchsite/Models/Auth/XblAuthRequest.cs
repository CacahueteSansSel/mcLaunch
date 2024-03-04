namespace mcLaunch.Launchsite.Models.Auth;

public class XblAuthRequest
{
    public XblAuthRequest(string token)
    {
        TokenType = "JWT";
        RelyingParty = "http://auth.xboxlive.com";
        Properties = new ModelProperties
        {
            AuthMethod = "RPS",
            SiteName = "user.auth.xboxlive.com",
            RpsTicket = $"d={token}"
        };
    }

    public string TokenType { get; set; }
    public string RelyingParty { get; set; }
    public ModelProperties Properties { get; set; }

    public class ModelProperties
    {
        public string AuthMethod { get; set; }
        public string SiteName { get; set; }
        public string RpsTicket { get; set; }
    }
}