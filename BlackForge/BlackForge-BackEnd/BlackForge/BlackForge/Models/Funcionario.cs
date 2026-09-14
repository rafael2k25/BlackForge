using System.Text.Json.Serialization;

namespace BlackForge.Models
{
    public class Funcionario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public int Idade { get; set; }
        public string? Telefone { get; set; }
        public string Setor { get; set; } = string.Empty;
        [JsonPropertyName("admissao")]
        public DateTime DataAdmissao { get; set; }
        public string? Email { get; set; }
        public string? Observacoes { get; set; }
    }
}
