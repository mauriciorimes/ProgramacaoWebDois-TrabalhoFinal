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
/// Auto-serviço do profissional: ele só pode visualizar (Details) e editar (Edit)
/// os PRÓPRIOS dados. O CPF não pode ser alterado e nenhum dado de outro
/// profissional é acessível — o vínculo é feito por TbProfissional.IdUser.
/// </summary>
[Authorize(Roles = Perfis.Profissionais)]
public class MeuCadastroController : Controller
{
    private readonly db_IFContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MeuCadastroController(db_IFContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: MeuCadastro -> mostra os próprios dados
    public IActionResult Index() => RedirectToAction(nameof(Details));

    // GET: MeuCadastro/Details
    public async Task<IActionResult> Details()
    {
        var profissional = await ProfissionalAtualAsync();
        if (profissional == null)
            return NotFound();

        return View(profissional);
    }

    // GET: MeuCadastro/Edit
    public async Task<IActionResult> Edit()
    {
        var profissional = await ProfissionalAtualAsync();
        if (profissional == null)
            return NotFound();

        return View(profissional);
    }

    // POST: MeuCadastro/Edit
    // Padrão do tutorial EF: carrega o próprio registro e atualiza só os campos
    // editáveis via TryUpdateModelAsync. CPF, IdUser, contrato e tipo NÃO entram
    // na lista — ficam preservados, então o CPF nunca é alterado.
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost()
    {
        var profissional = await ProfissionalAtualAsync();
        if (profissional == null)
            return NotFound();

        if (await TryUpdateModelAsync(
                profissional,
                prefix: "",
                p => p.Nome, p => p.CrmCrn, p => p.Especialidade,
                p => p.Logradouro, p => p.Numero, p => p.Bairro, p => p.Cep,
                p => p.Cidade, p => p.Estado, p => p.Ddd1, p => p.Ddd2,
                p => p.Telefone1, p => p.Telefone2))
        {
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Não foi possível salvar as alterações.");
            }
        }

        return View(profissional);
    }

    /// <summary>Profissional vinculado ao usuário logado (TbProfissional.IdUser == Id do Identity).</summary>
    private async Task<TbProfissional?> ProfissionalAtualAsync()
    {
        var userId = _userManager.GetUserId(User);
        return await _context.TbProfissionals.FirstOrDefaultAsync(p => p.IdUser == userId);
    }
}
