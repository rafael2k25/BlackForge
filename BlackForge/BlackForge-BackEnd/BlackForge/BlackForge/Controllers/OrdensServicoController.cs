using BlackForge.Data;
using BlackForge.Models;
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

            // Só o MaterialId importa; evita o EF tentar inserir um Material.
            ordem.Material = null;

            if (ordem.DataAbertura == default)
                ordem.DataAbertura = DateTime.Now;

            if (ordem.ValorMateriais < 0)
                return BadRequest(
                    "O valor dos materiais não pode ser negativo."
                );

            if (ordem.ValorMaoObra < 0)
                return BadRequest(
                    "O valor da mão de obra não pode ser negativo."
                );

            if (ordem.Desconto < 0)
                return BadRequest(
                    "O desconto não pode ser negativo."
                );

            ordem.ValorTotal =
                ordem.ValorMateriais +
                ordem.ValorMaoObra -
                ordem.Desconto;

            if (ordem.ValorTotal < 0)
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

            if (ordem.Quantidade <= 0)
                return BadRequest(
                    "A quantidade da OS deve ser maior que zero."
                );

            // O processo de produção copia quantidade, serviço e material
            // ao iniciar. Alterar esses campos durante a execução deixaria
            // a OS e o processo com valores diferentes.
            if (ordemExistente.Status == "em_execucao")
            {
                var tipoAlterado = !string.Equals(
                    ordem.TipoServico.Trim(),
                    ordemExistente.TipoServico?.Trim(),
                    StringComparison.OrdinalIgnoreCase);

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

            if (ordem.ValorMateriais < 0)
                return BadRequest(
                    "O valor dos materiais não pode ser negativo."
                );

            if (ordem.ValorMaoObra < 0)
                return BadRequest(
                    "O valor da mão de obra não pode ser negativo."
                );

            if (ordem.Desconto < 0)
                return BadRequest(
                    "O desconto não pode ser negativo."
                );

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
            ordemExistente.ValorMateriais = ordem.ValorMateriais;
            ordemExistente.ValorMaoObra = ordem.ValorMaoObra;
            ordemExistente.Desconto = ordem.Desconto;
            ordemExistente.CondicaoPagamento = ordem.CondicaoPagamento;
            ordemExistente.Observacoes = ordem.Observacoes;

            ordemExistente.ValorTotal =
                ordemExistente.ValorMateriais +
                ordemExistente.ValorMaoObra -
                ordemExistente.Desconto;

            if (ordemExistente.ValorTotal < 0)
                ordemExistente.ValorTotal = 0;

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

            var processosAtivos = await _context.ProcessosProducao
                .Where(p =>
                    p.OrdemServicoId == id &&
                    p.Status == "EM_EXECUCAO")
                .ToListAsync();

            string? maquinaNome = null;
            int? maquinaIdSelecionada = null;

            // A OS só pode ser concluída automaticamente pelo serviço.
            if (status == "concluida")
            {
                if (ordem.Status != "concluida")
                {
                    return Conflict(
                        "A OS não pode ser concluída manualmente. " +
                        "A produção automática deve finalizar o processo."
                    );
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
                {
                    return Conflict(
                        "Uma OS concluída não pode voltar para execução."
                    );
                }

                if (ordem.Quantidade <= 0)
                {
                    return BadRequest(
                        "A quantidade da OS deve ser maior que zero."
                    );
                }

                // Reutiliza o processo existente, evitando duplicação.
                if (processosAtivos.Count > 0)
                {
                    var processoExistente = processosAtivos[0];

                    maquinaIdSelecionada = processoExistente.MaquinaId;

                    maquinaNome = await _context.Maquinas
                        .Where(m => m.Id == processoExistente.MaquinaId)
                        .Select(m => m.Nome)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    var maquinasOcupadas = _context.ProcessosProducao
                        .Where(p => p.Status == "EM_EXECUCAO")
                        .Select(p => p.MaquinaId);

                    var tipoServicoOS = ordem.TipoServico.Trim();

                    // Só seleciona configurações do tipo de serviço correto.
                    var configuracaoSelecionada =
                        await _context.ConfiguracoesMaquina
                            .Include(c => c.Maquina)
                            .Where(c =>
                                c.Ativa &&
                                c.Maquina != null &&
                                !maquinasOcupadas.Contains(c.MaquinaId) &&
                                c.TipoServico.Trim().ToLower() ==
                                    tipoServicoOS.ToLower())
                            .OrderBy(c => c.Id)
                            .FirstOrDefaultAsync();

                    if (configuracaoSelecionada == null)
                    {
                        return Conflict(
                            "Não existe máquina livre com configuração ativa " +
                            "compatível com o serviço '" + tipoServicoOS + "'."
                        );
                    }

                    if (configuracaoSelecionada.ProducaoPorMinuto <= 0)
                    {
                        return Conflict(
                            "Configure uma produção por minuto maior que zero."
                        );
                    }

                    var consumoPorUnidade =
                        configuracaoSelecionada.ConsumoPorUnidade;

                    if (consumoPorUnidade < 0)
                    {
                        return Conflict(
                            "O consumo por unidade não pode ser negativo."
                        );
                    }

                    // O material vem da OS. O material da configuração da
                    // máquina é apenas um valor padrão opcional.
                    var materialId =
                        ordem.MaterialId ??
                        configuracaoSelecionada.MaterialId;

                    if (consumoPorUnidade > 0 && !materialId.HasValue)
                    {
                        return Conflict(
                            "Selecione o material da OS antes de iniciar a produção."
                        );
                    }

                    if (materialId.HasValue)
                    {
                        var material = await _context.Materiais
                            .AsNoTracking()
                            .FirstOrDefaultAsync(m =>
                                m.Id == materialId.Value);

                        if (material == null)
                        {
                            return Conflict(
                                "O material da OS não foi encontrado."
                            );
                        }

                        // Confere o estoque total antes de iniciar, para a OS
                        // não ficar "em execução" sem conseguir terminar.
                        var hoje = DateTime.Today;

                        var saldo = await _context.Lotes
                            .Where(l =>
                                l.MaterialId == materialId.Value &&
                                l.Quantidade > 0 &&
                                (l.DataValidade == null ||
                                 l.DataValidade >= hoje))
                            .SumAsync(l => (decimal?)l.Quantidade) ?? 0m;

                        var necessario = ordem.Quantidade * consumoPorUnidade;

                        if (saldo < necessario)
                        {
                            return Conflict(
                                $"Estoque insuficiente de '{material.Nome}'. " +
                                $"Necessário: {necessario:0.####} {material.Unidade}; " +
                                $"disponível: {saldo:0.####} {material.Unidade}."
                            );
                        }
                    }

                    var maquina = configuracaoSelecionada.Maquina;

                    maquinaIdSelecionada = maquina.Id;
                    maquinaNome = maquina.Nome;

                    var processo = new ProcessoProducao
                    {
                        OrdemServicoId = ordem.Id,
                        MaquinaId = maquina.Id,
                        MaterialId = materialId,

                        DataInicio = DateTime.Now,
                        QuantidadePlanejada = ordem.Quantidade,
                        QuantidadeProduzida = 0,

                        ProducaoPorMinuto =
                            configuracaoSelecionada.ProducaoPorMinuto,

                        ConsumoPorUnidade = consumoPorUnidade,

                        MaterialConsumido = 0,
                        QuantidadeConsumidaRegistrada = 0,
                        Status = "EM_EXECUCAO"
                    };

                    _context.ProcessosProducao.Add(processo);
                }
            }
            else if (status == "pendente")
            {
                // Cancela processos ativos, preservando o histórico.
                foreach (var processo in processosAtivos)
                {
                    processo.DataFim = DateTime.Now;
                    processo.Status = "CANCELADO";

                    maquinaIdSelecionada = processo.MaquinaId;

                    if (string.IsNullOrEmpty(maquinaNome))
                    {
                        maquinaNome = await _context.Maquinas
                            .Where(m => m.Id == processo.MaquinaId)
                            .Select(m => m.Nome)
                            .FirstOrDefaultAsync();
                    }
                }
            }

            ordem.Status = status;
            ordem.DataConclusao = null;

            await _context.SaveChangesAsync();

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

            // Movimentações de estoque ligadas aos processos fazem parte
            // do histórico e não podem ficar sem o processo de origem.
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

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ================= AUXILIARES =================

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
    }
}