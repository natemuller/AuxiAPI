namespace AuxiAPI.src.DTOs
{
    public class VisualizarUnidadeQuery
    {
        public int? CodCondom { get; set; }
        public string? NomeCondomino { get; set; }
        public int Pagina { get; set; } = 1; 
    }
}