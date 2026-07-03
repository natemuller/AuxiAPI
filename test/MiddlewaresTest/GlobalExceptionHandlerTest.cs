using AuxiAPI.src.Common;
using AuxiAPI.src.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace AuxiAPI.Tests.MiddlewaresTest;

public class GlobalExceptionHandlerTest
{
    [Fact]
    public async Task TryHandleAsync_DeveRetornar400_ParaArgumentException()
    {
        var context = CreateContext();
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(context, new ArgumentException("erro de validação"), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Contains("application/json", context.Response.ContentType);

        var body = await ReadBodyAsync(context);
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        Assert.Contains("erro de validação", root.GetProperty("mensagem").GetString());
        Assert.Equal(400, root.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task TryHandleAsync_DeveRetornar404_ParaKeyNotFoundException()
    {
        var context = CreateContext();
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(context, new KeyNotFoundException("não encontrado"), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);

        var body = await ReadBodyAsync(context);
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        Assert.Contains("não encontrado", root.GetProperty("mensagem").GetString());
        Assert.Equal(404, root.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task TryHandleAsync_DeveRetornar500_ParaExceptionGenerica()
    {
        var context = CreateContext();
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(context, new Exception("erro interno"), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        var body = await ReadBodyAsync(context);
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        Assert.Equal(MensagensDeErro.Erro500, root.GetProperty("mensagem").GetString());
        Assert.NotEqual("erro interno", root.GetProperty("mensagem").GetString());
    }

    private static DefaultHttpContext CreateContext()
    {
        return new DefaultHttpContext
        {
            Request = { Path = "/api/condominios/9999" },
            Response = { Body = new MemoryStream() }
        };
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
