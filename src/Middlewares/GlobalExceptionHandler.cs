using Microsoft.AspNetCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AuxiAPI.src.Common;

namespace AuxiAPI.src.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "exceção capturada no pipeline global: {Message}", exception.Message);

            var (statusCode, mensagemAmigavel) = exception switch
            {
                ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
                
                KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                
                _ => (StatusCodes.Status500InternalServerError, MensagensDeErro.Erro500)
            };

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var respostaFormatada = new
            {
                Sucesso = false,
                Status = statusCode,
                Mensagem = mensagemAmigavel,
                Caminho = httpContext.Request.Path.Value
            };

            await httpContext.Response.WriteAsJsonAsync(respostaFormatada, cancellationToken);

            return true; 
        }
    }
}