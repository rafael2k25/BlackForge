namespace BlackForge.Models
{
    public class OrdemServico
    {
        public int Id { get; set; }
        public string NumeroOS { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string? Contato { get; set; }
        public string? Endereco { get; set; }
        public DateTime DataAbertura { get; set; }
        public string DescricaoServico { get; set; } = string.Empty;
        public string TipoServico { get; set; } = string.Empty;
        public DateTime? DataEntrega { get; set; }
        public int? FuncionarioId { get; set; }
        public Funcionario? Funcionario { get; set; }
        public int? MaquinaId { get; set; }
        public Maquina? Maquina { get; set; }
        public decimal ValorMateriais { get; set; }
        public decimal ValorMaoObra { get; set; }
        public decimal Desconto { get; set; }
        public decimal ValorTotal { get; set; }
        public string? CondicaoPagamento { get; set; }
        public string? Observacoes { get; set; }
        public ICollection<OrdemServicoMaterial> Materiais { get; set; }
            = new List<OrdemServicoMaterial>();
        public ICollection<ProcessoProducao> ProcessosProducao { get; set; }
            = new List<ProcessoProducao>();
    }
}
