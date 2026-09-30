namespace BlackForge.Models
{
    public class MaterialCreateDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Unidade { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public decimal EstoqueMinimo { get; set; }
    }
    public class MaterialDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Unidade { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public decimal EstoqueMinimo { get; set; }
        public decimal QuantidadeTotal { get; set; }
        public decimal CustoMedio { get; set; }
        public bool AbaixoDoMinimo { get; set; }
    }
}