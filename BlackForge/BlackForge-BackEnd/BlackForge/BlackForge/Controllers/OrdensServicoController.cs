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

        private const int ProducaoPorMinutoPadrao = 10;
        private const decimal ConsumoPorUnidadePadrao = 1m;

        private static readonly string[] StatusValidos =
        {
    "pendente",
    "em_execucao",
    "concluida"
};

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> AlterarStatus(
            int id,
            [FromBody] AlterarStatusOSDto dto)
        {
            var status = dto.Status?.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(status) ||
                !StatusValidos.Contains(status))
            {
                return BadRequest("Status inválido.");
            }

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

            if (status == "em_execucao")
            {

                if (ordem.Quantidade <= 0)
                {
                    return BadRequest(
                        "A quantidade da OS deve ser maior que zero."
                    );
                }

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

                    var tipoServicoOS = ordem.TipoServico
                        .Trim()
                        .ToLowerInvariant();

                    var maquinaComConfiguracao =
                        await _context.ConfiguracoesMaquina
                            .Include(c => c.Maquina)
                            .Where(c =>
                                c.Ativa &&
                                c.Maquina != null &&
                                !maquinasOcupadas.Contains(c.MaquinaId))
                            .OrderBy(c => c.Id)
                            .ToListAsync();

                    ConfiguracaoMaquina? configuracaoSelecionada = null;

                    configuracaoSelecionada =
                        maquinaComConfiguracao
                            .FirstOrDefault(c =>
                                c.TipoServico != null &&
                                c.TipoServico
                                    .Trim()
                                    .ToLowerInvariant()
                                    == tipoServicoOS);

                    if (configuracaoSelecionada == null)
                    {
                        configuracaoSelecionada =
                            maquinaComConfiguracao.FirstOrDefault();
                    }

                    if (configuracaoSelecionada == null ||
                        configuracaoSelecionada.Maquina == null)
                    {
                        return Conflict(
                            "Nenhuma máquina livre e configurada está disponível no momento. " +
                            "Finalize um processo em andamento ou cadastre uma configuração de máquina."
                        );
                    }

                    var maquina = configuracaoSelecionada.Maquina;

                    maquinaIdSelecionada = maquina.Id;
                    maquinaNome = maquina.Nome;

                    var producaoPorMinuto =
                        configuracaoSelecionada.ProducaoPorMinuto > 0
                            ? configuracaoSelecionada.ProducaoPorMinuto
                            : ProducaoPorMinutoPadrao;

                    var consumoPorUnidade =
                        configuracaoSelecionada.ConsumoPorUnidade > 0
                            ? configuracaoSelecionada.ConsumoPorUnidade
                            : ConsumoPorUnidadePadrao;

                    var processo = new ProcessoProducao
                    {
                        OrdemServicoId = ordem.Id,

                        MaquinaId = maquina.Id,

                        DataInicio = DateTime.Now,

                        QuantidadePlanejada = ordem.Quantidade,

                        QuantidadeProduzida = 0,
                     
                        ProducaoPorMinuto = producaoPorMinuto,

                        ConsumoPorUnidade = consumoPorUnidade,

                        MaterialConsumido = 0,

                        Status = "EM_EXECUCAO"
                    };

                    _context.ProcessosProducao.Add(processo);
                }
            }

            else if (status == "concluida")
            {
                foreach (var processo in processosAtivos)
                {
                    processo.QuantidadeProduzida =
                        processo.QuantidadePlanejada;

                    processo.MaterialConsumido =
                        processo.QuantidadeProduzida *
                        processo.ConsumoPorUnidade;

                    processo.DataFim = DateTime.Now;

                    processo.Status = "CONCLUIDO";
                 
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

            else if (status == "pendente")
            {
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

            ordem.DataConclusao =
                status == "concluida"
                    ? DateTime.Now
                    : null;

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

            if (processos.Any())
            {
                _context.ProcessosProducao.RemoveRange(processos);
            }

            _context.OrdensServico.Remove(ordem);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}