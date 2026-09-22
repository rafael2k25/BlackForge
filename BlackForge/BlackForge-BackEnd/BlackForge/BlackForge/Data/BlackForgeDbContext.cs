using BlackForge.Models;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Data
{
    public class BlackForgeDbContext : DbContext
    {
        public BlackForgeDbContext(DbContextOptions<BlackForgeDbContext> options)
            : base(options)
        {
        }
        public DbSet<Material> Materiais { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<Movimentacao> Movimentacoes { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Maquina> Maquinas { get; set; }
        public DbSet<OrdemServico> OrdensServico { get; set; }
        public DbSet<OrdemServicoMaterial> OrdensServicoMateriais { get; set; }
        public DbSet<ProcessoProducao> ProcessosProducao { get; set; }
        public DbSet<ConfiguracaoMaquina> ConfiguracoesMaquina { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // ================= MATERIAL =================
            modelBuilder.Entity<Material>()
                .HasKey(m => m.Id);
            modelBuilder.Entity<Material>()
                .Property(m => m.Codigo)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<Material>()
                .Property(m => m.Nome)
                .HasMaxLength(150)
                .IsRequired();
            modelBuilder.Entity<Material>()
                .Property(m => m.Categoria)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<Material>()
                .Property(m => m.Unidade)
                .HasMaxLength(20)
                .IsRequired();

            // ================= LOTE =================         
            modelBuilder.Entity<Lote>()
                .HasKey(l => l.Id);
            modelBuilder.Entity<Lote>()
                .Property(l => l.Codigo)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<Lote>()
                .Property(l => l.Quantidade)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Lote>()
                .HasOne(l => l.Material)
                .WithMany()
                .HasForeignKey(l => l.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= MOVIMENTAÇÃO =================
            modelBuilder.Entity<Movimentacao>()
                .HasKey(m => m.Id);
            modelBuilder.Entity<Movimentacao>()
                .Property(m => m.Tipo)
                .HasMaxLength(20)
                .IsRequired();
            modelBuilder.Entity<Movimentacao>()
                .Property(m => m.Quantidade)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Movimentacao>()
                .HasOne(m => m.Material)
                .WithMany()
                .HasForeignKey(m => m.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Movimentacao>()
                .HasOne(m => m.Lote)
                .WithMany()
                .HasForeignKey(m => m.LoteId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= FUNCIONÁRIO =================
            modelBuilder.Entity<Funcionario>()
                .HasKey(f => f.Id);
            modelBuilder.Entity<Funcionario>()
                .Property(f => f.Nome)
                .HasMaxLength(150)
                .IsRequired();
            modelBuilder.Entity<Funcionario>()
                .Property(f => f.Matricula)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<Funcionario>()
                .Property(f => f.CPF)
                .HasMaxLength(14)
                .IsRequired();
            modelBuilder.Entity<Funcionario>()
                .Property(f => f.Cargo)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Funcionario>()
                .Property(f => f.Idade)
                .IsRequired();
            modelBuilder.Entity<Funcionario>()
                .Property(f => f.Telefone)
                .HasMaxLength(20)
                .IsRequired(false);
            modelBuilder.Entity<Funcionario>()
                .Property(f => f.Setor)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Funcionario>()
                .Property(f => f.DataAdmissao)
                .IsRequired();
            modelBuilder.Entity<Funcionario>()
                .Property(f => f.Email)
                .HasMaxLength(150)
                .IsRequired(false);
            modelBuilder.Entity<Funcionario>()
                .Property(f => f.Observacoes)
                .HasMaxLength(500)
                .IsRequired(false);
            modelBuilder.Entity<Funcionario>()
                .HasIndex(f => f.Matricula)
                .IsUnique();
            modelBuilder.Entity<Funcionario>()
                .HasIndex(f => f.CPF)
                .IsUnique();

            // ================= MÁQUINA =================

            modelBuilder.Entity<Maquina>().HasKey(m => m.Id);
            modelBuilder.Entity<Maquina>()
                .Property(m => m.Nome)
                .HasMaxLength(150)
                .IsRequired();
            modelBuilder.Entity<Maquina>()
                .Property(m => m.Codigo)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<Maquina>()
                .Property(m => m.Fabricante)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Maquina>()
                .Property(m => m.Modelo)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Maquina>()
                .Property(m => m.NumeroSerie)
                .HasMaxLength(100)
                .IsRequired();

            // ================= ORDEM DE SERVIÇO =================
            modelBuilder.Entity<OrdemServico>()
                .HasKey(o => o.Id);
            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.NumeroOS)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.Cliente)
                .HasMaxLength(150)
                .IsRequired();
            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.TipoServico)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.ValorMateriais)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.ValorMaoObra)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.Desconto)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.ValorTotal)
                .HasPrecision(18, 2);

            // Funcionário responsável pela OS
            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.Funcionario)
                .WithMany()
                .HasForeignKey(o => o.FuncionarioId)
                .OnDelete(DeleteBehavior.SetNull);

            // ================= ORDEM DE SERVIÇO E MATERIAL =================
            modelBuilder.Entity<OrdemServicoMaterial>()
                .HasKey(om => om.Id);
            modelBuilder.Entity<OrdemServicoMaterial>()
                .Property(om => om.Quantidade)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrdemServicoMaterial>()
                .Property(om => om.ValorUnitario)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrdemServicoMaterial>()
                .Property(om => om.Subtotal)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrdemServicoMaterial>()
                .Property(om => om.Unidade)
                .HasMaxLength(20)
                .IsRequired();
            modelBuilder.Entity<OrdemServicoMaterial>()
                .HasOne(om => om.OrdemServico)
                .WithMany(o => o.Materiais)
                .HasForeignKey(om => om.OrdemServicoId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OrdemServicoMaterial>()
                .HasOne(om => om.Material)
                .WithMany()
                .HasForeignKey(om => om.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= PROCESSOS DE PRODUÇÃO =================

            modelBuilder.Entity<ProcessoProducao>().HasKey(p => p.Id);
            modelBuilder.Entity<ProcessoProducao>()
                .Property(p => p.ProducaoPorMinuto)
                .HasPrecision(18, 2);
            modelBuilder.Entity<ProcessoProducao>()
                .Property(p => p.ConsumoPorUnidade)
                .HasPrecision(18, 4);
            modelBuilder.Entity<ProcessoProducao>()
                .Property(p => p.MaterialConsumido)
                .HasPrecision(18, 4);
            modelBuilder.Entity<ProcessoProducao>()
                .Property(p => p.Status)
                .HasMaxLength(30)
                .IsRequired();
            modelBuilder.Entity<ProcessoProducao>()
                .Property(p => p.Observacoes)
                .HasMaxLength(500)
                .IsRequired(false);
            modelBuilder.Entity<ProcessoProducao>()
                .HasOne(p => p.Maquina)
                .WithMany(m => m.ProcessosProducao)
                .HasForeignKey(p => p.MaquinaId)
                .HasPrincipalKey(m => m.Id)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProcessoProducao>()
                .HasOne(p => p.OrdemServico)
                .WithMany(o => o.ProcessosProducao)
                .HasForeignKey(p => p.OrdemServicoId)
                .HasPrincipalKey(o => o.Id)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.Maquina)
                .WithMany()
                .HasForeignKey(o => o.MaquinaId)
                .HasPrincipalKey(m => m.Id)
                .OnDelete(DeleteBehavior.SetNull);

            // ================= CONFIGURAÇÃO DA MÁQUINA =================
            modelBuilder.Entity<ConfiguracaoMaquina>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<ConfiguracaoMaquina>()
                .Property(c => c.TipoServico)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<ConfiguracaoMaquina>()
                .Property(c => c.ProducaoPorMinuto)
                .HasPrecision(18, 2);
            modelBuilder.Entity<ConfiguracaoMaquina>()
                .Property(c => c.ConsumoPorUnidade)
                .HasPrecision(18, 4);
            modelBuilder.Entity<ConfiguracaoMaquina>()
                .Property(c => c.Ativa)
                .IsRequired();
            modelBuilder.Entity<ConfiguracaoMaquina>()
                .HasOne(c => c.Maquina)
                .WithMany(m => m.Configuracoes)
                .HasForeignKey(c => c.MaquinaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}