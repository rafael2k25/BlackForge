namespace BlackForge.Models
{
    // Entrada de um novo lote
    public class LoteCreateDto
    {
        public int MaterialId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public decimal CustoUnitario { get; set; }
        public DateTime? DataEntrada { get; set; }
        public DateTime? DataFabricacao { get; set; }
        public DateTime? DataValidade { get; set; }
        public string? Observacoes { get; set; }
    }

    // Edição: a quantidade não é editável aqui (muda só por movimentação)
    public class LoteUpdateDto
    {
        public string Codigo { get; set; } = string.Empty;
        public decimal CustoUnitario { get; set; }
        public DateTime? DataFabricacao { get; set; }
        public DateTime? DataValidade { get; set; }
        public string? Observacoes { get; set; }
    }

    public class LoteDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int MaterialId { get; set; }
        public string MaterialCodigo { get; set; } = string.Empty;
        public string MaterialNome { get; set; } = string.Empty;
        public string Unidade { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public decimal CustoUnitario { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime? DataFabricacao { get; set; }
        public DateTime? DataValidade { get; set; }
        public string Status { get; set; } = "sem-validade";
        public string? Observacoes { get; set; }
    }
}