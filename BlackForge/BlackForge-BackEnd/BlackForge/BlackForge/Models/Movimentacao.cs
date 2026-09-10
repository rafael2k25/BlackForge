namespace BlackForge.Models
{
    public class Movimentacao
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public DateTime DataMovimentacao { get; set; }
        public string? Observacoes { get; set; }
        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;
        public int? LoteId { get; set; }
        public Lote? Lote { get; set; }
    }
}
