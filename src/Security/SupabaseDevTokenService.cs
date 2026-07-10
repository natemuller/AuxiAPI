using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuxiAPI.src.Security;

public class SupabaseDevTokenService : IDevTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SupabaseDevTokenService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private string _accessToken = string.Empty;
    private string _tokenType = "Bearer";
    private DateTime? _expiraEmUtc;

    public bool PossuiToken => !string.IsNullOrWhiteSpace(_accessToken);

    public string AccessToken => _accessToken;

    public string TokenType => _tokenType;

    public DateTime? ExpiraEmUtc => _expiraEmUtc;

    public SupabaseDevTokenService(
    IConfiguration configuration,
    ILogger<SupabaseDevTokenService> logger,
    IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task CarregarTokenAsync(CancellationToken cancellationToken = default)
    {
        var supabaseUrl = ObterConfiguracaoObrigatoria("Supabase:Url").TrimEnd('/');
        var anonKey = ObterConfiguracaoObrigatoria("Supabase:AnonKey");
        var email = ObterConfiguracaoObrigatoria("Supabase:Auth:Email");
        var password = ObterConfiguracaoObrigatoria("Supabase:Auth:Password");

        var client = _httpClientFactory.CreateClient();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{supabaseUrl}/auth/v1/token?grant_type=password"
        );

        request.Headers.Add("apikey", anonKey);

        request.Content = JsonContent.Create(new
        {
            email,
            password
        });

        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Erro ao autenticar no Supabase Auth. Status {(int)response.StatusCode}. Resposta: {erro}"
            );
        }

        var authResponse = await response.Content.ReadFromJsonAsync<SupabaseAuthResponse>(
            cancellationToken: cancellationToken
        );

        if (authResponse is null || string.IsNullOrWhiteSpace(authResponse.AccessToken))
            throw new InvalidOperationException("Supabase Auth não retornou um access_token válido.");

        _accessToken = authResponse.AccessToken;
        _tokenType = string.IsNullOrWhiteSpace(authResponse.TokenType)
            ? "Bearer"
            : NormalizarTokenType(authResponse.TokenType);

        _expiraEmUtc = CalcularExpiracaoUtc(authResponse);

        _logger.LogInformation("Token automático de desenvolvimento gerado com sucesso.");
        _logger.LogInformation("Token expira em UTC: {ExpiraEmUtc}", _expiraEmUtc);
    }

    public string ObterAuthorizationHeader()
    {
        if (!PossuiToken)
            throw new InvalidOperationException("Nenhum token foi carregado em memória.");

        return $"{TokenType} {AccessToken}";
    }

    private string ObterConfiguracaoObrigatoria(string chave)
    {
        var valor = _configuration[chave];

        if (string.IsNullOrWhiteSpace(valor))
            throw new InvalidOperationException($"configuração nao encontrada: {chave}");

        return valor;
    }

    private static string NormalizarTokenType(string tokenType)
    {
        return tokenType.Equals("bearer", StringComparison.OrdinalIgnoreCase)
            ? "Bearer"
            : tokenType;
    }

    private static DateTime? CalcularExpiracaoUtc(SupabaseAuthResponse authResponse)
    {
        if (authResponse.ExpiresAt.HasValue)
        {
            return DateTimeOffset
                .FromUnixTimeSeconds(authResponse.ExpiresAt.Value)
                .UtcDateTime;
        }

        if (authResponse.ExpiresIn > 0)
        {
            return DateTime.UtcNow.AddSeconds(authResponse.ExpiresIn);
        }

        return null;
    }
}