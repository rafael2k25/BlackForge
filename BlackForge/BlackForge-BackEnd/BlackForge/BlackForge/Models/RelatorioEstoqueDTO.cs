namespace BlackForge.Models
{
    public class RelatorioEstoqueDTO
    {
        public int TotalMateriais { get; set; }
        public decimal TotalEntradas { get; set; }
        public decimal TotalSaidas { get; set; }
        public int AbaixoDoMinimo { get; set; }
        public List<RelatorioMovimentacaoPeriodoDTO> PorPeriodo { get; set; } = new();
        public List<RelatorioConsumoMaterialDTO> ConsumoPorMaterial { get; set; } = new();
        public List<RelatorioMovimentacaoItemDTO> Registros { get; set; } = new();
    }

    public class RelatorioMovimentacaoItemDTO
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Material { get; set; } = "";
        public string Lote { get; set; } = "";
        public string Tipo { get; set; } = "";
        public decimal Quantidade { get; set; }
        public string Unidade { get; set; } = "";
        public decimal CustoUnitario { get; set; }
        public decimal Total { get; set; }
    }

    public class RelatorioMovimentacaoPeriodoDTO
    {
        public string Periodo { get; set; } = "";
        public decimal Entradas { get; set; }
        public decimal Saidas { get; set; }
    }

    public class RelatorioConsumoMaterialDTO
    {
        public string Material { get; set; } = "";
        public string Unidade { get; set; } = "";
        public decimal Quantidade { get; set; }
    }
}