
using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracoesPrecificacaoController : ControllerBase
    {
        private readonly BlackForgeDbContext _context;

        public ConfiguracoesPrecificacaoController(
            BlackForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/ConfiguracoesPrecificacao
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConfiguracaoPrecificacao>>>
            GetConfiguracoes()
        {
            var configuracoes = await _context.ConfiguracoesPrecificacao
                .AsNoTracking()
                .OrderByDescending(c => c.DataVigencia)
                .ToListAsync();

            return Ok(configuracoes);
        }

        // GET: api/ConfiguracoesPrecificacao/ativa
        [HttpGet("ativa")]
        public async Task<ActionResult<ConfiguracaoPrecificacao>>
            GetConfiguracaoAtiva()
        {
            var configuracao = await _context.ConfiguracoesPrecificacao
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Ativa);

            if (configuracao == null)
            {
                return NotFound(
                    "Nenhuma configuração de precificação está ativa.");
            }

            return Ok(configuracao);
        }

        // GET: api/ConfiguracoesPrecificacao/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ConfiguracaoPrecificacao>>
            GetConfiguracao(int id)
        {
            var configuracao = await _context.ConfiguracoesPrecificacao
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (configuracao == null)
            {
                return NotFound("Configuração não encontrada.");
            }

            return Ok(configuracao);
        }

        // POST: api/ConfiguracoesPrecificacao
        [HttpPost]
        public async Task<ActionResult<ConfiguracaoPrecificacao>>
            CriarConfiguracao(ConfiguracaoPrecificacao configuracao)
        {
            var erro = ValidarConfiguracao(configuracao);

            if (erro != null)
            {
                return BadRequest(erro);
            }

            if (configuracao.DataVigencia == default)
            {
                configuracao.DataVigencia = DateTime.Now;
            }

            await using var transacao =
                await _context.Database.BeginTransactionAsync();

            try
            {
                if (configuracao.Ativa)
                {
                    await DesativarConfiguracoesAtivas();
                }

                _context.ConfiguracoesPrecificacao.Add(configuracao);

                await _context.SaveChangesAsync();
                await transacao.CommitAsync();

                return CreatedAtAction(
                    nameof(GetConfiguracao),
                    new { id = configuracao.Id },
                    configuracao
                );
            }
            catch
            {
                await transacao.RollbackAsync();
                throw;
            }
        }

        // PUT: api/ConfiguracoesPrecificacao/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> AtualizarConfiguracao(
            int id,
            ConfiguracaoPrecificacao dados)
        {
            if (id != dados.Id)
            {
                return BadRequest(
                    "O ID da URL não corresponde ao ID da configuração.");
            }

            var erro = ValidarConfiguracao(dados);

            if (erro != null)
            {
                return BadRequest(erro);
            }

            var configuracao = await _context.ConfiguracoesPrecificacao
                .FirstOrDefaultAsync(c => c.Id == id);

            if (configuracao == null)
            {
                return NotFound("Configuração não encontrada.");
            }

            await using var transacao =
                await _context.Database.BeginTransactionAsync();

            try
            {
                if (dados.Ativa)
                {
                    await DesativarConfiguracoesAtivas(id);
                }

                configuracao.MargemDesejadaPercentual =
                    dados.MargemDesejadaPercentual;

                configuracao.ImpostosPercentual =
                    dados.ImpostosPercentual;

                configuracao.DespesasVariaveisPercentual =
                    dados.DespesasVariaveisPercentual;

                configuracao.CustosIndiretosPercentual =
                    dados.CustosIndiretosPercentual;

                configuracao.SetupPadraoMinutos =
                    dados.SetupPadraoMinutos;

                configuracao.CustoHoraOperadorPadrao =
                    dados.CustoHoraOperadorPadrao;

                configuracao.Ativa = dados.Ativa;

                configuracao.DataVigencia =
                    dados.DataVigencia == default
                        ? configuracao.DataVigencia
                        : dados.DataVigencia;

                configuracao.Observacoes = dados.Observacoes;

                await _context.SaveChangesAsync();
                await transacao.CommitAsync();

                return NoContent();
            }
            catch
            {
                await transacao.RollbackAsync();
                throw;
            }
        }

        // PUT: api/ConfiguracoesPrecificacao/1/ativar
        [HttpPut("{id:int}/ativar")]
        public async Task<IActionResult> AtivarConfiguracao(int id)
        {
            var configuracao = await _context.ConfiguracoesPrecificacao
                .FirstOrDefaultAsync(c => c.Id == id);

            if (configuracao == null)
            {
                return NotFound("Configuração não encontrada.");
            }

            if (configuracao.Ativa)
            {
                return Ok("Esta configuração já está ativa.");
            }

            await using var transacao =
                await _context.Database.BeginTransactionAsync();

            try
            {
                await DesativarConfiguracoesAtivas(id);

                configuracao.Ativa = true;

                await _context.SaveChangesAsync();
                await transacao.CommitAsync();

                return Ok("Configuração de precificação ativada com sucesso.");
            }
            catch
            {
                await transacao.RollbackAsync();
                throw;
            }
        }

        // DELETE: api/ConfiguracoesPrecificacao/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> ExcluirConfiguracao(int id)
        {
            var configuracao = await _context.ConfiguracoesPrecificacao
                .FirstOrDefaultAsync(c => c.Id == id);

            if (configuracao == null)
            {
                return NotFound("Configuração não encontrada.");
            }

            if (configuracao.Ativa)
            {
                return BadRequest(
                    "Desative a configuração antes de excluí-la.");
            }

            var possuiPrecificacoes = await _context.Precificacoes
                .AnyAsync(p => p.ConfiguracaoPrecificacaoId == id);

            if (possuiPrecificacoes)
            {
                return Conflict(
                    "Esta configuração já foi utilizada em precificações " +
                    "e não pode ser excluída.");
            }

            _context.ConfiguracoesPrecificacao.Remove(configuracao);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task DesativarConfiguracoesAtivas(int? ignorarId = null)
        {
            var ativas = await _context.ConfiguracoesPrecificacao
                .Where(c => c.Ativa &&
                    (!ignorarId.HasValue || c.Id != ignorarId.Value))
                .ToListAsync();

            foreach (var configuracao in ativas)
            {
                configuracao.Ativa = false;
            }
        }

        private static string? ValidarConfiguracao(
            ConfiguracaoPrecificacao configuracao)
        {
            if (configuracao.MargemDesejadaPercentual < 0 ||
                configuracao.MargemDesejadaPercentual >= 100)
            {
                return "A margem desejada deve ser maior ou igual a zero " +
                       "e menor que 100%.";
            }

            if (configuracao.ImpostosPercentual < 0 ||
                configuracao.ImpostosPercentual >= 100)
            {
                return "Os impostos devem ser maiores ou iguais a zero " +
                       "e menores que 100%.";
            }

            if (configuracao.DespesasVariaveisPercentual < 0 ||
                configuracao.DespesasVariaveisPercentual >= 100)
            {
                return "As despesas variáveis devem ser maiores ou iguais " +
                       "a zero e menores que 100%.";
            }

            if (configuracao.CustosIndiretosPercentual < 0)
            {
                return "Os custos indiretos não podem ser negativos.";
            }

            var percentuais =
                configuracao.MargemDesejadaPercentual +
                configuracao.ImpostosPercentual +
                configuracao.DespesasVariaveisPercentual;

            if (percentuais >= 100)
            {
                return "A soma da margem, dos impostos e das despesas " +
                       "variáveis deve ser menor que 100%.";
            }

            if (configuracao.SetupPadraoMinutos < 0)
            {
                return "O tempo padrão de setup não pode ser negativo.";
            }

            if (configuracao.CustoHoraOperadorPadrao < 0)
            {
                return "O custo padrão por hora do operador não pode ser negativo.";
            }
            return null;
        }
    }
}
