using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AuxiAPI.src.Security;

public class DevTokenStartupService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly IDevTokenService _devTokenService;
    private readonly ILogger<DevTokenStartupService> _logger;

    public DevTokenStartupService(
        IConfiguration configuration,
        IDevTokenService devTokenService,
        ILogger<DevTokenStartupService> logger)
    {
        _configuration = configuration;
        _devTokenService = devTokenService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var enabled = _configuration.GetValue<bool>("DevToken:Enabled");

        if (!enabled)
        {
            _logger.LogInformation("token automático está desabilitado.");
            return;
        }

        _logger.LogInformation("inicializando token automático...");

        await _devTokenService.CarregarTokenAsync(cancellationToken);

        var printTokenOnStartup = _configuration.GetValue<bool>("DevToken:PrintTokenOnStartup");

        if (printTokenOnStartup)
        {
            ImprimirTokenNoTerminal();
        }

        _logger.LogInformation("Inicialização do token automático concluída.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void ImprimirTokenNoTerminal()
{
    if (!_devTokenService.PossuiToken)
    {
        _logger.LogWarning("Token automático habilitado, mas nenhum token foi carregado em memória.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("============================================================");
    Console.WriteLine(" TOKEN AUTOMÁTICO");
    Console.WriteLine("============================================================");
    Console.WriteLine($"Expira em UTC: {_devTokenService.ExpiraEmUtc}");
    Console.WriteLine();
    Console.WriteLine("Use no header Authorization:");
    Console.WriteLine(_devTokenService.ObterAuthorizationHeader());
    Console.WriteLine();
    Console.WriteLine("============================================================");
    Console.WriteLine();
}
}