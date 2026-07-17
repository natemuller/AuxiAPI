using AuxiAPI.src.Contexts;
using AuxiAPI.src.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AuxiAPI.src.Security;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SupabaseConnection");
builder.Services.AddDbContext<CondominiosDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidarModelStateFilter>();
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddScoped<AuxiAPI.src.Repositories.ICondominioRepository, AuxiAPI.src.Repositories.CondominioRepository>();
builder.Services.AddScoped<AuxiAPI.src.Repositories.ICacheRepository, AuxiAPI.src.Repositories.CacheRepository>();

builder.Services.AddScoped<AuxiAPI.src.Repositories.IUnidadeRepository, AuxiAPI.src.Repositories.UnidadeRepository>();
builder.Services.AddScoped<AuxiAPI.src.Services.UnidadeService>();

builder.Services.AddScoped<AuxiAPI.src.Services.IDatabaseCacheService, AuxiAPI.src.Services.DatabaseCacheService>();
builder.Services.AddScoped<AuxiAPI.src.Services.CondominioService>();

var supabaseUrl = builder.Configuration["Supabase:Url"] ?? "https://gsmzasmtlllvzpjppfom.supabase.co";
var authorityUrl = $"{supabaseUrl.TrimEnd('/')}/auth/v1";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = authorityUrl;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = authorityUrl,
        ValidateAudience = true,
        ValidAudience = "authenticated",
        ValidateLifetime = true
    };
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AuxiAPI",
        Version = "teste"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insira o token JWT do Supabase. Exemplo: 'Bearer {token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

builder.Services.Configure<DevTokenOptions>(
    builder.Configuration.GetSection("DevToken"));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient();

    builder.Services.AddSingleton<IDevTokenService, SupabaseDevTokenService>();

    builder.Services.AddHostedService<DevTokenStartupService>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment() &&
    app.Configuration.GetValue<bool>("DevToken:ExposeEndpoint"))
{
    var endpointPath = app.Configuration["DevToken:EndpointPath"] ?? "/dev/token";

    app.MapGet(endpointPath, (IDevTokenService devTokenService) =>
    {
        if (!devTokenService.PossuiToken)
        {
            return Results.Problem(
                title: "Token automático não carregado",
                detail: "Nenhum token de desenvolvimento foi carregado em memória.",
                statusCode: StatusCodes.Status503ServiceUnavailable
            );
        }

        return Results.Ok(new
        {
            tokenType = devTokenService.TokenType,
            accessToken = devTokenService.AccessToken,
            authorizationHeader = devTokenService.ObterAuthorizationHeader(),
            expiresAtUtc = devTokenService.ExpiraEmUtc
        });
    })
    .AllowAnonymous();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AuxiAPI");
        options.RoutePrefix = "swagger";
    });
}

app.UseExceptionHandler();

app.UseStatusCodePages(async context =>
{
    context.HttpContext.Response.ContentType = "application/json";
    if (context.HttpContext.Response.StatusCode == 404)
    {
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            Sucesso = false,
            Status = 404,
            Mensagem = "a rota ou endpoint solicitado nao existe."
        });
    }
});

app.UseHttpsRedirection();

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<DevTokenInjectionMiddleware>();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
