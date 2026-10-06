namespace BlackForge.Models
{
    public class RelatorioServicosDTO
    {
        public int TotalOS { get; set; }

        public int EmExecucao { get; set; }

        public int Concluidas { get; set; }

        public decimal ValorTotal { get; set; }

        public List<RelatorioStatusDTO> PorStatus { get; set; }
            = new();

        public List<RelatorioTipoServicoDTO> PorTipo { get; set; }
            = new();

        public List<RelatorioServicoItemDTO> Registros { get; set; }
            = new();
    }
}