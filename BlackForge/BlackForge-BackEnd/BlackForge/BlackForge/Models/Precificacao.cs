namespace BlackForge.Models
{
    public class Precificacao
    {
        public int Id { get; set; }
        public int OrdemServicoId { get; set; }
        public int ConfiguracaoPrecificacaoId { get; set; }
        public DateTime DataCalculo { get; set; }
        public int QuantidadeConsiderada { get; set; }
        public decimal CustoMaterialEstimado { get; set; }
        public decimal CustoMaquinaEstimado { get; set; }
        public decimal CustoOperadorEstimado { get; set; }
        public decimal CustoSetupEstimado { get; set; }
        public decimal CustoIndiretoEstimado { get; set; }
        public decimal CustoTotalEstimado { get; set; }
        public decimal MargemDesejadaPercentual { get; set; }
        public decimal ImpostosPercentual { get; set; }
        public decimal DespesasVariaveisPercentual { get; set; }
        public decimal CustosIndiretosPercentual { get; set; }
        public decimal PrecoSugerido { get; set; }
        public decimal? PrecoAprovado { get; set; }
        public string Status { get; set; } = "RASCUNHO";
        public string? Observacoes { get; set; }
        public OrdemServico? OrdemServico { get; set; }
        public ConfiguracaoPrecificacao? ConfiguracaoPrecificacao { get; set; }
        public ICollection<ItemPrecificacao> Itens { get; set; }
            = new List<ItemPrecificacao>();
    }
}
