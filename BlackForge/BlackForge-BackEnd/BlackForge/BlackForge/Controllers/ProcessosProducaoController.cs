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

        [HttpGet("debug-sql")]
        public IActionResult DebugSql()
        {
            var query = _context.ProcessosProducao
                .Include(p => p.Maquina)
                .Include(p => p.OrdemServico)
                .AsNoTracking();

            var sql = query.ToQueryString();

            return Ok(sql);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProcessoProducao>>> GetProcessos()
        {
            var processos = await _context.ProcessosProducao
                .Include(p => p.Maquina)
                .Include(p => p.OrdemServico)
                .AsNoTracking()
                .ToListAsync();
            return Ok(processos);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ProcessoProducao>> GetProcesso(int id)
        {
            var processo = await _context.ProcessosProducao
                .Include(p => p.Maquina)
                .Include(p => p.OrdemServico)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            if (processo == null)
                return NotFound("Processo de produção não encontrado.");
            return Ok(processo);
        }
        [HttpGet("maquina/{maquinaId}")]
        public async Task<ActionResult<ProcessoProducao>> GetProcessoAtivoDaMaquina(
            int maquinaId)
        {
            var maquinaExiste = await _context.Maquinas
                .AnyAsync(m => m.Id == maquinaId);
            if (!maquinaExiste)
                return NotFound("Máquina não encontrada.");
            var processo = await _context.ProcessosProducao
                .Include(p => p.Maquina)
                .Include(p => p.OrdemServico)
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.MaquinaId == maquinaId &&
                    p.Status == "EM_EXECUCAO");
            if (processo == null)
                return NotFound("Nenhum processo em execução nesta máquina.");
            return Ok(processo);
        }
        [HttpGet("ativos")]
        public async Task<ActionResult<IEnumerable<ProcessoProducao>>> GetProcessosAtivos()
        {
            var processos = await _context.ProcessosProducao
                .Include(p => p.Maquina)
                .Include(p => p.OrdemServico)
                .Where(p => p.Status == "EM_EXECUCAO")
                .AsNoTracking()
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
            processo.QuantidadeProduzida = request.QuantidadeProduzida;
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