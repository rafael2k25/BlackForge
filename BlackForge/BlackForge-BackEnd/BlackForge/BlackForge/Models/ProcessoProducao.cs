namespace BlackForge.Models
{
    public class ProcessoProducao
    {
        public int Id { get; set; }
        public int OrdemServicoId { get; set; }
        public int MaquinaId { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int QuantidadePlanejada { get; set; }
        public int QuantidadeProduzida { get; set; }
        public decimal ProducaoPorMinuto { get; set; }
        public decimal ConsumoPorUnidade { get; set; }
        public decimal MaterialConsumido { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
        public Maquina Maquina { get; set; } = null!;
        public OrdemServico OrdemServico { get; set; } = null!;
    }
}