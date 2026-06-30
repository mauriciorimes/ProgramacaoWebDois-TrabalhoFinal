using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Authorization;
using Projeto1_IF.Data;
using Projeto1_IF.Models;

// Maurício Rimes Vieira
namespace Projeto1_IF.Controllers;

/// <summary>
/// CRUD de pacientes feito por cada profissional. O profissional só enxerga e
/// manipula os pacientes que ele mesmo cadastrou — o vínculo é registrado em
/// tbMedico_Paciente. A posse é checada no servidor em todas as ações.
/// </summary>
[Authorize(Roles = Perfis.Profissionais)]
public class TbPacientesController : Controller
{
    private readonly db_IFContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TbPacientesController(db_IFContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: TbPacientes -> apenas os pacientes do profissional logado
    public async Task<IActionResult> Index()
    {
        var profissional = await ProfissionalAtualAsync();
        if (profissional == null)
            return Forbid();

        // Include da cidade para exibir o NOME (e não o Id) na listagem.
        var pacientes = await _context.TbPacientes
            .Include(p => p.IdCidadeNavigation)
            .Where(p => _context.TbMedicoPacientes
                .Any(mp => mp.IdPaciente == p.IdPaciente && mp.IdProfissional == profissional.IdProfissional))
            .OrderBy(p => p.Nome)
            .ToListAsync();

        return View(pacientes);
    }

    // GET: TbPacientes/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        if (!await PacienteEhMeuAsync(id.Value))
            return NotFound();

        var paciente = await _context.TbPacientes
            .Include(p => p.IdCidadeNavigation)
            .FirstOrDefaultAsync(p => p.IdPaciente == id);
        if (paciente == null)
            return NotFound();

        return View(paciente);
    }

    // GET: TbPacientes/Create
    public IActionResult Create()
    {
        PopularCidades();
        return View();
    }

    // POST: TbPacientes/Create
    // Sem a chave primária (IdPaciente) no Bind — ela é gerada pelo banco.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Rg,Cpf,DataNascimento,NomeResponsavel,Sexo,Etnia,Endereco,Bairro,IdCidade,TelResidencial,TelComercial,TelCelular,Profissao,FlgAtleta,FlgGestante")] TbPaciente paciente)
    {
        var profissional = await ProfissionalAtualAsync();
        if (profissional == null)
            return Forbid();

        if (ModelState.IsValid)
        {
            try
            {
                // Cria o paciente e, em seguida, o vínculo com o profissional logado.
                _context.TbPacientes.Add(paciente);
                await _context.SaveChangesAsync();

                _context.TbMedicoPacientes.Add(new TbMedicoPaciente
                {
                    IdPaciente = paciente.IdPaciente,
                    IdProfissional = profissional.IdProfissional
                });
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty,
                    "Não foi possível cadastrar o paciente. Tente novamente e, se o problema persistir, contate o administrador.");
            }
        }

        PopularCidades(paciente.IdCidade);
        return View(paciente);
    }

    // GET: TbPacientes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        if (!await PacienteEhMeuAsync(id.Value))
            return NotFound();

        var paciente = await _context.TbPacientes.FindAsync(id);
        if (paciente == null)
            return NotFound();

        PopularCidades(paciente.IdCidade);
        return View(paciente);
    }

    // POST: TbPacientes/Edit/5
    // Padrão recomendado do tutorial EF: busca a entidade e aplica TryUpdateModelAsync
    // com lista explícita de campos (evita overposting e mantém o controle de alterações).
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int? id)
    {
        if (id == null)
            return NotFound();

        if (!await PacienteEhMeuAsync(id.Value))
            return NotFound();

        var pacienteParaAtualizar = await _context.TbPacientes.FirstOrDefaultAsync(p => p.IdPaciente == id);
        if (pacienteParaAtualizar == null)
            return NotFound();

        if (await TryUpdateModelAsync(
                pacienteParaAtualizar,
                prefix: "",
                p => p.Nome, p => p.Rg, p => p.Cpf, p => p.DataNascimento, p => p.NomeResponsavel,
                p => p.Sexo, p => p.Etnia, p => p.Endereco, p => p.Bairro, p => p.IdCidade,
                p => p.TelResidencial, p => p.TelComercial, p => p.TelCelular, p => p.Profissao,
                p => p.FlgAtleta, p => p.FlgGestante))
        {
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.TbPacientes.Any(p => p.IdPaciente == id))
                    return NotFound();
                throw;
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty,
                    "Não foi possível salvar as alterações. Tente novamente e, se o problema persistir, contate o administrador.");
            }
        }

        PopularCidades(pacienteParaAtualizar.IdCidade);
        return View(pacienteParaAtualizar);
    }

    // GET: TbPacientes/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        if (!await PacienteEhMeuAsync(id.Value))
            return NotFound();

        var paciente = await _context.TbPacientes
            .Include(p => p.IdCidadeNavigation)
            .FirstOrDefaultAsync(p => p.IdPaciente == id);
        if (paciente == null)
            return NotFound();

        return View(paciente);
    }

    // POST: TbPacientes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var profissional = await ProfissionalAtualAsync();
        if (profissional == null)
            return Forbid();

        if (!await PacienteEhMeuAsync(id))
            return NotFound();

        try
        {
            // Remove o vínculo deste profissional com o paciente.
            var vinculo = await _context.TbMedicoPacientes
                .FirstOrDefaultAsync(mp => mp.IdPaciente == id && mp.IdProfissional == profissional.IdProfissional);
            if (vinculo != null)
                _context.TbMedicoPacientes.Remove(vinculo);

            // Só exclui o paciente em si se nenhum outro profissional o acompanhar.
            var temOutroProfissional = await _context.TbMedicoPacientes
                .AnyAsync(mp => mp.IdPaciente == id && mp.IdProfissional != profissional.IdProfissional);
            if (!temOutroProfissional)
            {
                var paciente = await _context.TbPacientes.FindAsync(id);
                if (paciente != null)
                    _context.TbPacientes.Remove(paciente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Não foi possível excluir o paciente.");
            var paciente = await _context.TbPacientes
                .Include(p => p.IdCidadeNavigation)
                .FirstOrDefaultAsync(p => p.IdPaciente == id);
            return View(paciente);
        }
    }

    // ----- Helpers --------------------------------------------------------

    /// <summary>Preenche a lista de cidades (ordenada por nome) para o dropdown.</summary>
    private void PopularCidades(object? cidadeSelecionada = null)
    {
        ViewData["IdCidade"] = new SelectList(
            _context.TbCidades.OrderBy(c => c.Nome), "IdCidade", "Nome", cidadeSelecionada);
    }

    private async Task<TbProfissional?> ProfissionalAtualAsync()
    {
        var userId = _userManager.GetUserId(User);
        return await _context.TbProfissionals.FirstOrDefaultAsync(p => p.IdUser == userId);
    }

    /// <summary>Verifica se o paciente está vinculado ao profissional logado.</summary>
    private async Task<bool> PacienteEhMeuAsync(int idPaciente)
    {
        var profissional = await ProfissionalAtualAsync();
        if (profissional == null)
            return false;

        return await _context.TbMedicoPacientes
            .AnyAsync(mp => mp.IdPaciente == idPaciente && mp.IdProfissional == profissional.IdProfissional);
    }
}
