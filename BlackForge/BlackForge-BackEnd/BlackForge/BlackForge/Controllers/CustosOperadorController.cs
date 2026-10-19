using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustosOperadorController : ControllerBase
    {
        private readonly BlackForgeDbContext _context;

        public CustosOperadorController(BlackForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/CustosOperador
        [HttpGet]
        public async Task<IActionResult> GetTodos()
        {
            var custos = await _context.CustosOperador
                .AsNoTracking()
                .Include(c => c.Funcionario)
                .OrderBy(c => c.Id)
                .ToListAsync();

            var resultado = custos.Select(c => new
            {
                c.Id,
                c.FuncionarioId,
                Funcionario = c.Funcionario == null
                    ? null
                    : new
                    {
                        c.Funcionario.Id,
                        c.Funcionario.Nome
                    },
                c.Cargo,
                c.CustoMensalTotal,
                c.HorasProdutivasMes,
                CustoHora = c.HorasProdutivasMes > 0
                    ? Math.Round(
                        c.CustoMensalTotal / c.HorasProdutivasMes,
                        4,
                        MidpointRounding.AwayFromZero)
                    : 0,
                c.Ativo,
                c.DataAtualizacao,
                c.Observacoes
            });

            return Ok(resultado);
        }

        // GET: api/CustosOperador/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPorId(int id)
        {
            var custo = await _context.CustosOperador
                .AsNoTracking()
                .Include(c => c.Funcionario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (custo == null)
                return NotFound(new
                {
                    mensagem = "Custo do operador não encontrado."
                });

            return Ok(new
            {
                custo.Id,
                custo.FuncionarioId,
                Funcionario = custo.Funcionario == null
                    ? null
                    : new
                    {
                        custo.Funcionario.Id,
                        custo.Funcionario.Nome
                    },
                custo.Cargo,
                custo.CustoMensalTotal,
                custo.HorasProdutivasMes,
                CustoHora = custo.HorasProdutivasMes > 0
                    ? Math.Round(
                        custo.CustoMensalTotal / custo.HorasProdutivasMes,
                        4,
                        MidpointRounding.AwayFromZero)
                    : 0,
                custo.Ativo,
                custo.DataAtualizacao,
                custo.Observacoes
            });
        }

        // GET: api/CustosOperador/funcionario/3
        [HttpGet("funcionario/{funcionarioId:int}")]
        public async Task<IActionResult> GetPorFuncionario(int funcionarioId)
        {
            var custo = await _context.CustosOperador
                .AsNoTracking()
                .Include(c => c.Funcionario)
                .FirstOrDefaultAsync(c =>
                    c.FuncionarioId == funcionarioId);

            if (custo == null)
                return NotFound(new
                {
                    mensagem = "Nenhum custo cadastrado para esse funcionário."
                });

            return Ok(new
            {
                custo.Id,
                custo.FuncionarioId,
                Funcionario = custo.Funcionario == null
                    ? null
                    : new
                    {
                        custo.Funcionario.Id,
                        custo.Funcionario.Nome
                    },
                custo.Cargo,
                custo.CustoMensalTotal,
                custo.HorasProdutivasMes,
                CustoHora = custo.HorasProdutivasMes > 0
                    ? Math.Round(
                        custo.CustoMensalTotal / custo.HorasProdutivasMes,
                        4,
                        MidpointRounding.AwayFromZero)
                    : 0,
                custo.Ativo,
                custo.DataAtualizacao,
                custo.Observacoes
            });
        }

        // POST: api/CustosOperador
        [HttpPost]
        public async Task<IActionResult> Criar(
            [FromBody] CustoOperador dados)
        {
            if (dados.CustoMensalTotal <= 0)
                return BadRequest(new
                {
                    mensagem = "O custo mensal total deve ser maior que zero."
                });

            if (dados.HorasProdutivasMes <= 0)
                return BadRequest(new
                {
                    mensagem = "As horas produtivas mensais devem ser maiores que zero."
                });

            if (dados.FuncionarioId.HasValue == !string.IsNullOrWhiteSpace(dados.Cargo))
                return BadRequest(new
                {
                    mensagem = "Informe um funcionário ou um cargo, mas não ambos."
                });

            if (dados.FuncionarioId.HasValue)
            {
                var funcionarioExiste = await _context.Funcionarios
                    .AnyAsync(f => f.Id == dados.FuncionarioId.Value);

                if (!funcionarioExiste)
                    return BadRequest(new
                    {
                        mensagem = "O funcionário informado não existe."
                    });

                var jaExiste = await _context.CustosOperador
                    .AnyAsync(c =>
                        c.FuncionarioId == dados.FuncionarioId.Value);

                if (jaExiste)
                    return Conflict(new
                    {
                        mensagem = "Já existe um custo cadastrado para esse funcionário."
                    });

                dados.Cargo = null;
            }
            else
            {
                dados.Cargo = dados.Cargo?.Trim();

                if (string.IsNullOrWhiteSpace(dados.Cargo))
                    return BadRequest(new
                    {
                        mensagem = "Informe um cargo válido."
                    });

                var jaExiste = await _context.CustosOperador
                    .AnyAsync(c =>
                        c.FuncionarioId == null &&
                        c.Cargo == dados.Cargo);

                if (jaExiste)
                    return Conflict(new
                    {
                        mensagem = "Já existe um custo cadastrado para esse cargo."
                    });
            }

            dados.Id = 0;
            dados.DataAtualizacao = DateTime.Now;

            _context.CustosOperador.Add(dados);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPorId),
                new { id = dados.Id },
                new
                {
                    mensagem = "Custo do operador cadastrado com sucesso.",
                    dados.Id,
                    dados.FuncionarioId,
                    dados.Cargo,
                    dados.CustoMensalTotal,
                    dados.HorasProdutivasMes,
                    CustoHora = Math.Round(
                        dados.CustoMensalTotal / dados.HorasProdutivasMes,
                        4,
                        MidpointRounding.AwayFromZero),
                    dados.Ativo,
                    dados.DataAtualizacao,
                    dados.Observacoes
                });
        }

        // PUT: api/CustosOperador/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(
            int id,
            [FromBody] CustoOperador dados)
        {
            var custo = await _context.CustosOperador
                .FirstOrDefaultAsync(c => c.Id == id);

            if (custo == null)
                return NotFound(new
                {
                    mensagem = "Custo do operador não encontrado."
                });

            if (dados.CustoMensalTotal <= 0)
                return BadRequest(new
                {
                    mensagem = "O custo mensal total deve ser maior que zero."
                });

            if (dados.HorasProdutivasMes <= 0)
                return BadRequest(new
                {
                    mensagem = "As horas produtivas mensais devem ser maiores que zero."
                });

            if (dados.FuncionarioId.HasValue == !string.IsNullOrWhiteSpace(dados.Cargo))
                return BadRequest(new
                {
                    mensagem = "Informe um funcionário ou um cargo, mas não ambos."
                });

            if (dados.FuncionarioId.HasValue)
            {
                var funcionarioExiste = await _context.Funcionarios
                    .AnyAsync(f => f.Id == dados.FuncionarioId.Value);

                if (!funcionarioExiste)
                    return BadRequest(new
                    {
                        mensagem = "O funcionário informado não existe."
                    });

                var duplicado = await _context.CustosOperador
                    .AnyAsync(c =>
                        c.Id != id &&
                        c.FuncionarioId == dados.FuncionarioId.Value);

                if (duplicado)
                    return Conflict(new
                    {
                        mensagem = "Já existe um custo cadastrado para esse funcionário."
                    });

                custo.FuncionarioId = dados.FuncionarioId;
                custo.Cargo = null;
            }
            else
            {
                var cargo = dados.Cargo!.Trim();

                var duplicado = await _context.CustosOperador
                    .AnyAsync(c =>
                        c.Id != id &&
                        c.FuncionarioId == null &&
                        c.Cargo == cargo);

                if (duplicado)
                    return Conflict(new
                    {
                        mensagem = "Já existe um custo cadastrado para esse cargo."
                    });

                custo.FuncionarioId = null;
                custo.Cargo = cargo;
            }

            custo.CustoMensalTotal = dados.CustoMensalTotal;
            custo.HorasProdutivasMes = dados.HorasProdutivasMes;
            custo.Ativo = dados.Ativo;
            custo.Observacoes = dados.Observacoes;
            custo.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/CustosOperador/5/ativar
        [HttpPatch("{id:int}/ativar")]
        public async Task<IActionResult> AlterarAtivo(int id)
        {
            var custo = await _context.CustosOperador
                .FirstOrDefaultAsync(c => c.Id == id);

            if (custo == null)
                return NotFound(new
                {
                    mensagem = "Custo do operador não encontrado."
                });

            custo.Ativo = !custo.Ativo;
            custo.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = custo.Ativo
                    ? "Custo do operador ativado."
                    : "Custo do operador desativado.",
                custo.Id,
                custo.Ativo
            });
        }

        // DELETE: api/CustosOperador/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var custo = await _context.CustosOperador
                .FirstOrDefaultAsync(c => c.Id == id);

            if (custo == null)
                return NotFound(new
                {
                    mensagem = "Custo do operador não encontrado."
                });

            _context.CustosOperador.Remove(custo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
