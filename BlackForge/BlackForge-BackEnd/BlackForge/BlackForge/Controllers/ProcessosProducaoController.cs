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

        // GET: api/ProcessosProducao
        // Retorna todos os processos de produção
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProcessoProducaoDTO>>> GetProcessos()
        {
            var processos = await _context.ProcessosProducao
                .AsNoTracking()
                .Include(p => p.Maquina)
                .Include(p => p.OrdemServico)
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

                    Status = p.Status
                })
                .ToListAsync();

            return Ok(processos);
        }

        // GET: api/ProcessosProducao/5
        // Retorna um processo específico
        [HttpGet("{id}")]
        public async Task<ActionResult<ProcessoProducaoDTO>> GetProcesso(int id)
        {
            var processo = await _context.ProcessosProducao
                .AsNoTracking()
                .Include(p => p.Maquina)
                .Include(p => p.OrdemServico)
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

                    Status = p.Status
                })
                .FirstOrDefaultAsync();

            if (processo == null)
                return NotFound("Processo de produção não encontrado.");

            return Ok(processo);
        }

        // GET: api/ProcessosProducao/maquina/1
        // Retorna o processo atualmente executado por uma máquina
        [HttpGet("maquina/{maquinaId}")]
        public async Task<ActionResult<ProcessoProducaoDTO>> GetProcessoAtivoDaMaquina(
            int maquinaId)
        {
            var maquinaExiste = await _context.Maquinas
                .AnyAsync(m => m.Id == maquinaId);

            if (!maquinaExiste)
                return NotFound("Máquina não encontrada.");

            var processo = await _context.ProcessosProducao
                .AsNoTracking()
                .Include(p => p.Maquina)
                .Include(p => p.OrdemServico)
                .Where(p =>
                    p.MaquinaId == maquinaId &&
                    p.Status == "EM_EXECUCAO")
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

                    Status = p.Status
                })
                .FirstOrDefaultAsync();

            if (processo == null)
                return NotFound(
                    "Nenhum processo em execução nesta máquina.");

            return Ok(processo);
        }

        // GET: api/ProcessosProducao/ativos
        // Retorna todos os processos atualmente em execução
        [HttpGet("ativos")]
        public async Task<ActionResult<IEnumerable<ProcessoProducaoDTO>>> GetProcessosAtivos()
        {
            var processos = await _context.ProcessosProducao
                .AsNoTracking()
                .Include(p => p.Maquina)
                .Include(p => p.OrdemServico)
                .Where(p => p.Status == "EM_EXECUCAO")
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

                    Status = p.Status
                })
                .ToListAsync();

            return Ok(processos);
        }
    }
}