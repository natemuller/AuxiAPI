using Testcontainers.PostgreSql;
using Xunit;

namespace AuxiAPI.Tests.TestInfrastructure;

public sealed class PostgresTestFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public string ConnectionString { get; private set; } = string.Empty;
    public string? SkipReason { get; private set; }
    public bool IsAvailable => string.IsNullOrWhiteSpace(SkipReason);

    public async Task InitializeAsync()
    {
        try
        {
            _container = new PostgreSqlBuilder()
                .WithImage("postgres:16.4")
                .WithDatabase("auxiapi")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();
        }
        catch (Exception ex)
        {
            SkipReason = $"PostgreSQL Testcontainers não está disponível: {ex.Message}";

            throw new InvalidOperationException(
                "Não foi possível iniciar o PostgreSQL usado pelos testes de integração. " +
                "Verifique se o Docker está instalado e em execução.",
                ex);
        }
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}

public static class PostgresCollectionNames
{
    public const string PostgresCollection = "PostgresCollection";
}

[CollectionDefinition(PostgresCollectionNames.PostgresCollection)]
public sealed class PostgresCollectionDefinition : ICollectionFixture<PostgresTestFixture>
{
}
