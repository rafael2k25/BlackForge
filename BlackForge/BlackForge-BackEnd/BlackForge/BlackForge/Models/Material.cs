namespace BlackForge.Models
{
    public class Material
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Unidade { get; set; } = string.Empty;
        public string? Descricao { get; set; }
    }
}
