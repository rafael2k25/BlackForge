namespace BlackForge.Models
{
    public class ItemPrecificacao
    {
        public int Id { get; set; }
        public int PrecificacaoId { get; set; }
        public string TipoItem { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int? MaterialId { get; set; }
        public int? LoteId { get; set; }

        // Cópias históricas para preservar a precificação.
        public string? CodigoMaterialSnapshot { get; set; }
        public string? NomeMaterialSnapshot { get; set; }
        public string? CodigoLoteSnapshot { get; set; }

        public decimal Quantidade { get; set; }
        public string? Unidade { get; set; }
        public decimal CustoUnitario { get; set; }
        public decimal CustoTotal { get; set; }
        public Precificacao? Precificacao { get; set; }
        public Material? Material { get; set; }
        public Lote? Lote { get; set; }
    }
}
