namespace AuxiAPI.src.Security;

public class DevTokenOptions
{
    public bool Enabled { get; set; } = false;
    public bool PrintTokenOnStartup { get; set; } = false;
    public bool ExposeEndpoint { get; set; } = false;
    public bool InjectTokenInRequests { get; set; } = false;
    public string EndpointPath { get; set; } = "/dev/token";
    public int RefreshBeforeExpirationSeconds { get; set; } = 300;
}