namespace BlackForge.Models
{
    public class ConfiguracaoSistema
    {
        public int Id { get; set; }
        public int TemaId { get; set; }
        public Tema Tema { get; set; } = null!;
    }
}