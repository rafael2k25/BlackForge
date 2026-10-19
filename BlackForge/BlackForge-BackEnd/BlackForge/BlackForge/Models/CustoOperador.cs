namespace BlackForge.Models
{
    public class CustoOperador
    {
        public int Id { get; set; }
        public int? FuncionarioId { get; set; }
        public string? Cargo { get; set; }
        public decimal CustoMensalTotal { get; set; }
        public decimal HorasProdutivasMes { get; set; }
        public decimal? CustoHora { get; private set; }
        public bool Ativo { get; set; } = true;
        public DateTime DataAtualizacao { get; set; }
        public string? Observacoes { get; set; }
        public Funcionario? Funcionario { get; set; }
    }
}
