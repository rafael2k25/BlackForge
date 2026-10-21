using BlackForge.Data;
using BlackForge.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdensServicoController : ControllerBase
    {
        private static readonly string[] StatusValidos =
        {
            "pendente",
            "em_execucao",
            "concluida"
        };

        private readonly BlackForgeDbContext _context;

        public OrdensServicoController(BlackForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/ordensservico
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrdemServico>>> GetOrdensServico()
        {
            var ordens = await _context.OrdensServico
                .AsNoTracking()
                .Include(o => o.Funcionario)
                .OrderByDescending(o => o.DataAbertura)
                .ToListAsync();

            return Ok(ordens);
        }

        // GET: api/ordensservico/1
        [HttpGet("{id}")]
        public async Task<ActionResult<OrdemServico>> GetOrdemServico(int id)
        {
            var ordem = await _context.OrdensServico
                .AsNoTracking()
                .Include(o => o.Funcionario)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordem == null)
                return NotFound();

            return Ok(ordem);
        }

        // POST: api/ordensservico
        [HttpPost]
        public async Task<ActionResult<OrdemServico>> CriarOrdemServico(
            OrdemServico ordem)
        {
            if (string.IsNullOrWhiteSpace(ordem.NumeroOS))
                return BadRequest("O número da OS é obrigatório.");

            if (string.IsNullOrWhiteSpace(ordem.Cliente))
                return BadRequest("O cliente é obrigatório.");

            if (string.IsNullOrWhiteSpace(ordem.DescricaoServico))
                return BadRequest("A descrição do serviço é obrigatória.");

            if (string.IsNullOrWhiteSpace(ordem.TipoServico))
                return BadRequest("O tipo de serviço é obrigatório.");

            var tipoNormalizado = NormalizarTipoServico(ordem.TipoServico);

            if (string.IsNullOrEmpty(tipoNormalizado) ||
                !TiposServicoValidos.Contains(tipoNormalizado))
            {
                return BadRequest("Tipo de serviço inválido.");
            }

            ordem.TipoServico = tipoNormalizado;

            if (!string.IsNullOrWhiteSpace(ordem.CondicaoPagamento) &&
                !CondicoesPagamentoValidas.Contains(
                    ordem.CondicaoPagamento.Trim(),
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest("Condição de pagamento inválida.");
            }

            if (ordem.Quantidade <= 0)
                return BadRequest(
                    "A quantidade da OS deve ser maior que zero."
                );

            var numeroExiste = await _context.OrdensServico
                .AnyAsync(o => o.NumeroOS == ordem.NumeroOS);

            if (numeroExiste)
                return Conflict(
                    "Já existe uma ordem de serviço com esse número."
                );

            if (ordem.FuncionarioId.HasValue)
            {
                var funcionarioExiste = await _context.Funcionarios
                    .AnyAsync(f => f.Id == ordem.FuncionarioId.Value);

                if (!funcionarioExiste)
                    return BadRequest(
                        "O funcionário responsável informado não existe."
                    );
            }

            var erroMaterial = await ValidarMaterialAsync(ordem.MaterialId);

            if (erroMaterial != null)
                return BadRequest(erroMaterial);

            ordem.Material = null;
            ordem.Funcionario = null;

            if (ordem.DataAbertura == default)
                ordem.DataAbertura = DateTime.Now;

            ordem.ValorMateriais = 0;
            ordem.ValorMaoObra = 0;
            ordem.Desconto = 0;
            ordem.ValorTotal = 0;

            ordem.Status = "pendente";
            ordem.DataConclusao = null;

            _context.OrdensServico.Add(ordem);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetOrdemServico),
                new { id = ordem.Id },
                ordem
            );
        }

        // PUT: api/ordensservico/1
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarOrdemServico(
            int id,
            OrdemServico ordem)
        {
            if (id != ordem.Id)
                return BadRequest(
                    "O ID da URL não corresponde ao ID da ordem."
                );

            var ordemExistente = await _context.OrdensServico
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordemExistente == null)
                return NotFound();

            if (ordemExistente.Status == "concluida")
                return Conflict(
                    "Uma OS concluída não pode ser editada. Reabra a OS antes de alterar."
                );

            if (string.IsNullOrWhiteSpace(ordem.NumeroOS))
                return BadRequest("O número da OS é obrigatório.");

            if (string.IsNullOrWhiteSpace(ordem.Cliente))
                return BadRequest("O cliente é obrigatório.");

            if (string.IsNullOrWhiteSpace(ordem.DescricaoServico))
                return BadRequest("A descrição do serviço é obrigatória.");

            if (string.IsNullOrWhiteSpace(ordem.TipoServico))
                return BadRequest("O tipo de serviço é obrigatório.");

            var tipoNormalizado = NormalizarTipoServico(ordem.TipoServico);

            if (string.IsNullOrEmpty(tipoNormalizado) ||
                !TiposServicoValidos.Contains(tipoNormalizado))
            {
                return BadRequest("Tipo de serviço inválido.");
            }

            ordem.TipoServico = tipoNormalizado;

            if (!string.IsNullOrWhiteSpace(ordem.CondicaoPagamento) &&
                !CondicoesPagamentoValidas.Contains(
                    ordem.CondicaoPagamento.Trim(),
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest("Condição de pagamento inválida.");
            }

            if (ordem.Quantidade <= 0)
                return BadRequest(
                    "A quantidade da OS deve ser maior que zero."
                );

            if (ordemExistente.Status == "em_execucao")
            {
                var tipoExistenteNormalizado =
                    NormalizarTipoServico(ordemExistente.TipoServico);

                var tipoAlterado = tipoNormalizado != tipoExistenteNormalizado;

                if (ordem.Quantidade != ordemExistente.Quantidade ||
                    tipoAlterado ||
                    ordem.MaterialId != ordemExistente.MaterialId)
                {
                    return Conflict(
                        "Não é possível alterar a quantidade, o tipo de serviço " +
                        "ou o material de uma OS em execução. " +
                        "Volte a OS para pendente antes de alterar."
                    );
                }
            }

            var numeroExiste = await _context.OrdensServico
                .AnyAsync(o =>
                    o.NumeroOS == ordem.NumeroOS &&
                    o.Id != id);

            if (numeroExiste)
                return Conflict(
                    "Já existe outra ordem de serviço com esse número."
                );

            if (ordem.FuncionarioId.HasValue)
            {
                var funcionarioExiste = await _context.Funcionarios
                    .AnyAsync(f => f.Id == ordem.FuncionarioId.Value);

                if (!funcionarioExiste)
                    return BadRequest(
                        "O funcionário responsável informado não existe."
                    );
            }

            var erroMaterial = await ValidarMaterialAsync(ordem.MaterialId);

            if (erroMaterial != null)
                return BadRequest(erroMaterial);
            ordemExistente.NumeroOS = ordem.NumeroOS;
            ordemExistente.Cliente = ordem.Cliente;
            ordemExistente.Contato = ordem.Contato;
            ordemExistente.Endereco = ordem.Endereco;
            ordemExistente.DataAbertura = ordem.DataAbertura;
            ordemExistente.DescricaoServico = ordem.DescricaoServico;
            ordemExistente.TipoServico = ordem.TipoServico;
            ordemExistente.Quantidade = ordem.Quantidade;
            ordemExistente.MaterialId = ordem.MaterialId;
            ordemExistente.DataEntrega = ordem.DataEntrega;
            ordemExistente.FuncionarioId = ordem.FuncionarioId;
            ordemExistente.CondicaoPagamento = ordem.CondicaoPagamento;
            ordemExistente.Observacoes = ordem.Observacoes;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/ordensservico/1/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> AlterarStatus(
            int id,
            [FromBody] AlterarStatusOSDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
                return BadRequest("Informe o status da OS.");

            var status = dto.Status.Trim().ToLowerInvariant();

            if (!StatusValidos.Contains(status))
                return BadRequest("Status inválido.");

            var ordem = await _context.OrdensServico
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordem == null)
                return NotFound("Ordem de serviço não encontrada.");

            if (ordem.Status == "concluida" && status != "concluida")
            {
                return Conflict(
                    "Uma OS concluída não pode voltar para outro status."
                );
            }

            var processosAbertos = await _context.ProcessosProducao
                .Where(p =>
                    p.OrdemServicoId == id &&
                    (p.Status == "EM_EXECUCAO" || p.Status == "PAUSADO"))
                .ToListAsync();

            var processoAtivo = processosAbertos
                .FirstOrDefault(p => p.Status == "EM_EXECUCAO");

            var processoPausado = processosAbertos
                .Where(p => p.Status == "PAUSADO")
                .OrderByDescending(p => p.Id)
                .FirstOrDefault();

            string? maquinaNome = null;
            int? maquinaIdSelecionada = null;

            if (status == "concluida")
            {
                if (ordem.Status != "concluida")
                {
                    return Conflict(
                        "A OS não pode ser concluída manualmente. " +
                        "A produção automática deve finalizar o processo.");
                }

                return Ok(new
                {
                    ordem.Id,
                    ordem.Status,
                    ordem.DataConclusao,
                    ordem.Quantidade,
                    MaquinaId = (int?)null,
                    MaquinaNome = (string?)null
                });
            }

            if (status == "em_execucao")
            {
                if (ordem.Status == "concluida")
                    return Conflict("Uma OS concluída não pode voltar para execução.");

                if (ordem.Quantidade <= 0)
                    return BadRequest("A quantidade da OS deve ser maior que zero.");

                if (processoAtivo != null)
                {
                    maquinaIdSelecionada = processoAtivo.MaquinaId;
                    maquinaNome = await ObterNomeMaquinaAsync(processoAtivo.MaquinaId);
                }

                else if (processoPausado != null)
                {
                    if (processoPausado.QuantidadePlanejada <= 0 ||
                        processoPausado.ProducaoPorMinuto <= 0 ||
                        processoPausado.ConsumoPorUnidade < 0 ||
                        processoPausado.QuantidadeProduzida < 0 ||
                        processoPausado.QuantidadeProduzida >
                            processoPausado.QuantidadePlanejada ||
                        processoPausado.TempoProducaoSegundos < 0 ||
                        processoPausado.QuantidadeConsumidaRegistrada < 0)
                    {
                        return Conflict(
                            "O processo pausado possui parâmetros inválidos. " +
                            "Corrija os dados antes de retomar a produção.");
                    }

                    var maquina = await _context.Maquinas
                        .FirstOrDefaultAsync(
                            m => m.Id == processoPausado.MaquinaId);

                    if (maquina == null)
                    {
                        return Conflict(
                            "A máquina do processo pausado não foi encontrada.");
                    }

                    var maquinaOcupada = await _context.ProcessosProducao
                        .AnyAsync(p =>
                            p.Status == "EM_EXECUCAO" &&
                            p.MaquinaId == processoPausado.MaquinaId);

                    if (maquinaOcupada)
                    {
                        return Conflict(
                            "A máquina do processo pausado está ocupada por outra OS.");
                    }

                    var tipoServicoOS =
                        NormalizarTipoServico(ordem.TipoServico);

                    var configuracoesAtivas = await _context.ConfiguracoesMaquina
                        .Where(c =>
                            c.MaquinaId == processoPausado.MaquinaId &&
                            c.Ativa)
                        .ToListAsync();

                    var configuracaoCompativel = configuracoesAtivas
                        .FirstOrDefault(c =>
                            NormalizarTipoServico(c.TipoServico) == tipoServicoOS);

                    if (configuracaoCompativel == null)
                    {
                        return Conflict(
                            "A máquina não possui configuração ativa compatível " +
                            "com o tipo de serviço desta OS.");
                    }

                    if (configuracaoCompativel.ProducaoPorMinuto <= 0 ||
                        configuracaoCompativel.ConsumoPorUnidade < 0)
                    {
                        return Conflict(
                            "A configuração da máquina possui parâmetros inválidos.");
                    }

                    if (processoPausado.ConsumoPorUnidade > 0)
                    {
                        if (!processoPausado.MaterialId.HasValue)
                        {
                            return Conflict(
                                "O processo não possui material configurado.");
                        }

                        var materialExiste = await _context.Materiais
                            .AnyAsync(m =>
                                m.Id == processoPausado.MaterialId.Value);

                        if (!materialExiste)
                        {
                            return Conflict(
                                "O material do processo pausado não existe mais.");
                        }
                    }
                    processoPausado.Status = "EM_EXECUCAO";
                    processoPausado.DataFim = null;
                    processoPausado.UltimaAtualizacaoProducao = DateTime.Now;
                    maquinaIdSelecionada = maquina.Id;
                    maquinaNome = maquina.Nome;
                }

                else
                {
                    var maquinasOcupadas = _context.ProcessosProducao
                        .Where(p => p.Status == "EM_EXECUCAO")
                        .Select(p => p.MaquinaId);

                    var tipoServicoOS = NormalizarTipoServico(ordem.TipoServico);

                    if (string.IsNullOrEmpty(tipoServicoOS))
                        return BadRequest("O tipo de serviço da OS é inválido.");

                    var configuracoesAtivas = await _context.ConfiguracoesMaquina
                        .Include(c => c.Maquina)
                        .Where(c =>
                            c.Ativa &&
                            c.Maquina != null &&
                            !maquinasOcupadas.Contains(c.MaquinaId))
                        .OrderBy(c => c.Id)
                        .ToListAsync();

                    var configuracaoSelecionada = configuracoesAtivas
                        .FirstOrDefault(c =>
                            NormalizarTipoServico(c.TipoServico) == tipoServicoOS);

                    if (configuracaoSelecionada == null)
                    {
                        return Conflict(
                            "Não existe máquina livre com configuração ativa " +
                            $"compatível com o serviço '{tipoServicoOS}'.");
                    }

                    if (configuracaoSelecionada.ProducaoPorMinuto <= 0)
                        return Conflict("Configure uma produção por minuto maior que zero.");

                    var consumoPorUnidade = configuracaoSelecionada.ConsumoPorUnidade;

                    if (consumoPorUnidade < 0)
                        return Conflict("O consumo por unidade não pode ser negativo.");

                    int? materialId = ordem.MaterialId ?? configuracaoSelecionada.MaterialId;

                    if (consumoPorUnidade > 0 && materialId == null)
                    {
                        return Conflict(
                            "Não é possível iniciar a produção: defina o material na OS " +
                            "ou na configuração da máquina.");
                    }

                    var maquina = configuracaoSelecionada.Maquina;

                    maquinaIdSelecionada = maquina.Id;
                    maquinaNome = maquina.Nome;

                    var agora = DateTime.Now;

                    _context.ProcessosProducao.Add(new ProcessoProducao
                    {
                        OrdemServicoId = ordem.Id,
                        MaquinaId = maquina.Id,
                        MaterialId = materialId,

                        DataInicio = agora,
                        UltimaAtualizacaoProducao = agora,

                        QuantidadePlanejada = ordem.Quantidade,
                        QuantidadeProduzida = 0,

                        ProducaoPorMinuto = configuracaoSelecionada.ProducaoPorMinuto,
                        ConsumoPorUnidade = consumoPorUnidade,

                        MaterialConsumido = 0,
                        QuantidadeConsumidaRegistrada = 0,
                        Status = "EM_EXECUCAO"
                    });
                }
            }
            else if (status == "pendente")
            {
                foreach (var processo in processosAbertos)
                {
                    processo.DataFim = DateTime.Now;
                    processo.Status = "CANCELADO";

                    maquinaIdSelecionada = processo.MaquinaId;
                    maquinaNome ??= await ObterNomeMaquinaAsync(processo.MaquinaId);
                }
            }

            ordem.Status = status;
            ordem.DataConclusao = null;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.GetBaseException() is SqlException sqlEx &&
                    (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    return Conflict(
                        "A operação não pôde ser concluída porque a OS ou a máquina " +
                        "já possui um processo ativo. Atualize os dados e tente novamente."
                    );
                }
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro ao salvar a alteração do status da OS."
                );
            }

            return Ok(new
            {
                ordem.Id,
                ordem.Status,
                ordem.DataConclusao,
                ordem.Quantidade,
                MaquinaId = maquinaIdSelecionada,
                MaquinaNome = maquinaNome
            });
        }

        private Task<string?> ObterNomeMaquinaAsync(int maquinaId) =>
            _context.Maquinas
                .Where(m => m.Id == maquinaId)
                .Select(m => (string?)m.Nome)
                .FirstOrDefaultAsync();

        // DELETE: api/ordensservico/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirOrdemServico(int id)
        {
            var ordem = await _context.OrdensServico
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordem == null)
                return NotFound("Ordem de serviço não encontrada.");

            var processos = await _context.ProcessosProducao
                .Where(p => p.OrdemServicoId == id)
                .ToListAsync();

            if (processos.Any(p => p.Status == "EM_EXECUCAO"))
                return Conflict(
                    "A OS está em execução. Volte a OS para pendente antes de excluir."
                );

            var processosIds = processos.Select(p => p.Id).ToList();

            if (processosIds.Count > 0)
            {
                var consumiuEstoque = await _context.Movimentacoes
                    .AnyAsync(m =>
                        m.ProcessoProducaoId != null &&
                        processosIds.Contains(m.ProcessoProducaoId.Value));

                if (consumiuEstoque)
                    return Conflict(
                        "Esta OS já consumiu material do estoque e não pode ser " +
                        "excluída, pois o histórico de estoque ficaria incoerente."
                    );

                _context.ProcessosProducao.RemoveRange(processos);
            }

            _context.OrdensServico.Remove(ordem);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Registros ligados à OS (ex.: precificações) impedem a exclusão.
                return Conflict(
                    "Esta OS possui registros vinculados (como precificações) " +
                    "e não pode ser excluída."
                );
            }

            return NoContent();
        }

        private async Task<string?> ValidarMaterialAsync(int? materialId)
        {
            if (!materialId.HasValue)
                return null;

            var existe = await _context.Materiais
                .AnyAsync(m => m.Id == materialId.Value);

            return existe
                ? null
                : "O material informado não existe.";
        }

        private static readonly string[] CondicoesPagamentoValidas =
        {
            "avista", "pix", "cartao", "boleto", "30dias", "parcelado",
            "50% na entrada e 50% na entrega", "outro"
        };

        private static readonly string[] TiposServicoValidos =
        {
            "usinagem",
            "torneamento",
            "fresagem",
            "corte",
            "solda",
            "acabamento",
            "conformacao",
            "furacao",
            "retifica",
            "outro"
        };

        private static string NormalizarTipoServico(string? tipo)
        {
            var valor = tipo?.Trim().ToLowerInvariant() ?? "";

            return valor switch
            {
                "usinagem" => "usinagem",
                "torneamento" or "torneamento cnc" => "torneamento",
                "fresagem" or "fresagem cnc" => "fresagem",
                "corte" or "corte cnc" => "corte",
                "solda" or "soldagem" or "solda cnc" => "solda",
                "acabamento" => "acabamento",
                "conformacao" or "conformação" => "conformacao",
                "furacao" or "furação" => "furacao",
                "retifica" or "retífica" => "retifica",
                "outro" => "outro",
                _ => ""
            };
        }
    }
}