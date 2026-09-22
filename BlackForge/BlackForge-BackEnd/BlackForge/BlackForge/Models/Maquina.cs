namespace BlackForge.Models
{
    public class Maquina
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Fabricante { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public DateTime DataAquisicao { get; set; }
        public string? Observacoes { get; set; }
        public ICollection<ProcessoProducao> ProcessosProducao { get; set; }
            = new List<ProcessoProducao>();
        public ICollection<ConfiguracaoMaquina> Configuracoes { get; set; }
            = new List<ConfiguracaoMaquina>();
    }
}
