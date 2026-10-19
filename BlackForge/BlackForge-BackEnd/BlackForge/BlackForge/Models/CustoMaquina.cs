namespace BlackForge.Models
{
    public class CustoMaquina
    {
        public int Id { get; set; }
        public int MaquinaId { get; set; }

        public decimal ValorAquisicao { get; set; }
        public decimal ValorResidual { get; set; }
        public decimal VidaUtilHoras { get; set; }

        public decimal ConsumoEnergiaKW { get; set; }
        public decimal CustoKWh { get; set; }
        public decimal CustoManutencaoHora { get; set; }
        public decimal OutrosCustosHora { get; set; }

        public decimal? CustoHoraManual { get; set; }
        public string MetodoCalculo { get; set; } = "DETALHADO";

        public bool Ativa { get; set; } = true;
        public DateTime DataAtualizacao { get; set; }
        public string? Observacoes { get; set; }

        public Maquina? Maquina { get; set; }
    }
}
