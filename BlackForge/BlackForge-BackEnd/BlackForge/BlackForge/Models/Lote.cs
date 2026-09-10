namespace BlackForge.Models
{
    public class Lote
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public DateTime? DataFabricacao { get; set; }
        public DateTime? DataValidade { get; set; }
        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;
    }
}
