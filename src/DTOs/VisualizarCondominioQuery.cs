namespace AuxiAPI.src.DTOs
{
    public class VisualizarCondominioQuery
    {
    public string? CodigoDoCondominio { get; set; }

    private string? _cnpjDoCondominio;
    public string? CNPJDoCondominio
    {
        get => _cnpjDoCondominio;
        set => _cnpjDoCondominio = value != null 
            ? new string(value.Where(char.IsDigit).ToArray()) 
                : null;
    }
    public string? NomeDoCondominio { get; set; }

    public int Pagina { get; set; } = 1;
    }
}