namespace BlackForge.Models
{
    public class ProcessoProducaoDTO
    {
        public int Id { get; set; }  
        public int OrdemServicoId { get; set; }
        public string NumeroOS { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string TipoServico { get; set; } = string.Empty;
        public int MaquinaId { get; set; }
        public string MaquinaNome { get; set; } = string.Empty;
        public string MaquinaCodigo { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int QuantidadePlanejada { get; set; }
        public int QuantidadeProduzida { get; set; }
        public decimal ProducaoPorMinuto { get; set; }
        public decimal ConsumoPorUnidade { get; set; }
        public decimal MaterialConsumido { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
    }
}