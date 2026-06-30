// Maurício Rimes Vieira
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Projeto1_IF.Authorization;
using Projeto1_IF.Data;
using Projeto1_IF.Models;

namespace Projeto1_IF.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly db_IFContext _context;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        db_IFContext context)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    /// <summary>Cidades para o dropdown de endereço do profissional.</summary>
    public SelectList Cidades { get; set; } = default!;

    /// <summary>Planos disponíveis; cada um marcado com o tipo (Médico/Nutricionista) para filtragem.</summary>
    public List<PlanoItem> Planos { get; set; } = new();

    public class PlanoItem
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Tipo { get; set; } = "";
        public decimal Valor { get; set; }
    }

    public class InputModel
    {
        // --- Dados de login -------------------------------------------------
        [Required]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter no mínimo {2} e no máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar senha")]
        [Compare("Password", ErrorMessage = "A senha e a confirmação não conferem.")]
        public string? ConfirmPassword { get; set; }

        // --- Tipo de profissional / plano -----------------------------------
        [Required(ErrorMessage = "Escolha o tipo de profissional.")]
        [Display(Name = "Registrar-se como")]
        public string TipoProfissional { get; set; } = default!; // "Medico" ou "Nutricionista"

        [Required(ErrorMessage = "Selecione um plano.")]
        [Display(Name = "Plano")]
        public int IdPlano { get; set; }

        // --- Dados do profissional ------------------------------------------
        [Required]
        [StringLength(100)]
        [Display(Name = "Nome completo")]
        public string Nome { get; set; } = default!;

        [Required]
        [StringLength(15)]
        [Display(Name = "CPF")]
        public string Cpf { get; set; } = default!;

        [StringLength(20)]
        [Display(Name = "CRM / CRN")]
        public string? CrmCrn { get; set; }

        [StringLength(100)]
        [Display(Name = "Especialidade")]
        public string? Especialidade { get; set; }

        // --- Endereço -------------------------------------------------------
        [Required]
        [Display(Name = "Cidade")]
        public int IdCidade { get; set; }

        [StringLength(100)]
        [Display(Name = "Logradouro")]
        public string? Logradouro { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Número")]
        public string Numero { get; set; } = default!;

        [Required]
        [StringLength(100)]
        [Display(Name = "Bairro")]
        public string Bairro { get; set; } = default!;

        [Required]
        [StringLength(10)]
        [Display(Name = "CEP")]
        public string Cep { get; set; } = default!;

        // --- Contato --------------------------------------------------------
        [StringLength(2)]
        [Display(Name = "DDD")]
        public string? Ddd1 { get; set; }

        [StringLength(25)]
        [Display(Name = "Telefone")]
        public string? Telefone1 { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        await CarregarListasAsync();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        await CarregarListasAsync();

        if (!ModelState.IsValid)
            return Page();

        // Determina a role e o tipo de profissional a partir da escolha do usuário.
        var (role, tipoNome) = Input.TipoProfissional == Perfis.Medico
            ? (Perfis.Medico, TiposProfissional.Medico)
            : (Perfis.Nutricionista, TiposProfissional.Nutricionista);

        // Blindagem no servidor: garante que o plano escolhido é do mesmo tipo do
        // profissional, mesmo que o filtro do navegador (JS) seja burlado.
        var planoEscolhido = await _context.TbPlanos.FirstOrDefaultAsync(p => p.IdPlano == Input.IdPlano);
        var tipoDoPlano = planoEscolhido != null && planoEscolhido.Nome.StartsWith("Médico")
            ? Perfis.Medico
            : Perfis.Nutricionista;
        if (planoEscolhido is null || tipoDoPlano != Input.TipoProfissional)
        {
            ModelState.AddModelError(string.Empty,
                "O plano selecionado não corresponde ao tipo de profissional escolhido.");
            return Page();
        }

        var user = CreateUser();
        await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        try
        {
            _logger.LogInformation("Novo usuário registrado com senha.");

            await _userManager.AddToRoleAsync(user, role);

            var tipo = await _context.TbTipoProfissionals.FirstAsync(t => t.Nome == tipoNome);
            var plano = await _context.TbPlanos.FirstAsync(p => p.IdPlano == Input.IdPlano);
            var cidade = await _context.TbCidades.FirstAsync(c => c.IdCidade == Input.IdCidade);

            // Contrato gerado a partir do plano escolhido (como visto com o contrato do profissional).
            var contrato = new TbContrato
            {
                IdPlano = plano.IdPlano,
                DataInicio = DateTime.Now,
                DataFim = DateTime.Now.AddDays(plano.Validade)
            };
            _context.TbContratos.Add(contrato);
            await _context.SaveChangesAsync();

            var profissional = new TbProfissional
            {
                IdUser = user.Id,
                IdTipoProfissional = tipo.IdTipoProfissional,
                IdContrato = contrato.IdContrato,
                IdCidade = cidade.IdCidade,
                Nome = Input.Nome,
                Cpf = Input.Cpf,
                CrmCrn = Input.CrmCrn,
                Especialidade = Input.Especialidade,
                Logradouro = Input.Logradouro,
                Numero = Input.Numero,
                Bairro = Input.Bairro,
                Cep = Input.Cep,
                Cidade = cidade.Nome,
                Ddd1 = Input.Ddd1,
                Telefone1 = Input.Telefone1
            };
            _context.TbProfissionals.Add(profissional);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Se a criação do profissional falhar, desfaz o usuário para não deixar login órfão.
            _logger.LogError(ex, "Falha ao criar o profissional; removendo o usuário criado.");
            await _userManager.DeleteAsync(user);
            ModelState.AddModelError(string.Empty, "Não foi possível concluir o cadastro do profissional. Tente novamente.");
            return Page();
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(returnUrl);
    }

    private async Task CarregarListasAsync()
    {
        Cidades = new SelectList(
            await _context.TbCidades.OrderBy(c => c.Nome).ToListAsync(),
            nameof(TbCidade.IdCidade), nameof(TbCidade.Nome));

        Planos = await _context.TbPlanos
            .OrderBy(p => p.Nome)
            .Select(p => new PlanoItem
            {
                Id = p.IdPlano,
                Nome = p.Nome,
                Valor = p.Valor,
                // Convenção de nomes definida no seeder: "Médico ..." x "Nutricionista ...".
                Tipo = p.Nome.StartsWith("Médico") ? Perfis.Medico : Perfis.Nutricionista
            })
            .ToListAsync();
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<ApplicationUser>)_userStore;
    }
}
