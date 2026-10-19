using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrecificacoesController : ControllerBase
    {
        private readonly BlackForgeDbContext _context;

        public PrecificacoesController(BlackForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/Precificacoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Precificacao>>> GetTodas()
        {
            var precificacoes = await _context.Precificacoes
                .AsNoTracking()
                .Include(p => p.Itens)
                .OrderByDescending(p => p.DataCalculo)
                .ToListAsync();

            return Ok(precificacoes);
        }

        // GET: api/Precificacoes/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Precificacao>> GetPorId(int id)
        {
            var precificacao = await _context.Precificacoes
                .AsNoTracking()
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (precificacao == null)
            {
                return NotFound(new
                {
                    mensagem = "Precificação não encontrada."
                });
            }

            return Ok(precificacao);
        }

        // GET: api/Precificacoes/ordem-servico/5
        [HttpGet("ordem-servico/{ordemServicoId:int}")]
        public async Task<ActionResult<IEnumerable<Precificacao>>> GetPorOrdemServico(
            int ordemServicoId)
        {
            var existeOS = await _context.OrdensServico
                .AnyAsync(o => o.Id == ordemServicoId);

            if (!existeOS)
                return NotFound(new { mensagem = "Ordem de Serviço não encontrada." });

            var precificacoes = await _context.Precificacoes
                .AsNoTracking()
                .Include(p => p.Itens)
                .Where(p => p.OrdemServicoId == ordemServicoId)
                .OrderByDescending(p => p.DataCalculo)
                .ToListAsync();

            return Ok(precificacoes);
        }

        // POST: api/Precificacoes/calcular/5
        // Calcula e salva uma nova estimativa para a OS.
        [HttpPost("calcular/{ordemServicoId:int}")]
        public async Task<ActionResult<Precificacao>> Calcular(int ordemServicoId)
        {
            var os = await _context.OrdensServico
                .AsNoTracking()
                .Include(o => o.Funcionario)
                .FirstOrDefaultAsync(o => o.Id == ordemServicoId);

            if (os == null)
                return NotFound(new { mensagem = "Ordem de Serviço não encontrada." });

            if (os.Quantidade <= 0)
                return BadRequest(new { mensagem = "A quantidade da OS deve ser maior que zero." });

            var configuracaoFinanceira = await _context.ConfiguracoesPrecificacao
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Ativa);

            if (configuracaoFinanceira == null)
            {
                return BadRequest(new
                {
                    mensagem = "Não existe uma configuração de precificação ativa."
                });
            }

            decimal margem = configuracaoFinanceira.MargemDesejadaPercentual;
            decimal impostos = configuracaoFinanceira.ImpostosPercentual;
            decimal despesasVariaveis =
                configuracaoFinanceira.DespesasVariaveisPercentual;
            decimal percentualIndireto =
                configuracaoFinanceira.CustosIndiretosPercentual;

            if (margem < 0 || impostos < 0 ||
                despesasVariaveis < 0 || percentualIndireto < 0 ||
                margem + impostos + despesasVariaveis >= 100)
            {
                return BadRequest(new
                {
                    mensagem = "Os percentuais da configuração financeira são inválidos."
                });
            }

            // Busca o processo de produção mais recente da OS, se existir.
            var processo = await _context.ProcessosProducao
                .AsNoTracking()
                .Where(p => p.OrdemServicoId == os.Id)
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            int? materialIdInicial = processo?.MaterialId ?? os.MaterialId;

            // Busca configurações ativas compatíveis com o serviço.
            var consultaConfiguracoes = _context.ConfiguracoesMaquina
                .AsNoTracking()
                .Include(c => c.Maquina)
                .Where(c =>
                    c.Ativa &&
                    c.TipoServico == os.TipoServico);

            if (processo != null)
            {
                consultaConfiguracoes = consultaConfiguracoes
                    .Where(c => c.MaquinaId == processo.MaquinaId);
            }

            if (materialIdInicial.HasValue)
            {
                consultaConfiguracoes = consultaConfiguracoes
                    .Where(c =>
                        c.MaterialId == materialIdInicial.Value ||
                        c.MaterialId == null);
            }

            var configuracoes = await consultaConfiguracoes.ToListAsync();

            if (configuracoes.Count == 0)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Nenhuma máquina ativa é compatível com o tipo de serviço e o material da OS."
                });
            }

            // Custo do operador (não depende da máquina, então vem antes da seleção).
            decimal custoHoraOperador = 0;

            if (os.FuncionarioId.HasValue)
            {
                var custoFuncionario = await _context.CustosOperador
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.FuncionarioId == os.FuncionarioId.Value &&
                        c.Ativo);

                if (custoFuncionario != null)
                    custoHoraOperador = custoFuncionario.CustoHora ?? 0;
            }

            if (custoHoraOperador <= 0)
                custoHoraOperador = configuracaoFinanceira.CustoHoraOperadorPadrao;

            if (custoHoraOperador <= 0)
            {
                return BadRequest(new
                {
                    mensagem =
                        "Não existe custo por hora válido para o operador nem custo padrão configurado."
                });
            }

            // Custo/hora das máquinas candidatas (uma consulta só, sem N+1).
            var idsMaquinas = configuracoes.Select(c => c.MaquinaId).Distinct().ToList();

            var custosAtivos = await _context.CustosMaquina
                .AsNoTracking()
                .Where(c => idsMaquinas.Contains(c.MaquinaId) && c.Ativa)
                .ToListAsync();

            var custoHoraPorMaquina = new Dictionary<int, decimal>();

            foreach (var grupo in custosAtivos.GroupBy(c => c.MaquinaId))
            {
                var custoHoraCalculado = CalcularCustoHoraMaquina(grupo.First());

                if (custoHoraCalculado.HasValue)
                    custoHoraPorMaquina[grupo.Key] = custoHoraCalculado.Value;
            }

            // Candidatas com custo de máquina válido e produtividade > 0.
            var preliminares = configuracoes
                .Where(c => custoHoraPorMaquina.ContainsKey(c.MaquinaId))
                .Select(c => new
                {
                    Config = c,
                    Producao = processo != null ? processo.ProducaoPorMinuto : c.ProducaoPorMinuto,
                    Consumo = processo != null ? processo.ConsumoPorUnidade : c.ConsumoPorUnidade,
                    MaterialId = processo?.MaterialId ?? os.MaterialId ?? c.MaterialId,
                    CustoHora = custoHoraPorMaquina[c.MaquinaId]
                })
                .Where(x => x.Producao > 0 && x.Consumo >= 0)
                .ToList();

            // Lotes FIFO dos materiais envolvidos (uma consulta só).
            var idsMateriais = preliminares
                .Where(x => x.Consumo > 0 && x.MaterialId.HasValue)
                .Select(x => x.MaterialId!.Value)
                .Distinct()
                .ToList();

            var lotesPorMaterial = (await _context.Lotes
                    .AsNoTracking()
                    .Where(l => idsMateriais.Contains(l.MaterialId) &&
                                l.Quantidade > 0 &&
                                l.CustoUnitario >= 0)
                    .OrderBy(l => l.DataValidade == null)
                    .ThenBy(l => l.DataValidade)
                    .ThenBy(l => l.DataEntrada)
                    .ThenBy(l => l.Id)
                    .ToListAsync())
                .GroupBy(l => (int)l.MaterialId)
                .ToDictionary(g => g.Key, g => g.ToList());

            decimal horasSetupBase = configuracaoFinanceira.SetupPadraoMinutos / 60m;

            var viaveis = new List<(ConfiguracaoMaquina Config, decimal Producao,
                decimal Consumo, int? MaterialId, decimal CustoHora, decimal CustoTotal)>();
            var descartadasPorEstoque = new List<object>();

            foreach (var x in preliminares)
            {
                decimal custoMaterial = 0;

                if (x.Consumo > 0)
                {
                    if (!x.MaterialId.HasValue)
                        continue; // consome material, mas não há material definido

                    decimal necessaria = os.Quantidade * x.Consumo;
                    lotesPorMaterial.TryGetValue(x.MaterialId.Value, out var lotesMat);
                    decimal saldo = lotesMat?.Sum(l => l.Quantidade) ?? 0;

                    if (saldo < necessaria)
                    {
                        descartadasPorEstoque.Add(new
                        {
                            configuracaoId = x.Config.Id,
                            maquinaId = x.Config.MaquinaId,
                            quantidadeNecessaria = necessaria,
                            saldoDisponivel = saldo
                        });
                        continue;
                    }

                    custoMaterial = CalcularCustoFifo(lotesMat!, necessaria);
                }

                decimal horasProd = os.Quantidade / x.Producao / 60m;
                decimal custoHoraConjunto = x.CustoHora + custoHoraOperador;
                decimal custoTotal =
                    custoMaterial + (horasProd + horasSetupBase) * custoHoraConjunto;

                viaveis.Add((x.Config, x.Producao, x.Consumo, x.MaterialId, x.CustoHora, custoTotal));
            }

            if (viaveis.Count == 0)
            {
                if (descartadasPorEstoque.Count > 0)
                {
                    return BadRequest(new
                    {
                        mensagem = "Nenhuma máquina compatível tem estoque suficiente do material para esta OS.",
                        candidatasSemEstoque = descartadasPorEstoque
                    });
                }

                return BadRequest(new
                {
                    mensagem = "Nenhuma máquina compatível possui custo ativo válido, produtividade maior que zero e material definido."
                });
            }

            var escolhida = viaveis
                .OrderBy(v => v.CustoTotal)
                .ThenByDescending(v =>
                    materialIdInicial.HasValue && v.Config.MaterialId == materialIdInicial.Value)
                .ThenBy(v => v.Config.Id)
                .First();

            ConfiguracaoMaquina configuracaoMaquina = escolhida.Config;
            decimal custoHoraMaquina = escolhida.CustoHora;
            decimal producaoPorMinuto = escolhida.Producao;
            decimal consumoPorUnidade = escolhida.Consumo;
            int? materialId = escolhida.MaterialId;

            if (consumoPorUnidade < 0)
            {
                return BadRequest(new
                {
                    mensagem = "O consumo por unidade não pode ser negativo."
                });
            }

            // Estima o tempo pela quantidade planejada e produtividade.
            decimal horasProducao =
                os.Quantidade / producaoPorMinuto / 60m;

            decimal minutosSetup =
                configuracaoFinanceira.SetupPadraoMinutos;

            if (minutosSetup < 0)
            {
                return BadRequest(new
                {
                    mensagem = "O tempo de setup não pode ser negativo."
                });
            }

            decimal horasSetup = minutosSetup / 60m;

            decimal custoMaquinaEstimado =
                horasProducao * custoHoraMaquina;

            decimal custoOperadorEstimado =
                horasProducao * custoHoraOperador;

            // O setup considera máquina e operador durante esse período.
            decimal custoSetupEstimado =
                horasSetup * (custoHoraMaquina + custoHoraOperador);

            // Os itens de material serão distribuídos por lote, FIFO.
            var itensMaterial = new List<ItemPrecificacao>();
            decimal custoMaterialEstimado = 0;

            if (consumoPorUnidade > 0)
            {
                if (!materialId.HasValue)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "É necessário definir o material da OS ou da configuração de produção."
                    });
                }

                var material = await _context.Materiais
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == materialId.Value);

                if (material == null)
                {
                    return BadRequest(new
                    {
                        mensagem = "O material associado à produção não foi encontrado."
                    });
                }

                decimal quantidadeNecessaria =
                    os.Quantidade * consumoPorUnidade;

                var lotes = await _context.Lotes
                    .AsNoTracking()
                    .Where(l =>
                        l.MaterialId == materialId.Value &&
                        l.Quantidade > 0 &&
                        l.CustoUnitario >= 0)
                    .OrderBy(l => l.DataValidade == null)
                    .ThenBy(l => l.DataValidade)
                    .ThenBy(l => l.DataEntrada)
                    .ThenBy(l => l.Id)
                    .ToListAsync();

                decimal saldoDisponivel = lotes.Sum(l => l.Quantidade);

                if (saldoDisponivel < quantidadeNecessaria)
                {
                    return BadRequest(new
                    {
                        mensagem =
                            "O estoque disponível é insuficiente para estimar o material pela distribuição FIFO.",
                        quantidadeNecessaria,
                        saldoDisponivel,
                        unidade = material.Unidade
                    });
                }

                decimal restante = quantidadeNecessaria;

                foreach (var lote in lotes)
                {
                    if (restante <= 0)
                        break;

                    decimal quantidadeLote =
                        Math.Min(restante, lote.Quantidade);

                    decimal custoLote = Math.Round(
                        quantidadeLote * lote.CustoUnitario,
                        2,
                        MidpointRounding.AwayFromZero);

                    itensMaterial.Add(new ItemPrecificacao
                    {
                        TipoItem = "MATERIAL",
                        Descricao = $"Material {material.Nome} - lote {lote.Codigo}",
                        MaterialId = material.Id,
                        LoteId = lote.Id,
                        CodigoMaterialSnapshot = material.Codigo,
                        NomeMaterialSnapshot = material.Nome,
                        CodigoLoteSnapshot = lote.Codigo,
                        Quantidade = quantidadeLote,
                        Unidade = material.Unidade,
                        CustoUnitario = lote.CustoUnitario,
                        CustoTotal = custoLote
                    });

                    custoMaterialEstimado += custoLote;
                    restante -= quantidadeLote;
                }
            }

            // Custo base direto.
            decimal custoBase =
                custoMaterialEstimado
                + custoMaquinaEstimado
                + custoOperadorEstimado
                + custoSetupEstimado;

            // Custos indiretos são adicionados ao custo, não ao denominador.
            decimal custoIndiretoEstimado = Math.Round(
                custoBase * percentualIndireto / 100m,
                2,
                MidpointRounding.AwayFromZero);

            decimal custoTotalEstimado =
                custoBase + custoIndiretoEstimado;

            // Margem, impostos e despesas variáveis são percentuais do preço.
            decimal percentualSobreVenda =
                (margem + impostos + despesasVariaveis) / 100m;

            if (percentualSobreVenda >= 1)
            {
                return BadRequest(new
                {
                    mensagem =
                        "A soma de margem, impostos e despesas variáveis deve ser inferior a 100%."
                });
            }

            decimal precoSugerido = Math.Round(
                custoTotalEstimado / (1m - percentualSobreVenda),
                2,
                MidpointRounding.AwayFromZero);

            var precificacao = new Precificacao
            {
                OrdemServicoId = os.Id,
                ConfiguracaoPrecificacaoId = configuracaoFinanceira.Id,
                DataCalculo = DateTime.Now,
                QuantidadeConsiderada = os.Quantidade,
                CustoMaterialEstimado = Math.Round(
                    custoMaterialEstimado, 2, MidpointRounding.AwayFromZero),
                CustoMaquinaEstimado = Math.Round(
                    custoMaquinaEstimado, 2, MidpointRounding.AwayFromZero),
                CustoOperadorEstimado = Math.Round(
                    custoOperadorEstimado, 2, MidpointRounding.AwayFromZero),
                CustoSetupEstimado = Math.Round(
                    custoSetupEstimado, 2, MidpointRounding.AwayFromZero),
                CustoIndiretoEstimado = custoIndiretoEstimado,
                CustoTotalEstimado = Math.Round(
                    custoTotalEstimado, 2, MidpointRounding.AwayFromZero),
                MargemDesejadaPercentual = margem,
                ImpostosPercentual = impostos,
                DespesasVariaveisPercentual = despesasVariaveis,
                CustosIndiretosPercentual = percentualIndireto,
                PrecoSugerido = precoSugerido,
                Status = "RASCUNHO",
                Observacoes =
                    $"Máquina: {configuracaoMaquina.Maquina.Nome}. " +
                    $"Tempo estimado de produção: {horasProducao:F4} horas. " +
                    $"Setup: {minutosSetup:F2} minutos. " +
                    "Estimativa sem movimentação de estoque."
            };

            precificacao.Itens.Add(new ItemPrecificacao
            {
                TipoItem = "MAQUINA",
                Descricao = $"Uso da máquina {configuracaoMaquina.Maquina.Nome}",
                Quantidade = horasProducao,
                Unidade = "hora",
                CustoUnitario = custoHoraMaquina,
                CustoTotal = Math.Round(
                    custoMaquinaEstimado, 2, MidpointRounding.AwayFromZero)
            });

            precificacao.Itens.Add(new ItemPrecificacao
            {
                TipoItem = "OPERADOR",
                Descricao = os.Funcionario != null
                    ? $"Operador: {os.Funcionario.Nome}"
                    : "Operador pelo custo padrão configurado",
                Quantidade = horasProducao,
                Unidade = "hora",
                CustoUnitario = custoHoraOperador,
                CustoTotal = Math.Round(
                    custoOperadorEstimado, 2, MidpointRounding.AwayFromZero)
            });

            precificacao.Itens.Add(new ItemPrecificacao
            {
                TipoItem = "SETUP",
                Descricao = "Setup de máquina e operador",
                Quantidade = horasSetup,
                Unidade = "hora",
                CustoUnitario = custoHoraMaquina + custoHoraOperador,
                CustoTotal = Math.Round(
                    custoSetupEstimado, 2, MidpointRounding.AwayFromZero)
            });

            precificacao.Itens.Add(new ItemPrecificacao
            {
                TipoItem = "INDIRETO",
                Descricao = $"Custos indiretos ({percentualIndireto:F3}%)",
                Quantidade = 1,
                Unidade = "estimativa",
                CustoUnitario = custoIndiretoEstimado,
                CustoTotal = custoIndiretoEstimado
            });

            foreach (var item in itensMaterial)
            {
                precificacao.Itens.Add(item);
            }

            _context.Precificacoes.Add(precificacao);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPorId),
                new { id = precificacao.Id },
                precificacao);
        }

        private static decimal? CalcularCustoHoraMaquina(CustoMaquina custo)
        {
            if (custo.MetodoCalculo == "MANUAL")
            {
                if (!custo.CustoHoraManual.HasValue || custo.CustoHoraManual.Value <= 0)
                    return null;

                return custo.CustoHoraManual.Value;
            }

            if (custo.VidaUtilHoras <= 0 || custo.ValorAquisicao < custo.ValorResidual)
                return null;

            return (custo.ValorAquisicao - custo.ValorResidual) / custo.VidaUtilHoras
                + custo.ConsumoEnergiaKW * custo.CustoKWh
                + custo.CustoManutencaoHora
                + custo.OutrosCustosHora;
        }

        private static decimal CalcularCustoFifo(List<Lote> lotes, decimal necessaria)
        {
            decimal restante = necessaria;
            decimal total = 0;

            foreach (var lote in lotes)
            {
                if (restante <= 0) break;

                decimal qtd = Math.Min(restante, lote.Quantidade);
                total += Math.Round(qtd * lote.CustoUnitario, 2, MidpointRounding.AwayFromZero);
                restante -= qtd;
            }

            return total;
        }

        // PATCH: api/Precificacoes/5/aprovar
        [HttpPatch("{id:int}/aprovar")]
        public async Task<IActionResult> Aprovar(
            int id,
            [FromBody] AprovarPrecoRequest request)
        {
            if (request == null || request.PrecoAprovado <= 0)
            {
                return BadRequest(new
                {
                    mensagem = "O preço aprovado deve ser maior que zero."
                });
            }

            var precificacao = await _context.Precificacoes
                .FirstOrDefaultAsync(p => p.Id == id);

            if (precificacao == null)
            {
                return NotFound(new
                {
                    mensagem = "Precificação não encontrada."
                });
            }

            if (precificacao.Status == "APROVADA")
            {
                return BadRequest(new
                {
                    mensagem = "Esta precificação já foi aprovada."
                });
            }

            var ordemServico = await _context.OrdensServico
                .FirstOrDefaultAsync(o => o.Id == precificacao.OrdemServicoId);

            if (ordemServico == null)
            {
                return NotFound(new
                {
                    mensagem = "A Ordem de Serviço vinculada à precificação não foi encontrada."
                });
            }

            decimal precoAprovado = Math.Round(
                request.PrecoAprovado,
                2,
                MidpointRounding.AwayFromZero);

            precificacao.PrecoAprovado = precoAprovado;
            precificacao.Status = "APROVADA";

            // Atualiza o valor comercial da Ordem de Serviço.
            ordemServico.ValorTotal = precoAprovado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Preço aprovado e Ordem de Serviço atualizada com sucesso.",
                precificacao.Id,
                precificacao.OrdemServicoId,
                precificacao.PrecoSugerido,
                precificacao.PrecoAprovado,
                precificacao.Status,
                valorTotalOS = ordemServico.ValorTotal
            });
        }

    }

    public class AprovarPrecoRequest
    {
        public decimal PrecoAprovado { get; set; }
    }
}
