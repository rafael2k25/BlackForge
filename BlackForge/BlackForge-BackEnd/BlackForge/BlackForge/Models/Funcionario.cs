namespace BlackForge.Models
{
    public class Funcionario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Setor { get; set; } = string.Empty;
        public DateTime DataAdmissao { get; set; }
        public string? Email { get; set; }
        public string? Observacoes { get; set; }
    }
}
