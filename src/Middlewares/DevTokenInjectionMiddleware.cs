using AuxiAPI.src.Security;
using Microsoft.Extensions.Options;

namespace AuxiAPI.src.Middlewares;

public class DevTokenInjectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDevTokenService _devTokenService;
    private readonly DevTokenOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DevTokenInjectionMiddleware> _logger;

    public DevTokenInjectionMiddleware(
        RequestDelegate next,
        IDevTokenService devTokenService,
        IOptions<DevTokenOptions> options,
        IWebHostEnvironment environment,
        ILogger<DevTokenInjectionMiddleware> logger)
    {
        _next = next;
        _devTokenService = devTokenService;
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (DeveInjetarToken(context))
        {
            context.Request.Headers.Authorization = _devTokenService.ObterAuthorizationHeader();

            _logger.LogDebug(
                "Token automático de desenvolvimento injetado na requisição {Method} {Path}.",
                context.Request.Method,
                context.Request.Path
            );
        }

        await _next(context);
    }

    private bool DeveInjetarToken(HttpContext context)
    {
        if (!_environment.IsDevelopment())
            return false;

        if (!_options.Enabled)
            return false;

        if (!_options.InjectTokenInRequests)
            return false;

        if (!_devTokenService.PossuiToken)
            return false;

        if (!EhRotaDaApi(context))
            return false;

        if (JaPossuiAuthorization(context))
            return false;

        return true;
    }

    private static bool EhRotaDaApi(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api");
    }

    private static bool JaPossuiAuthorization(HttpContext context)
    {
        return context.Request.Headers.ContainsKey("Authorization");
    }
}