using AuxiAPI.src.Middlewares;
using AuxiAPI.src.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AuxiAPI.Tests.MiddlewaresTest;

public class DevTokenInjectionMiddlewareTest
{
    [Fact]
    public async Task InvokeAsync_DeveInjetarAuthorization_QuandoRotaApiNaoPossuiHeaderETokenExiste()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/condominios";
        context.Request.Method = HttpMethods.Get;

        var tokenService = new FakeDevTokenService(
            possuiToken: true,
            authorizationHeader: "Bearer token_teste"
        );

        var options = CriarOptions(
            enabled: true,
            injectTokenInRequests: true
        );

        var environment = new FakeWebHostEnvironment
        {
            EnvironmentName = Environments.Development
        };

        var middleware = CriarMiddleware(
            context =>
            {
                Assert.Equal("Bearer token_teste", context.Request.Headers.Authorization.ToString());
                return Task.CompletedTask;
            },
            tokenService,
            options,
            environment
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("Bearer token_teste", context.Request.Headers.Authorization.ToString());
    }

    [Fact]
    public async Task InvokeAsync_NaoDeveSobrescreverAuthorization_QuandoHeaderJaExiste()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/condominios";
        context.Request.Method = HttpMethods.Get;
        context.Request.Headers.Authorization = "Bearer token_invalido";

        var tokenService = new FakeDevTokenService(
            possuiToken: true,
            authorizationHeader: "Bearer token_teste"
        );

        var options = CriarOptions(
            enabled: true,
            injectTokenInRequests: true
        );

        var environment = new FakeWebHostEnvironment
        {
            EnvironmentName = Environments.Development
        };

        var middleware = CriarMiddleware(
            _ => Task.CompletedTask,
            tokenService,
            options,
            environment
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("Bearer token_invalido", context.Request.Headers.Authorization.ToString());
    }

    [Fact]
    public async Task InvokeAsync_NaoDeveInjetarAuthorization_QuandoRotaNaoForApi()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger";
        context.Request.Method = HttpMethods.Get;

        var tokenService = new FakeDevTokenService(
            possuiToken: true,
            authorizationHeader: "Bearer token_teste"
        );

        var options = CriarOptions(
            enabled: true,
            injectTokenInRequests: true
        );

        var environment = new FakeWebHostEnvironment
        {
            EnvironmentName = Environments.Development
        };

        var middleware = CriarMiddleware(
            _ => Task.CompletedTask,
            tokenService,
            options,
            environment
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(context.Request.Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public async Task InvokeAsync_NaoDeveInjetarAuthorization_QuandoInjecaoEstiverDesabilitada()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/condominios";
        context.Request.Method = HttpMethods.Get;

        var tokenService = new FakeDevTokenService(
            possuiToken: true,
            authorizationHeader: "Bearer token_teste"
        );

        var options = CriarOptions(
            enabled: true,
            injectTokenInRequests: false
        );

        var environment = new FakeWebHostEnvironment
        {
            EnvironmentName = Environments.Development
        };

        var middleware = CriarMiddleware(
            _ => Task.CompletedTask,
            tokenService,
            options,
            environment
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(context.Request.Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public async Task InvokeAsync_NaoDeveInjetarAuthorization_QuandoNaoPossuiToken()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/condominios";
        context.Request.Method = HttpMethods.Get;

        var tokenService = new FakeDevTokenService(
            possuiToken: false,
            authorizationHeader: string.Empty
        );

        var options = CriarOptions(
            enabled: true,
            injectTokenInRequests: true
        );

        var environment = new FakeWebHostEnvironment
        {
            EnvironmentName = Environments.Development
        };

        var middleware = CriarMiddleware(
            _ => Task.CompletedTask,
            tokenService,
            options,
            environment
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(context.Request.Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public async Task InvokeAsync_NaoDeveInjetarAuthorization_QuandoAmbienteNaoForDevelopment()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/condominios";
        context.Request.Method = HttpMethods.Get;

        var tokenService = new FakeDevTokenService(
            possuiToken: true,
            authorizationHeader: "Bearer token_teste"
        );

        var options = CriarOptions(
            enabled: true,
            injectTokenInRequests: true
        );

        var environment = new FakeWebHostEnvironment
        {
            EnvironmentName = Environments.Production
        };

        var middleware = CriarMiddleware(
            _ => Task.CompletedTask,
            tokenService,
            options,
            environment
        );

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(context.Request.Headers.ContainsKey("Authorization"));
    }

    private static DevTokenInjectionMiddleware CriarMiddleware(
        RequestDelegate next,
        IDevTokenService tokenService,
        IOptions<DevTokenOptions> options,
        IWebHostEnvironment environment)
    {
        return new DevTokenInjectionMiddleware(
            next,
            tokenService,
            options,
            environment,
            NullLogger<DevTokenInjectionMiddleware>.Instance
        );
    }

    private static IOptions<DevTokenOptions> CriarOptions(
        bool enabled,
        bool injectTokenInRequests)
    {
        return Options.Create(new DevTokenOptions
        {
            Enabled = enabled,
            InjectTokenInRequests = injectTokenInRequests,
            PrintTokenOnStartup = false,
            ExposeEndpoint = false,
            EndpointPath = "/dev/token",
            RefreshBeforeExpirationSeconds = 300
        });
    }

    private sealed class FakeDevTokenService : IDevTokenService
    {
        private readonly string _authorizationHeader;

        public FakeDevTokenService(bool possuiToken, string authorizationHeader)
        {
            PossuiToken = possuiToken;
            _authorizationHeader = authorizationHeader;
        }

        public bool PossuiToken { get; }

        public string AccessToken => "token_teste";

        public string TokenType => "Bearer";

        public DateTime? ExpiraEmUtc => DateTime.UtcNow.AddMinutes(30);

        public Task CarregarTokenAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public string ObterAuthorizationHeader()
        {
            return _authorizationHeader;
        }
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "AuxiAPI.Tests";

        public string WebRootPath { get; set; } = string.Empty;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}