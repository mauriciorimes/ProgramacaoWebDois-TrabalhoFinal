using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Models;

// Maurício Rimes Vieira
namespace Projeto1_IF.Controllers;

[Authorize]
public class TbPacientesController : Controller
{
    private readonly db_IFContext _context;

    public TbPacientesController(db_IFContext context)
    {
        _context = context;
    }

    // GET: TbPacientes
    public async Task<IActionResult> Index()
    {
        return View(await _context.TbPacientes.ToListAsync());
    }

    // GET: TbPacientes/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tbPaciente = await _context.TbPacientes
            .FirstOrDefaultAsync(m => m.IdPaciente == id);

        if (tbPaciente == null)
        {
            return NotFound();
        }

        return View(tbPaciente);
    }

    // GET: TbPacientes/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: TbPacientes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdPaciente,Nome,Rg,Cpf,DataNascimento,NomeResponsavel,Sexo,Etnia,Endereco,Bairro,IdCidade,TelResidencial,TelComercial,TelCelular,Profissao,FlgAtleta,FlgGestante")] TbPaciente tbPaciente)
    {
        if (ModelState.IsValid)
        {
            _context.Add(tbPaciente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(tbPaciente);
    }

    // GET: TbPacientes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tbPaciente = await _context.TbPacientes.FindAsync(id);

        if (tbPaciente == null)
        {
            return NotFound();
        }

        return View(tbPaciente);
    }

    // POST: TbPacientes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int? id, [Bind("IdPaciente,Nome,Rg,Cpf,DataNascimento,NomeResponsavel,Sexo,Etnia,Endereco,Bairro,IdCidade,TelResidencial,TelComercial,TelCelular,Profissao,FlgAtleta,FlgGestante")] TbPaciente tbPaciente)
    {
        if (id != tbPaciente.IdPaciente)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(tbPaciente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TbPacienteExists(tbPaciente.IdPaciente))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(tbPaciente);
    }

    // GET: TbPacientes/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tbPaciente = await _context.TbPacientes
            .FirstOrDefaultAsync(m => m.IdPaciente == id);

        if (tbPaciente == null)
        {
            return NotFound();
        }

        return View(tbPaciente);
    }

    // POST: TbPacientes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var tbPaciente = await _context.TbPacientes.FindAsync(id);
        if (tbPaciente != null)
        {
            _context.TbPacientes.Remove(tbPaciente);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TbPacienteExists(int id)
    {
        return _context.TbPacientes.Any(e => e.IdPaciente == id);
    }
}
