using Microsoft.AspNetCore.Mvc.Filters;

namespace AuxiAPI.src.Middlewares
{
    public class ValidarModelStateFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var mensagemErro = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(mensagemErro) || mensagemErro.Contains("is not valid"))
                {
                    mensagemErro = "dados ou formato da requisição são inválidos.";
                }
                throw new ArgumentException(mensagemErro);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) {}
    }
}