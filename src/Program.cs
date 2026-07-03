using AuxiAPI.src.Contexts;
using AuxiAPI.src.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

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
builder.Services.AddScoped<AuxiAPI.src.Services.CondominioService>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AuxiAPI",
        Version = "teste"
    });
});

builder.Services.AddResponseCaching();

var app = builder.Build();


// Configure the HTTP request pipeline.
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

app.UseResponseCaching();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
