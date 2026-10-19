
using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustosMaquinaController : ControllerBase
    {
        private readonly BlackForgeDbContext _context;

        public CustosMaquinaController(BlackForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/CustosMaquina
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustoMaquina>>>
            GetCustosMaquina()
        {
            var custos = await _context.CustosMaquina
                .Include(c => c.Maquina)
                .AsNoTracking()
                .OrderBy(c => c.MaquinaId)
                .ToListAsync();

            return Ok(custos);
        }

        // GET: api/CustosMaquina/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustoMaquina>>
            GetCustoMaquina(int id)
        {
            var custo = await _context.CustosMaquina
                .Include(c => c.Maquina)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (custo == null)
            {
                return NotFound("Custo da máquina não encontrado.");
            }

            return Ok(custo);
        }

        // GET: api/CustosMaquina/maquina/1
        [HttpGet("maquina/{maquinaId:int}")]
        public async Task<ActionResult<CustoMaquina>>
            GetCustoPorMaquina(int maquinaId)
        {
            var maquinaExiste = await _context.Maquinas
                .AnyAsync(m => m.Id == maquinaId);

            if (!maquinaExiste)
            {
                return NotFound("Máquina não encontrada.");
            }

            var custo = await _context.CustosMaquina
                .Include(c => c.Maquina)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.MaquinaId == maquinaId);

            if (custo == null)
            {
                return NotFound(
                    "Esta máquina ainda não possui custos cadastrados.");
            }

            return Ok(custo);
        }

        // GET: api/CustosMaquina/1/calculo
        [HttpGet("{id:int}/calculo")]
        public async Task<IActionResult> CalcularCustoHora(int id)
        {
            var custo = await _context.CustosMaquina
                .Include(c => c.Maquina)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (custo == null)
            {
                return NotFound("Custo da máquina não encontrado.");
            }

            var validacao = ValidarCusto(custo);

            if (validacao != null)
            {
                return BadRequest(validacao);
            }

            decimal depreciacaoHora = 0;
            decimal custoEnergiaHora = 0;

            if (custo.MetodoCalculo == "MANUAL")
            {
                return Ok(new
                {
                    custo.Id,
                    custo.MaquinaId,
                    Maquina = custo.Maquina?.Nome,
                    MetodoCalculo = custo.MetodoCalculo,
                    CustoHora = custo.CustoHoraManual,
                    Observacao = "Custo por hora informado manualmente."
                });
            }

            if (custo.VidaUtilHoras > 0)
            {
                depreciacaoHora =
                    (custo.ValorAquisicao - custo.ValorResidual)
                    / custo.VidaUtilHoras;
            }

            custoEnergiaHora =
                custo.ConsumoEnergiaKW * custo.CustoKWh;

            var custoHoraTotal =
                depreciacaoHora +
                custoEnergiaHora +
                custo.CustoManutencaoHora +
                custo.OutrosCustosHora;

            return Ok(new
            {
                custo.Id,
                custo.MaquinaId,
                Maquina = custo.Maquina?.Nome,
                MetodoCalculo = custo.MetodoCalculo,
                DepreciacaoPorHora = Math.Round(depreciacaoHora, 4),
                EnergiaPorHora = Math.Round(custoEnergiaHora, 4),
                custo.CustoManutencaoHora,
                custo.OutrosCustosHora,
                CustoHora = Math.Round(custoHoraTotal, 4)
            });
        }

        // POST: api/CustosMaquina
        [HttpPost]
        public async Task<ActionResult<CustoMaquina>>
            CriarCustoMaquina(CustoMaquina custo)
        {
            var maquinaExiste = await _context.Maquinas
                .AnyAsync(m => m.Id == custo.MaquinaId);

            if (!maquinaExiste)
            {
                return BadRequest("A máquina informada não existe.");
            }

            var jaExiste = await _context.CustosMaquina
                .AnyAsync(c => c.MaquinaId == custo.MaquinaId);

            if (jaExiste)
            {
                return Conflict(
                    "Esta máquina já possui um cadastro de custos. " +
                    "Utilize a atualização para modificar os valores.");
            }

            var validacao = ValidarCusto(custo);

            if (validacao != null)
            {
                return BadRequest(validacao);
            }

            custo.DataAtualizacao = DateTime.Now;

            _context.CustosMaquina.Add(custo);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCustoMaquina),
                new { id = custo.Id },
                custo
            );
        }

        // PUT: api/CustosMaquina/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> AtualizarCustoMaquina(
            int id,
            CustoMaquina dados)
        {
            if (dados.Id != 0 && id != dados.Id)
            {
                return BadRequest(
                    "O ID da URL não corresponde ao ID do custo enviado.");
            }

            var custo = await _context.CustosMaquina
                .FirstOrDefaultAsync(c => c.Id == id);

            if (custo == null)
            {
                return NotFound("Custo da máquina não encontrado.");
            }

            if (dados.MaquinaId != custo.MaquinaId)
            {
                return BadRequest(
                    "Não é permitido alterar a máquina vinculada a este custo.");
            }

            var validacao = ValidarCusto(dados);

            if (validacao != null)
            {
                return BadRequest(validacao);
            }

            custo.ValorAquisicao = dados.ValorAquisicao;
            custo.ValorResidual = dados.ValorResidual;
            custo.VidaUtilHoras = dados.VidaUtilHoras;
            custo.ConsumoEnergiaKW = dados.ConsumoEnergiaKW;
            custo.CustoKWh = dados.CustoKWh;
            custo.CustoManutencaoHora = dados.CustoManutencaoHora;
            custo.OutrosCustosHora = dados.OutrosCustosHora;
            custo.CustoHoraManual = dados.CustoHoraManual;
            custo.MetodoCalculo = dados.MetodoCalculo;
            custo.Ativa = dados.Ativa;
            custo.Observacoes = dados.Observacoes;
            custo.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/CustosMaquina/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> ExcluirCustoMaquina(int id)
        {
            var custo = await _context.CustosMaquina
                .FirstOrDefaultAsync(c => c.Id == id);

            if (custo == null)
            {
                return NotFound("Custo da máquina não encontrado.");
            }

            _context.CustosMaquina.Remove(custo);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ================= VALIDAÇÕES =================

        private static string? ValidarCusto(CustoMaquina custo)
        {
            if (custo.ValorAquisicao < 0 ||
                custo.ValorResidual < 0)
            {
                return "Os valores de aquisição e residual não podem ser negativos.";
            }

            if (custo.ValorResidual > custo.ValorAquisicao)
            {
                return "O valor residual não pode superar o valor de aquisição.";
            }

            if (custo.VidaUtilHoras < 0)
            {
                return "A vida útil não pode ser negativa.";
            }

            if (custo.ConsumoEnergiaKW < 0 ||
                custo.CustoKWh < 0 ||
                custo.CustoManutencaoHora < 0 ||
                custo.OutrosCustosHora < 0)
            {
                return "Os custos e o consumo não podem ser negativos.";
            }

            if (custo.MetodoCalculo != "DETALHADO" &&
                custo.MetodoCalculo != "MANUAL")
            {
                return "O método deve ser DETALHADO ou MANUAL.";
            }

            if (custo.MetodoCalculo == "MANUAL")
            {
                if (!custo.CustoHoraManual.HasValue ||
                    custo.CustoHoraManual.Value <= 0)
                {
                    return "Informe um custo por hora manual maior que zero.";
                }
            }

            if (custo.MetodoCalculo == "DETALHADO" &&
                custo.ValorAquisicao > custo.ValorResidual &&
                custo.VidaUtilHoras <= 0)
            {
                return "Informe uma vida útil maior que zero para calcular a depreciação.";
            }

            return null;
        }
    }
}
