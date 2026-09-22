using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracoesMaquinaController : ControllerBase
    {
        private readonly BlackForgeDbContext _context;
        public ConfiguracoesMaquinaController(BlackForgeDbContext context)
        {
            _context = context;
        }       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConfiguracaoMaquina>>> GetConfiguracoes()
        {
            var configuracoes = await _context.ConfiguracoesMaquina
                .Include(c => c.Maquina)
                .AsNoTracking()
                .ToListAsync();
            return Ok(configuracoes);
        }     
        [HttpGet("{id}")]
        public async Task<ActionResult<ConfiguracaoMaquina>> GetConfiguracao(int id)
        {
            var configuracao = await _context.ConfiguracoesMaquina
                .Include(c => c.Maquina)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (configuracao == null)
                return NotFound();
            return Ok(configuracao);
        }       
        [HttpGet("maquina/{maquinaId}")]
        public async Task<ActionResult<IEnumerable<ConfiguracaoMaquina>>> GetPorMaquina(
            int maquinaId)
        {
            var maquinaExiste = await _context.Maquinas
                .AnyAsync(m => m.Id == maquinaId);
            if (!maquinaExiste)
                return NotFound("Máquina não encontrada.");
            var configuracoes = await _context.ConfiguracoesMaquina
                .Where(c => c.MaquinaId == maquinaId)
                .AsNoTracking()
                .ToListAsync();
            return Ok(configuracoes);
        }
        [HttpPost]
        public async Task<ActionResult<ConfiguracaoMaquina>> CriarConfiguracao(
            ConfiguracaoMaquina configuracao)
        {
            var maquinaExiste = await _context.Maquinas
                .AnyAsync(m => m.Id == configuracao.MaquinaId);
            if (!maquinaExiste)
                return BadRequest("A máquina informada não existe.");
            if (string.IsNullOrWhiteSpace(configuracao.TipoServico))
                return BadRequest("O tipo de serviço é obrigatório.");
            if (configuracao.ProducaoPorMinuto <= 0)
                return BadRequest("A produção por minuto deve ser maior que zero.");
            if (configuracao.ConsumoPorUnidade < 0)
                return BadRequest("O consumo por unidade não pode ser negativo.");
            _context.ConfiguracoesMaquina.Add(configuracao);
            await _context.SaveChangesAsync();
            return CreatedAtAction(
                nameof(GetConfiguracao),
                new { id = configuracao.Id },
                configuracao
            );
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarConfiguracao(
            int id,
            ConfiguracaoMaquina configuracao)
        {
            if (id != configuracao.Id)
                return BadRequest("O ID da URL não corresponde ao ID da configuração.");
            var maquinaExiste = await _context.Maquinas
                .AnyAsync(m => m.Id == configuracao.MaquinaId);
            if (!maquinaExiste)
                return BadRequest("A máquina informada não existe.");
            if (string.IsNullOrWhiteSpace(configuracao.TipoServico))
                return BadRequest("O tipo de serviço é obrigatório.");
            if (configuracao.ProducaoPorMinuto <= 0)
                return BadRequest("A produção por minuto deve ser maior que zero.");
            if (configuracao.ConsumoPorUnidade < 0)
                return BadRequest("O consumo por unidade não pode ser negativo.");
            _context.Entry(configuracao).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }  
        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirConfiguracao(int id)
        {
            var configuracao = await _context.ConfiguracoesMaquina
                .FindAsync(id);
            if (configuracao == null)
                return NotFound();
            _context.ConfiguracoesMaquina.Remove(configuracao);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}