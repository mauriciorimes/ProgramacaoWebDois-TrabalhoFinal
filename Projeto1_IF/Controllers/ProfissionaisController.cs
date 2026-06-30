using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Authorization;
using Projeto1_IF.Data;
using Projeto1_IF.Models;

// Maurício Rimes Vieira
namespace Projeto1_IF.Controllers;

/// <summary>
/// Área dos gerentes. Cada gerente só enxerga os profissionais do seu tipo
/// (GerenteGeral vê todos) e pode visualizar, editar e excluir — nunca criar.
/// As restrições são aplicadas no servidor, não apenas escondendo links.
/// </summary>
[Authorize(Roles = Perfis.Gerentes)]
public class ProfissionaisController : Controller
{
    private readonly db_IFContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfissionaisController(db_IFContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Profissionais
    public async Task<IActionResult> Index()
    {
        var tiposPermitidos = await TiposPermitidosAsync();
        var profissionais = await _context.TbProfissionals
            .Where(p => p.IdTipoProfissional != null && tiposPermitidos.Contains(p.IdTipoProfissional.Value))
            .Include(p => p.IdContratoNavigation).ThenInclude(c => c.IdPlanoNavigation)
            .OrderBy(p => p.Nome)
            .ToListAsync();

        return View(profissionais);
    }

    // GET: Profissionais/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var profissional = await _context.TbProfissionals
            .Include(p => p.IdContratoNavigation).ThenInclude(c => c.IdPlanoNavigation)
            .FirstOrDefaultAsync(p => p.IdProfissional == id);

        if (profissional == null || !await PodeAcessarAsync(profissional))
            return NotFound();

        return View(profissional);
    }

    // GET: Profissionais/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var profissional = await _context.TbProfissionals.FindAsync(id);
        if (profissional == null || !await PodeAcessarAsync(profissional))
            return NotFound();

        return View(profissional);
    }

    // POST: Profissionais/Edit/5
    // O gerente pode alterar qualquer campo, inclusive o CPF. Padrão do tutorial EF:
    // carrega a entidade e atualiza os campos editáveis via TryUpdateModelAsync
    // (IdUser, contrato e tipo não entram na lista, então ficam preservados).
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id)
    {
        var profissional = await _context.TbProfissionals.FindAsync(id);
        if (profissional == null || !await PodeAcessarAsync(profissional))
            return NotFound();

        if (await TryUpdateModelAsync(
                profissional,
                prefix: "",
                p => p.Nome, p => p.Cpf, p => p.CrmCrn, p => p.Especialidade,
                p => p.Logradouro, p => p.Numero, p => p.Bairro, p => p.Cep,
                p => p.Cidade, p => p.Estado, p => p.Ddd1, p => p.Ddd2,
                p => p.Telefone1, p => p.Telefone2, p => p.Salario))
        {
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Não foi possível salvar as alterações.");
            }
        }

        return View(profissional);
    }

    // GET: Profissionais/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var profissional = await _context.TbProfissionals
            .FirstOrDefaultAsync(p => p.IdProfissional == id);

        if (profissional == null || !await PodeAcessarAsync(profissional))
            return NotFound();

        // Informa à view quantos pacientes o profissional possui, para decidir a exclusão.
        ViewBag.QtdPacientes = await _context.TbMedicoPacientes
            .CountAsync(mp => mp.IdProfissional == id);

        return View(profissional);
    }

    // POST: Profissionais/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, bool cascata = false)
    {
        var profissional = await _context.TbProfissionals.FindAsync(id);
        if (profissional == null || !await PodeAcessarAsync(profissional))
            return NotFound();

        var vinculos = await _context.TbMedicoPacientes
            .Where(mp => mp.IdProfissional == id)
            .ToListAsync();

        // Regra base: só exclui profissional sem pacientes.
        // Item extra: com "cascata", remove também os pacientes dos quais ele é o único profissional.
        if (vinculos.Count > 0 && !cascata)
        {
            ModelState.AddModelError(string.Empty,
                "Este profissional possui pacientes. Marque a opção de exclusão em cascata para removê-lo junto com os pacientes exclusivos dele.");
            ViewBag.QtdPacientes = vinculos.Count;
            return View(profissional);
        }

        try
        {
            if (cascata && vinculos.Count > 0)
            {
                foreach (var vinculo in vinculos)
                {
                    // Paciente é removido apenas se este for o único profissional dele.
                    var outrosProfissionais = await _context.TbMedicoPacientes
                        .AnyAsync(mp => mp.IdPaciente == vinculo.IdPaciente && mp.IdProfissional != id);

                    _context.TbMedicoPacientes.Remove(vinculo);

                    if (!outrosProfissionais)
                    {
                        var paciente = await _context.TbPacientes.FindAsync(vinculo.IdPaciente);
                        if (paciente != null)
                            _context.TbPacientes.Remove(paciente);
                    }
                }
            }

            var idContrato = profissional.IdContrato;
            var idUser = profissional.IdUser;

            _context.TbProfissionals.Remove(profissional);
            await _context.SaveChangesAsync();

            // Limpa o contrato órfão e o login Identity associado.
            var contrato = await _context.TbContratos.FindAsync(idContrato);
            if (contrato != null)
            {
                _context.TbContratos.Remove(contrato);
                await _context.SaveChangesAsync();
            }

            var user = await _userManager.FindByIdAsync(idUser);
            if (user != null)
                await _userManager.DeleteAsync(user);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty,
                "Não foi possível excluir o profissional (verifique se há registros vinculados a ele).");
            ViewBag.QtdPacientes = vinculos.Count;
            return View(profissional);
        }

        return RedirectToAction(nameof(Index));
    }

    // ----- Helpers de autorização por tipo --------------------------------

    /// <summary>IDs de tipo de profissional que o gerente logado tem permissão de ver.</summary>
    private async Task<List<int>> TiposPermitidosAsync()
    {
        if (User.IsInRole(Perfis.GerenteGeral))
            return await _context.TbTipoProfissionals.Select(t => t.IdTipoProfissional).ToListAsync();

        var nomes = new List<string>();
        if (User.IsInRole(Perfis.GerenteMedico)) nomes.Add(TiposProfissional.Medico);
        if (User.IsInRole(Perfis.GerenteNutricionista)) nomes.Add(TiposProfissional.Nutricionista);

        return await _context.TbTipoProfissionals
            .Where(t => nomes.Contains(t.Nome))
            .Select(t => t.IdTipoProfissional)
            .ToListAsync();
    }

    /// <summary>Garante que o gerente não acesse profissional de tipo fora do seu alcance.</summary>
    private async Task<bool> PodeAcessarAsync(TbProfissional profissional)
    {
        var tiposPermitidos = await TiposPermitidosAsync();
        return profissional.IdTipoProfissional != null
            && tiposPermitidos.Contains(profissional.IdTipoProfissional.Value);
    }
}
