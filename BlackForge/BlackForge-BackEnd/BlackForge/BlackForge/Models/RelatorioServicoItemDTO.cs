namespace BlackForge.Models
{
    public class RelatorioServicoItemDTO
    {
        public int Id { get; set; }

        public string NumeroOS { get; set; } = string.Empty;

        public string Cliente { get; set; } = string.Empty;

        public string TipoServico { get; set; } = string.Empty;

        public string Responsavel { get; set; } = string.Empty;

        public DateTime DataAbertura { get; set; }

        public DateTime? DataEntrega { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal ValorTotal { get; set; }
    }
}