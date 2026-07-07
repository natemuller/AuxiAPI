using System.Globalization;
using System.Text;

namespace AuxiAPI.src.Common.Text;

public static class TextNormalizer
{
    public static string NormalizarBusca(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        var semAcentos = RemoverAcentos(texto.Trim());

        return semAcentos.ToLowerInvariant();
    }

    private static string RemoverAcentos(string texto)
    {
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var caractere in normalizado)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(caractere);

            if (categoria != UnicodeCategory.NonSpacingMark)
                builder.Append(caractere);
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }
}