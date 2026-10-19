namespace BlackForge.Models
{
    public class ConfiguracaoPrecificacao
    {
        public int Id { get; set; }
        public decimal MargemDesejadaPercentual { get; set; }
        public decimal ImpostosPercentual { get; set; }
        public decimal DespesasVariaveisPercentual { get; set; }
        public decimal CustosIndiretosPercentual { get; set; }
        public decimal SetupPadraoMinutos { get; set; }
        public decimal CustoHoraOperadorPadrao { get; set; }
        public bool Ativa { get; set; } = true;
        public DateTime DataVigencia { get; set; }
        public string? Observacoes { get; set; }
        public ICollection<Precificacao> Precificacoes { get; set; }
            = new List<Precificacao>();
    }
}
