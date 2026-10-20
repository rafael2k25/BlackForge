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

        // Simulação: após 120s de produção efetiva acumulada, a OS é concluída.
        private const decimal LimiteTempoProducaoSegundos = 120m;

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
            // Protegido por try/catch: uma falha aqui não pode derrubar o host.
            try
            {
                await InicializarRelogiosAsync(stoppingToken);
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Falha ao inicializar os relógios de produção.");
            }

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

            var agora = DateTime.Now;

            // Parâmetros inválidos: pausa o processo para liberar a máquina
            // em vez de deixá-lo preso em EM_EXECUCAO e logando a cada ciclo.
            if (processo.QuantidadePlanejada <= 0 ||
                processo.ProducaoPorMinuto <= 0 ||
                processo.ConsumoPorUnidade < 0)
            {
                await PausarProcessoAsync(
                    context,
                    transacao,
                    processo,
                    agora,
                    "possui parâmetros inválidos",
                    cancellationToken);
                return;
            }

            if (processo.ConsumoPorUnidade > 0 &&
                !processo.MaterialId.HasValue)
            {
                await PausarProcessoAsync(
                    context,
                    transacao,
                    processo,
                    agora,
                    "não possui material configurado",
                    cancellationToken);
                return;
            }

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

            var tempoAcumulado = Math.Min(
                processo.TempoProducaoSegundos + segundosDecorridos,
                LimiteTempoProducaoSegundos);

            var limiteAtingido =
                tempoAcumulado >= LimiteTempoProducaoSegundos;

            // Ao atingir 120s de produção efetiva, entrega a quantidade planejada.
            var quantidadeCalculada = limiteAtingido
                ? processo.QuantidadePlanejada
                : (int)Math.Floor(
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
                    // sem estoque não entra no tempo produtivo acumulado,
                    // e a OS não é concluída sem registrar o consumo.
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

                // Saídas já registradas hoje por este processo, por lote.
                // O consumo é somado nelas, em vez de criar uma movimentação
                // nova a cada ciclo: uma linha por processo, lote e dia.
                var saidasDoDia = consumoAdicional > 0
                    ? (await context.Movimentacoes
                        .Where(m =>
                            m.ProcessoProducaoId == processo.Id &&
                            m.Tipo == "saida" &&
                            m.DataMovimentacao >= hoje &&
                            m.LoteId != null)
                        .ToListAsync(cancellationToken))
                        .GroupBy(m => m.LoteId!.Value)
                        .ToDictionary(g => g.Key, g => g.First())
                    : new Dictionary<int, Movimentacao>();

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

                    if (saidasDoDia.TryGetValue(lote.Id, out var saida))
                    {
                        saida.Quantidade = Math.Round(
                            saida.Quantidade + retirar,
                            4,
                            MidpointRounding.AwayFromZero);
                    }
                    else
                    {
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
                }

                if (restante > 0)
                {
                    // Sem commit: a transação é descartada e nada é alterado.
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

                _logger.LogInformation(
                    "Processo {Id} concluído após {Tempo}s de produção. " +
                    "OS {OsId} concluída. Máquina {MaquinaId} liberada.",
                    processo.Id,
                    tempoAcumulado,
                    processo.OrdemServicoId,
                    processo.MaquinaId);
            }

            await context.SaveChangesAsync(cancellationToken);
            await transacao.CommitAsync(cancellationToken);
        }

        private async Task PausarProcessoAsync(
            BlackForgeDbContext context,
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transacao,
            ProcessoProducao processo,
            DateTime agora,
            string motivo,
            CancellationToken cancellationToken)
        {
            processo.Status = "PAUSADO";
            processo.DataFim = agora;
            processo.UltimaAtualizacaoProducao = agora;

            await context.SaveChangesAsync(cancellationToken);
            await transacao.CommitAsync(cancellationToken);

            _logger.LogError(
                "Processo {Id} {Motivo}. Pausado e máquina {MaquinaId} liberada.",
                processo.Id,
                motivo,
                processo.MaquinaId);
        }
    }
}