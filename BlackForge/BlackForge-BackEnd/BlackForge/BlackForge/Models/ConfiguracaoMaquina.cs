namespace BlackForge.Models
{
    public class ConfiguracaoMaquina
    {
        public int Id { get; set; }
        public int MaquinaId { get; set; }
        public Maquina Maquina { get; set; } = null!;
        public string TipoServico { get; set; } = string.Empty;
        public decimal ProducaoPorMinuto { get; set; }
        public decimal ConsumoPorUnidade { get; set; }
        public bool Ativa { get; set; } = true;
    }
}