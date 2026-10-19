namespace BlackForge.Models
{
    public class ApuracaoCustoOS
    {
        public int Id { get; set; }
        public int OrdemServicoId { get; set; }
        public int Versao { get; set; }
        public DateTime DataApuracao { get; set; }
        public decimal CustoMaterialReal { get; set; }
        public decimal CustoMaquinaReal { get; set; }
        public decimal CustoOperadorReal { get; set; }
        public decimal CustoSetupReal { get; set; }
        public decimal CustoIndiretoReal { get; set; }
        public decimal CustoTotalReal { get; set; }
        public decimal ReceitaReal { get; set; }
        public decimal ResultadoReal { get; private set; }
        public decimal? MargemRealPercentual { get; private set; }
        public string Status { get; set; } = "RASCUNHO";
        public string? Observacoes { get; set; }
        public OrdemServico? OrdemServico { get; set; }
    }
}
