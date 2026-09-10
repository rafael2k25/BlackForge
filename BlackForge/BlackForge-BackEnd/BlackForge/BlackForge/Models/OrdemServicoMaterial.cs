namespace BlackForge.Models
{
    public class OrdemServicoMaterial
    {
        public int Id { get; set; }
        public int OrdemServicoId { get; set; }
        public OrdemServico OrdemServico { get; set; } = null!;
        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;
        public decimal Quantidade { get; set; }
        public string Unidade { get; set; } = string.Empty;
        public decimal ValorUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
