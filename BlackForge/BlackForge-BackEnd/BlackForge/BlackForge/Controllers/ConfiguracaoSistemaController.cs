using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracaoController : ControllerBase
    {
        private readonly BlackForgeDbContext _context;

        public ConfiguracaoController(BlackForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/configuracao
        [HttpGet]
        public async Task<ActionResult<ConfiguracaoSistema>> GetConfiguracao()
        {
            var configuracao = await _context.ConfiguracaoSistema
                .Include(c => c.Tema)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == 1);

            if (configuracao == null)
            {
                return NotFound("Configuração do sistema não encontrada.");
            }

            return Ok(configuracao);
        }

        // PUT: api/configuracao/tema
        [HttpPut("tema")]
        public async Task<ActionResult<ConfiguracaoSistema>> AlterarTema(
            [FromBody] string codigoTema)
        {
            if (string.IsNullOrWhiteSpace(codigoTema))
            {
                return BadRequest("O tema não pode ser vazio.");
            }

            codigoTema = codigoTema.Trim().ToLower();

            var tema = await _context.Temas
                .FirstOrDefaultAsync(t =>
                    t.Codigo == codigoTema &&
                    t.Ativo);

            if (tema == null)
            {
                return NotFound("Tema não encontrado ou está inativo.");
            }

            var configuracao = await _context.ConfiguracaoSistema
                .FirstOrDefaultAsync(c => c.Id == 1);

            if (configuracao == null)
            {
                return NotFound("Configuração do sistema não encontrada.");
            }

            configuracao.TemaId = tema.Id;

            await _context.SaveChangesAsync();

            configuracao.Tema = tema;

            return Ok(configuracao);
        }
    }
}