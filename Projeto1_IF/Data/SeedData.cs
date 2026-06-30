// Maurício Rimes Vieira
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Authorization;
using Projeto1_IF.Models;

namespace Projeto1_IF.Data;

/// <summary>
/// Carga inicial executada no startup: cria as Roles, os tipos de profissional,
/// os planos (Médico/Nutricionista) e os três usuários gerentes já associados
/// às suas autorizações diretamente no banco.
/// </summary>
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var db = sp.GetRequiredService<db_IFContext>();

        // 1) Roles -------------------------------------------------------------
        string[] roles =
        {
            Perfis.Medico, Perfis.Nutricionista,
            Perfis.GerenteMedico, Perfis.GerenteNutricionista, Perfis.GerenteGeral
        };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2) Tipos de profissional --------------------------------------------
        await GarantirTipoProfissionalAsync(db, TiposProfissional.Medico);
        await GarantirTipoProfissionalAsync(db, TiposProfissional.Nutricionista);
        await db.SaveChangesAsync();

        // 3) Planos (dois para Médico e dois para Nutricionista) ---------------
        await GarantirPlanoAsync(db, "Médico Total", 365, 200.00m);
        await GarantirPlanoAsync(db, "Médico Parcial", 365, 120.00m);
        await GarantirPlanoAsync(db, "Nutricionista Total", 365, 180.00m);
        await GarantirPlanoAsync(db, "Nutricionista Parcial", 365, 100.00m);
        await db.SaveChangesAsync();

        // 4) Gerentes (usuário + associação da Role no banco) ------------------
        await GarantirGerenteAsync(userManager, "gerente.medico@clinica.com", "Gerente@123", Perfis.GerenteMedico);
        await GarantirGerenteAsync(userManager, "gerente.nutri@clinica.com", "Gerente@123", Perfis.GerenteNutricionista);
        await GarantirGerenteAsync(userManager, "gerente.geral@clinica.com", "Gerente@123", Perfis.GerenteGeral);
    }

    private static async Task GarantirTipoProfissionalAsync(db_IFContext db, string nome)
    {
        if (!await db.TbTipoProfissionals.AnyAsync(t => t.Nome == nome))
            db.TbTipoProfissionals.Add(new TbTipoProfissional { Nome = nome });
    }

    private static async Task GarantirPlanoAsync(db_IFContext db, string nome, int validade, decimal valor)
    {
        if (!await db.TbPlanos.AnyAsync(p => p.Nome == nome))
            db.TbPlanos.Add(new TbPlano { Nome = nome, Validade = validade, Valor = valor });
    }

    private static async Task GarantirGerenteAsync(
        UserManager<ApplicationUser> userManager, string email, string senha, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var resultado = await userManager.CreateAsync(user, senha);
            if (!resultado.Succeeded)
                return; // Em caso de falha não interrompe o startup do restante da aplicação.
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);
    }
}
