using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Models;

// Maurício Rimes Vieira
namespace Projeto1_IF.Controllers;

[Authorize]
public class TbAlimentosController : Controller
{
    private readonly db_IFContext _context;

    public TbAlimentosController(db_IFContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.TbAlimentos.ToListAsync());
    }

    public async Task<IActionResult> Details(int? idalimento)
    {
        if (idalimento == null)
        {
            return NotFound();
        }

        var tbalimento = await _context.TbAlimentos
            .FirstOrDefaultAsync(m => m.IdAlimento == idalimento);
        if (tbalimento == null)
        {
            return NotFound();
        }

        return View(tbalimento);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdAlimento,IdTipoQuantidade,Nome,Carboidrato,VitaminaA,VitaminaB,TbReceitaAlimentarPadraoXAlimentos")] TbAlimento tbalimento)
    {
        if (ModelState.IsValid)
        {
            _context.Add(tbalimento);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(tbalimento);
    }

    public async Task<IActionResult> Edit(int? idalimento)
    {
        if (idalimento == null)
        {
            return NotFound();
        }

        var tbalimento = await _context.TbAlimentos.FindAsync(idalimento);
        if (tbalimento == null)
        {
            return NotFound();
        }
        return View(tbalimento);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? idalimento, [Bind("IdAlimento,IdTipoQuantidade,Nome,Carboidrato,VitaminaA,VitaminaB,TbReceitaAlimentarPadraoXAlimentos")] TbAlimento tbalimento)
    {
        if (idalimento != tbalimento.IdAlimento)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(tbalimento);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TbAlimentoExists(tbalimento.IdAlimento))
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
        return View(tbalimento);
    }

    public async Task<IActionResult> Delete(int? idalimento)
    {
        if (idalimento == null)
        {
            return NotFound();
        }

        var tbalimento = await _context.TbAlimentos
            .FirstOrDefaultAsync(m => m.IdAlimento == idalimento);
        if (tbalimento == null)
        {
            return NotFound();
        }

        return View(tbalimento);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? idalimento)
    {
        var tbalimento = await _context.TbAlimentos.FindAsync(idalimento);
        if (tbalimento != null)
        {
            _context.TbAlimentos.Remove(tbalimento);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TbAlimentoExists(int? idalimento)
    {
        return _context.TbAlimentos.Any(e => e.IdAlimento == idalimento);
    }
}
