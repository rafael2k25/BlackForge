namespace BlackForge.Models
{
    public class Lote
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;       
        public decimal Quantidade { get; set; }
        public decimal CustoUnitario { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime? DataFabricacao { get; set; }
        public DateTime? DataValidade { get; set; }
        public string? Observacoes { get; set; }
        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;
    }
}