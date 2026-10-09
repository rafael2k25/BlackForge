
using BlackForge.Data;
using BlackForge.Models;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Services
{
    public class ProducaoBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ProducaoBackgroundService> _logger;

        private static readonly TimeSpan Intervalo =
            TimeSpan.FromSeconds(5);

        public ProducaoBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ProducaoBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Serviço automático de produção do BlackForge iniciado.");

            // Ao iniciar a API, reinicia a referência temporal dos processos
            // ativos. O tempo em que a API esteve desligada não é contado.
            await InicializarRelogiosAsync(stoppingToken);

            using var timer = new PeriodicTimer(Intervalo);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!await timer.WaitForNextTickAsync(stoppingToken))
                        break;

                    await ProcessarProducoesAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Erro ao atualizar as produções automáticas.");
                }
            }
        }

        private async Task InicializarRelogiosAsync(
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<BlackForgeDbContext>();

            var agora = DateTime.Now;

            var processos = await context.ProcessosProducao
                .Where(p => p.Status == "EM_EXECUCAO")
                .ToListAsync(cancellationToken);

            foreach (var processo in processos)
            {
                // Não soma o período em que a API esteve desligada.
                processo.UltimaAtualizacaoProducao = agora;
            }

            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Relógios de {Quantidade} processos ativos inicializados.",
                processos.Count);
        }

        private async Task ProcessarProducoesAsync(
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<BlackForgeDbContext>();

            var processosIds = await context.ProcessosProducao
                .AsNoTracking()
                .Where(p => p.Status == "EM_EXECUCAO")
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            foreach (var processoId in processosIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await AtualizarProcessoAsync(
                        processoId,
                        cancellationToken);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Erro ao processar o processo de produção {Id}.",
                        processoId);
                }
            }
        }

        private async Task AtualizarProcessoAsync(
            int processoId,
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<BlackForgeDbContext>();

            await using var transacao =
                await context.Database.BeginTransactionAsync(
                    cancellationToken);

            var processo = await context.ProcessosProducao
                .Include(p => p.OrdemServico)
                .FirstOrDefaultAsync(
                    p => p.Id == processoId &&
                         p.Status == "EM_EXECUCAO",
                    cancellationToken);

            if (processo == null)
                return;

            if (processo.QuantidadePlanejada <= 0 ||
                processo.ProducaoPorMinuto <= 0 ||
                processo.ConsumoPorUnidade < 0)
            {
                _logger.LogWarning(
                    "Processo {Id} possui parâmetros inválidos.",
                    processo.Id);
                return;
            }

            if (processo.ConsumoPorUnidade > 0 &&
                !processo.MaterialId.HasValue)
            {
                _logger.LogWarning(
                    "Processo {Id} não possui material configurado.",
                    processo.Id);
                return;
            }

            var agora = DateTime.Now;

            // Se ainda não existe referência temporal, inicializa sem
            // contabilizar um intervalo desconhecido.
            if (!processo.UltimaAtualizacaoProducao.HasValue)
            {
                processo.UltimaAtualizacaoProducao = agora;
                await context.SaveChangesAsync(cancellationToken);
                await transacao.CommitAsync(cancellationToken);
                return;
            }

            var segundosDecorridos = Math.Max(
                0,
                (decimal)(agora -
                    processo.UltimaAtualizacaoProducao.Value).TotalSeconds);

            // Evita acumular um intervalo absurdo em caso de relógio
            // alterado ou de uma interrupção inesperada do serviço.
            segundosDecorridos = Math.Min(segundosDecorridos, 30m);

            var tempoAcumulado =
                processo.TempoProducaoSegundos + segundosDecorridos;

            var quantidadeCalculada = (int)Math.Floor(
                tempoAcumulado / 60m *
                processo.ProducaoPorMinuto);

            var quantidadeDesejada = Math.Min(
                processo.QuantidadePlanejada,
                Math.Max(
                    processo.QuantidadeProduzida,
                    quantidadeCalculada));

            var consumoDesejado = Math.Round(
                quantidadeDesejada * processo.ConsumoPorUnidade,
                4,
                MidpointRounding.AwayFromZero);

            var consumoAdicional = Math.Round(
                consumoDesejado -
                processo.QuantidadeConsumidaRegistrada,
                4,
                MidpointRounding.AwayFromZero);

            if (consumoAdicional < 0)
                consumoAdicional = 0;

            // Se a produção precisa de material, valida o estoque antes
            // de acumular tempo ou avançar a quantidade produzida.
            if (processo.ConsumoPorUnidade > 0)
            {
                var materialId = processo.MaterialId!.Value;

                var hoje = DateTime.Today;

                var lotes = await context.Lotes
                    .Where(l =>
                        l.MaterialId == materialId &&
                        l.Quantidade > 0 &&
                        (l.DataValidade == null ||
                         l.DataValidade >= hoje))
                    .OrderBy(l => l.DataValidade == null)
                    .ThenBy(l => l.DataValidade)
                    .ThenBy(l => l.DataEntrada)
                    .ThenBy(l => l.Id)
                    .ToListAsync(cancellationToken);

                var estoqueDisponivel = lotes.Sum(l => l.Quantidade);

                if (estoqueDisponivel < consumoAdicional)
                {
                    // Atualiza apenas a referência temporal. O intervalo
                    // sem estoque não entra no tempo produtivo acumulado.
                    processo.UltimaAtualizacaoProducao = agora;

                    await context.SaveChangesAsync(cancellationToken);
                    await transacao.CommitAsync(cancellationToken);

                    _logger.LogWarning(
                        "Processo {Id} aguardando estoque. " +
                        "Necessário: {Necessario}; disponível: {Disponivel}.",
                        processo.Id,
                        consumoAdicional,
                        estoqueDisponivel);

                    return;
                }

                var restante = consumoAdicional;

                foreach (var lote in lotes)
                {
                    if (restante <= 0)
                        break;

                    var retirar = Math.Round(
                        Math.Min(lote.Quantidade, restante),
                        4,
                        MidpointRounding.AwayFromZero);

                    if (retirar <= 0)
                        continue;

                    lote.Quantidade = Math.Round(
                        lote.Quantidade - retirar,
                        4,
                        MidpointRounding.AwayFromZero);

                    restante = Math.Round(
                        restante - retirar,
                        4,
                        MidpointRounding.AwayFromZero);

                    context.Movimentacoes.Add(new Movimentacao
                    {
                        Tipo = "saida",
                        Quantidade = retirar,
                        DataMovimentacao = agora,
                        MaterialId = materialId,
                        LoteId = lote.Id,
                        ProcessoProducaoId = processo.Id,
                        Observacoes =
                            $"Consumo automático da produção. " +
                            $"Processo #{processo.Id}; " +
                            $"OS #{processo.OrdemServicoId}."
                    });
                }

                if (restante > 0)
                {
                    _logger.LogWarning(
                        "Consumo incompleto no processo {Id}.",
                        processo.Id);
                    return;
                }

                processo.QuantidadeConsumidaRegistrada =
                    Math.Round(
                        processo.QuantidadeConsumidaRegistrada +
                        consumoAdicional,
                        4,
                        MidpointRounding.AwayFromZero);
            }

            processo.TempoProducaoSegundos = tempoAcumulado;
            processo.UltimaAtualizacaoProducao = agora;
            processo.QuantidadeProduzida = quantidadeDesejada;
            processo.MaterialConsumido =
                processo.QuantidadeConsumidaRegistrada;

            if (processo.QuantidadeProduzida >=
                processo.QuantidadePlanejada)
            {
                processo.QuantidadeProduzida =
                    processo.QuantidadePlanejada;

                processo.Status = "CONCLUIDO";
                processo.DataFim = agora;

                processo.OrdemServico.Status = "concluida";
                processo.OrdemServico.DataConclusao = agora;
            }

            await context.SaveChangesAsync(cancellationToken);
            await transacao.CommitAsync(cancellationToken);
        }
    }
}
