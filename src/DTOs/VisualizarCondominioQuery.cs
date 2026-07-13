namespace AuxiAPI.src.DTOs
{
    public class VisualizarCondominioQuery
    {
        private string? _cnpj;

        public string? Cnpj
        {
            get => _cnpj;
            set => _cnpj = value != null
                ? new string(value.Where(char.IsDigit).ToArray())
                : null;
        }

        public string? NomeCondom { get; set; }

        public int Pagina { get; set; } = 1;
    }
}