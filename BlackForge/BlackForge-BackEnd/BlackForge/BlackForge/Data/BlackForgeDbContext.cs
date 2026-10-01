
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

        // =========================================================
        // DBSETS
        // =========================================================

        public DbSet<Material> Materiais { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<Movimentacao> Movimentacoes { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Maquina> Maquinas { get; set; }
        public DbSet<OrdemServico> OrdensServico { get; set; }
        public DbSet<ProcessoProducao> ProcessosProducao { get; set; }
        public DbSet<Tema> Temas { get; set; }
        public DbSet<ConfiguracaoSistema> ConfiguracaoSistema { get; set; }
        public DbSet<ConfiguracaoMaquina> ConfiguracoesMaquina { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================================
            // MATERIAL
            // =========================================================

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

            modelBuilder.Entity<Material>()
                .Property(m => m.Descricao)
                .HasMaxLength(500)
                .IsRequired(false);

            modelBuilder.Entity<Material>()
                .Property(m => m.EstoqueMinimo)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Material>()
                .HasIndex(m => m.Codigo)
                .IsUnique();

            // =========================================================
            // LOTE
            // =========================================================

            modelBuilder.Entity<Lote>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<Lote>()
                .Property(l => l.Codigo)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Lote>()
                .Property(l => l.Quantidade)
                .HasPrecision(18, 2)
                .IsRequired();

            modelBuilder.Entity<Lote>()
                .Property(l => l.CustoUnitario)
                .HasPrecision(18, 2)
                .IsRequired();

            modelBuilder.Entity<Lote>()
                .Property(l => l.DataEntrada)
                .IsRequired();

            modelBuilder.Entity<Lote>()
                .Property(l => l.DataFabricacao)
                .IsRequired(false);

            modelBuilder.Entity<Lote>()
                .Property(l => l.DataValidade)
                .IsRequired(false);

            modelBuilder.Entity<Lote>()
                .Property(l => l.Observacoes)
                .HasMaxLength(500)
                .IsRequired(false);

            // Exclusão em cascata:
            // Excluir Material também exclui seus Lotes.

            modelBuilder.Entity<Lote>()
                .HasOne(l => l.Material)
                .WithMany()
                .HasForeignKey(l => l.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Lote>()
                .HasIndex(l => new
                {
                    l.MaterialId,
                    l.Codigo
                })
                .IsUnique();

            // =========================================================
            // MOVIMENTAÇÃO
            // =========================================================

            modelBuilder.Entity<Movimentacao>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Movimentacao>()
                .Property(m => m.Tipo)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<Movimentacao>()
                .Property(m => m.Quantidade)
                .HasPrecision(18, 2)
                .IsRequired();

            modelBuilder.Entity<Movimentacao>()
                .Property(m => m.DataMovimentacao)
                .IsRequired();

            modelBuilder.Entity<Movimentacao>()
                .Property(m => m.Observacoes)
                .HasMaxLength(500)
                .IsRequired(false);

            // Excluir Material também exclui suas Movimentações.

            modelBuilder.Entity<Movimentacao>()
                .HasOne(m => m.Material)
                .WithMany()
                .HasForeignKey(m => m.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            // A relação com Lote permanece restritiva.
            // O material excluído já terá suas movimentações removidas.

            modelBuilder.Entity<Movimentacao>()
                .HasOne(m => m.Lote)
                .WithMany()
                .HasForeignKey(m => m.LoteId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // FUNCIONÁRIO
            // =========================================================

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

            // =========================================================
            // MÁQUINA
            // =========================================================

            modelBuilder.Entity<Maquina>()
                .HasKey(m => m.Id);

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

            modelBuilder.Entity<Maquina>()
                .Property(m => m.DataAquisicao)
                .IsRequired();

            modelBuilder.Entity<Maquina>()
                .HasIndex(m => m.Codigo)
                .IsUnique();

            modelBuilder.Entity<Maquina>()
                .HasIndex(m => m.NumeroSerie)
                .IsUnique();

            // =========================================================
            // ORDEM DE SERVIÇO
            // =========================================================

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
                .Property(o => o.Contato)
                .HasMaxLength(100)
                .IsRequired(false);

            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.Endereco)
                .HasMaxLength(300)
                .IsRequired(false);

            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.DataAbertura)
                .IsRequired();

            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.DescricaoServico)
                .HasMaxLength(1000)
                .IsRequired();

            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.TipoServico)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.DataEntrega)
                .IsRequired(false);

            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.ValorMateriais)
                .HasPrecision(18, 2)
                .IsRequired();

            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.ValorMaoObra)
                .HasPrecision(18, 2)
                .IsRequired();

            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.Desconto)
                .HasPrecision(18, 2)
                .IsRequired();

            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.ValorTotal)
                .HasPrecision(18, 2)
                .IsRequired();

            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.CondicaoPagamento)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            modelBuilder.Entity<OrdemServico>()
                .Property(o => o.Observacoes)
                .HasMaxLength(500)
                .IsRequired(false);

            modelBuilder.Entity<OrdemServico>()
                .HasIndex(o => o.NumeroOS)
                .IsUnique();

            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.Funcionario)
                .WithMany()
                .HasForeignKey(o => o.FuncionarioId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.Maquina)
                .WithMany()
                .HasForeignKey(o => o.MaquinaId)
                .HasPrincipalKey(m => m.Id)
                .OnDelete(DeleteBehavior.SetNull);

            // =========================================================
            // PROCESSOS DE PRODUÇÃO
            // =========================================================

            modelBuilder.Entity<ProcessoProducao>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<ProcessoProducao>()
                .Property(p => p.DataInicio)
                .IsRequired();

            modelBuilder.Entity<ProcessoProducao>()
                .Property(p => p.DataFim)
                .IsRequired(false);

            modelBuilder.Entity<ProcessoProducao>()
                .Property(p => p.QuantidadePlanejada)
                .IsRequired();

            modelBuilder.Entity<ProcessoProducao>()
                .Property(p => p.QuantidadeProduzida)
                .IsRequired();

            modelBuilder.Entity<ProcessoProducao>()
                .Property(p => p.ProducaoPorMinuto)
                .HasPrecision(18, 2)
                .IsRequired();

            modelBuilder.Entity<ProcessoProducao>()
                .Property(p => p.ConsumoPorUnidade)
                .HasPrecision(18, 4)
                .IsRequired();

            modelBuilder.Entity<ProcessoProducao>()
                .Property(p => p.MaterialConsumido)
                .HasPrecision(18, 4)
                .IsRequired();

            modelBuilder.Entity<ProcessoProducao>()
                .Property(p => p.Status)
                .HasMaxLength(30)
                .IsRequired();

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

            // =========================================================
            // CONFIGURAÇÃO DA MÁQUINA
            // =========================================================

            modelBuilder.Entity<ConfiguracaoMaquina>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<ConfiguracaoMaquina>()
                .Property(c => c.TipoServico)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<ConfiguracaoMaquina>()
                .Property(c => c.ProducaoPorMinuto)
                .HasPrecision(18, 2)
                .IsRequired();

            modelBuilder.Entity<ConfiguracaoMaquina>()
                .Property(c => c.ConsumoPorUnidade)
                .HasPrecision(18, 4)
                .IsRequired();

            modelBuilder.Entity<ConfiguracaoMaquina>()
                .Property(c => c.Ativa)
                .IsRequired();

            modelBuilder.Entity<ConfiguracaoMaquina>()
                .HasOne(c => c.Maquina)
                .WithMany(m => m.Configuracoes)
                .HasForeignKey(c => c.MaquinaId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================================================
            // TEMAS
            // =========================================================

            modelBuilder.Entity<Tema>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Tema>()
                .Property(t => t.Nome)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Tema>()
                .Property(t => t.Codigo)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Tema>()
                .Property(t => t.Ativo)
                .IsRequired();

            modelBuilder.Entity<Tema>()
                .HasIndex(t => t.Codigo)
                .IsUnique();

            // =========================================================
            // CONFIGURAÇÃO DO SISTEMA
            // =========================================================

            modelBuilder.Entity<ConfiguracaoSistema>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<ConfiguracaoSistema>()
                .Property(c => c.TemaId)
                .IsRequired();

            modelBuilder.Entity<ConfiguracaoSistema>()
                .HasOne(c => c.Tema)
                .WithMany()
                .HasForeignKey(c => c.TemaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}