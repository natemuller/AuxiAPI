namespace AuxiAPI.src.Security;

public interface IDevTokenService
{
    bool PossuiToken { get; }
    string AccessToken { get; }
    string TokenType { get; }
    DateTime? ExpiraEmUtc { get; }
    Task CarregarTokenAsync(CancellationToken cancellationToken = default);
    string ObterAuthorizationHeader();
}