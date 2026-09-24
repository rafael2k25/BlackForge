using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessosProducaoController : ControllerBase
    {
        private readonly BlackForgeDbContext _context;

        public ProcessosProducaoController(BlackForgeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProcessoProducaoDTO>>> GetProcessos()
        {
            var processos = await _context.ProcessosProducao
                .Include(p => p.Maquina)
                .Include(p => p.OrdemServico)
                .AsNoTracking()
                .Select(p => new ProcessoProducaoDTO
                {
                    Id = p.Id,

                    OrdemServicoId = p.OrdemServicoId,
                    NumeroOS = p.OrdemServico.NumeroOS,
                    Cliente = p.OrdemServico.Cliente,
                    TipoServico = p.OrdemServico.TipoServico,

                    MaquinaId = p.MaquinaId,
                    MaquinaNome = p.Maquina.Nome,
                    MaquinaCodigo = p.Maquina.Codigo,

                    DataInicio = p.DataInicio,
                    DataFim = p.DataFim,

                    QuantidadePlanejada = p.QuantidadePlanejada,
                    QuantidadeProduzida = p.QuantidadeProduzida,

                    ProducaoPorMinuto = p.ProducaoPorMinuto,
                    ConsumoPorUnidade = p.ConsumoPorUnidade,
                    MaterialConsumido = p.MaterialConsumido,

                    Status = p.Status,
                    Observacoes = p.Observacoes
                })
                .ToListAsync();

            return Ok(processos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProcessoProducaoDTO>> GetProcesso(int id)
        {
            var processo = await _context.ProcessosProducao
                .Include(p => p.Maquina)
                .Include(p => p.OrdemServico)
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProcessoProducaoDTO
                {
                    Id = p.Id,

                    OrdemServicoId = p.OrdemServicoId,
                    NumeroOS = p.OrdemServico.NumeroOS,
                    Cliente = p.OrdemServico.Cliente,
                    TipoServico = p.OrdemServico.TipoServico,

                    MaquinaId = p.MaquinaId,
                    MaquinaNome = p.Maquina.Nome,
                    MaquinaCodigo = p.Maquina.Codigo,

                    DataInicio = p.DataInicio,
                    DataFim = p.DataFim,

                    QuantidadePlanejada = p.QuantidadePlanejada,
                    QuantidadeProduzida = p.QuantidadeProduzida,

                    ProducaoPorMinuto = p.ProducaoPorMinuto,
                    ConsumoPorUnidade = p.ConsumoPorUnidade,
                    MaterialConsumido = p.MaterialConsumido,

                    Status = p.Status,
                    Observacoes = p.Observacoes
                })
                .FirstOrDefaultAsync();

            if (processo == null)
                return NotFound("Processo de produção não encontrado.");

            return Ok(processo);
        }

        [HttpGet("maquina/{maquinaId}")]
        public async Task<ActionResult<ProcessoProducaoDTO>> GetProcessoAtivoDaMaquina(int maquinaId)
        {
            var maquinaExiste = await _context.Maquinas
                .AnyAsync(m => m.Id == maquinaId);

            if (!maquinaExiste)
                return NotFound("Máquina não encontrada.");

            var processo = await _context.ProcessosProducao
                .Include(p => p.Maquina)
                .Include(p => p.OrdemServico)
                .Where(p =>
                    p.MaquinaId == maquinaId &&
                    p.Status == "EM_EXECUCAO")
                .AsNoTracking()
                .Select(p => new ProcessoProducaoDTO
                {
                    Id = p.Id,

                    OrdemServicoId = p.OrdemServicoId,
                    NumeroOS = p.OrdemServico.NumeroOS,
                    Cliente = p.OrdemServico.Cliente,
                    TipoServico = p.OrdemServico.TipoServico,

                    MaquinaId = p.MaquinaId,
                    MaquinaNome = p.Maquina.Nome,
                    MaquinaCodigo = p.Maquina.Codigo,

                    DataInicio = p.DataInicio,
                    DataFim = p.DataFim,

                    QuantidadePlanejada = p.QuantidadePlanejada,
                    QuantidadeProduzida = p.QuantidadeProduzida,

                    ProducaoPorMinuto = p.ProducaoPorMinuto,
                    ConsumoPorUnidade = p.ConsumoPorUnidade,
                    MaterialConsumido = p.MaterialConsumido,

                    Status = p.Status,
                    Observacoes = p.Observacoes
                })
                .FirstOrDefaultAsync();

            if (processo == null)
                return NotFound("Nenhum processo em execução nesta máquina.");

            return Ok(processo);
        }

        [HttpGet("ativos")]
        public async Task<ActionResult<IEnumerable<ProcessoProducaoDTO>>> GetProcessosAtivos()
        {
            var processos = await _context.ProcessosProducao
                .Include(p => p.Maquina)
                .Include(p => p.OrdemServico)
                .Where(p => p.Status == "EM_EXECUCAO")
                .AsNoTracking()
                .Select(p => new ProcessoProducaoDTO
                {
                    Id = p.Id,

                    OrdemServicoId = p.OrdemServicoId,
                    NumeroOS = p.OrdemServico.NumeroOS,
                    Cliente = p.OrdemServico.Cliente,
                    TipoServico = p.OrdemServico.TipoServico,

                    MaquinaId = p.MaquinaId,
                    MaquinaNome = p.Maquina.Nome,
                    MaquinaCodigo = p.Maquina.Codigo,

                    DataInicio = p.DataInicio,
                    DataFim = p.DataFim,

                    QuantidadePlanejada = p.QuantidadePlanejada,
                    QuantidadeProduzida = p.QuantidadeProduzida,

                    ProducaoPorMinuto = p.ProducaoPorMinuto,
                    ConsumoPorUnidade = p.ConsumoPorUnidade,
                    MaterialConsumido = p.MaterialConsumido,

                    Status = p.Status,
                    Observacoes = p.Observacoes
                })
                .ToListAsync();

            return Ok(processos);
        }
      
        [HttpPut("{id}/finalizar")]
        public async Task<IActionResult> FinalizarProcesso(
            int id,
            [FromBody] FinalizarProcessoRequest request)
        {
            var processo = await _context.ProcessosProducao
                .FirstOrDefaultAsync(p => p.Id == id);

            if (processo == null)
                return NotFound("Processo de produção não encontrado.");

            if (processo.Status != "EM_EXECUCAO")
                return BadRequest(
                    "Somente processos em execução podem ser finalizados."
                );

            if (request.QuantidadeProduzida < 0)
                return BadRequest(
                    "A quantidade produzida não pode ser negativa."
                );

            if (request.QuantidadeProduzida > processo.QuantidadePlanejada)
                return BadRequest(
                    "A quantidade produzida não pode ser maior que a quantidade planejada."
                );

            processo.QuantidadeProduzida =
                request.QuantidadeProduzida;

            processo.DataFim = DateTime.Now;

            processo.MaterialConsumido =
                processo.QuantidadeProduzida *
                processo.ConsumoPorUnidade;

            processo.Status = "CONCLUIDO";

            await _context.SaveChangesAsync();

            return Ok(processo);
        }
    }

    public class FinalizarProcessoRequest
    {
        public int QuantidadeProduzida { get; set; }
    }
}