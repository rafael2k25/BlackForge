using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuncionariosController : ControllerBase
    {
        private readonly BlackForgeDbContext _context;

        public FuncionariosController(BlackForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/funcionarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Funcionario>>> GetFuncionarios()
        {
            var funcionarios = await _context.Funcionarios
                .AsNoTracking()
                .ToListAsync();

            return Ok(funcionarios);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Funcionario>> GetFuncionario(int id)
        {
            var funcionario = await _context.Funcionarios
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id);

            if (funcionario == null)
            {
                return NotFound();
            }

            return Ok(funcionario);
        }

        [HttpPost]
        public async Task<ActionResult<Funcionario>> CriarFuncionario(
            Funcionario funcionario)
        {
            
            if (string.IsNullOrWhiteSpace(funcionario.Nome))
            {
                return BadRequest("O nome do funcionário é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(funcionario.Matricula))
            {
                return BadRequest("A matrícula do funcionário é obrigatória.");
            }

            if (string.IsNullOrWhiteSpace(funcionario.CPF))
            {
                return BadRequest("O CPF do funcionário é obrigatório.");
            }

            if (funcionario.Idade <= 0)
            {
                return BadRequest("A idade do funcionário deve ser maior que zero.");
            }

            if (funcionario.DataAdmissao == default)
            {
                return BadRequest("A data de admissão é obrigatória.");
            }

            var matriculaExiste = await _context.Funcionarios
                .AnyAsync(f => f.Matricula == funcionario.Matricula);

            if (matriculaExiste)
            {
                return Conflict(
                    "Já existe um funcionário com essa matrícula."
                );
            }

            var cpfExiste = await _context.Funcionarios
                .AnyAsync(f => f.CPF == funcionario.CPF);

            if (cpfExiste)
            {
                return Conflict(
                    "Já existe um funcionário com esse CPF."
                );
            }

            _context.Funcionarios.Add(funcionario);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetFuncionario),
                new { id = funcionario.Id },
                funcionario
            );
        }
     
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarFuncionario(
            int id,
            Funcionario funcionario)
        {

            if (id != funcionario.Id)
            {
                return BadRequest(
                    "O ID da URL não corresponde ao ID do funcionário."
                );
            }

            if (string.IsNullOrWhiteSpace(funcionario.Nome))
            {
                return BadRequest("O nome do funcionário é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(funcionario.Matricula))
            {
                return BadRequest("A matrícula do funcionário é obrigatória.");
            }

            if (string.IsNullOrWhiteSpace(funcionario.CPF))
            {
                return BadRequest("O CPF do funcionário é obrigatório.");
            }

            if (funcionario.Idade <= 0)
            {
                return BadRequest("A idade do funcionário deve ser maior que zero.");
            }

            if (funcionario.DataAdmissao == default)
            {
                return BadRequest("A data de admissão é obrigatória.");
            }

            var funcionarioExiste = await _context.Funcionarios
                .AnyAsync(f => f.Id == id);

            if (!funcionarioExiste)
            {
                return NotFound();
            }

            var matriculaEmUso = await _context.Funcionarios
                .AnyAsync(f =>
                    f.Matricula == funcionario.Matricula &&
                    f.Id != id);

            if (matriculaEmUso)
            {
                return Conflict(
                    "Já existe outro funcionário com essa matrícula."
                );
            }

            var cpfEmUso = await _context.Funcionarios
                .AnyAsync(f =>
                    f.CPF == funcionario.CPF &&
                    f.Id != id);

            if (cpfEmUso)
            {
                return Conflict(
                    "Já existe outro funcionário com esse CPF."
                );
            }

            _context.Entry(funcionario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FuncionarioExiste(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirFuncionario(int id)
        {
            var funcionario = await _context.Funcionarios
                .FindAsync(id);

            if (funcionario == null)
            {
                return NotFound();
            }

            _context.Funcionarios.Remove(funcionario);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        private bool FuncionarioExiste(int id)
        {
            return _context.Funcionarios
                .Any(f => f.Id == id);
        }
    }
}