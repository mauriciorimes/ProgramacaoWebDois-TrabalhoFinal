// Maurício Rimes Vieira
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Projeto1_IF.Models;

// Rótulos amigáveis para as telas do paciente (Display), sem alterar o model
// gerado pelo scaffold nem o design do banco.
[ModelMetadataType(typeof(TbPacienteMetadata))]
public partial class TbPaciente
{
}

public class TbPacienteMetadata
{
    [Display(Name = "RG")]
    public string? Rg { get; set; }

    [Display(Name = "CPF")]
    public string? Cpf { get; set; }

    [Display(Name = "Data de Nascimento")]
    public DateOnly DataNascimento { get; set; }

    [Display(Name = "Nome do Responsável")]
    public string? NomeResponsavel { get; set; }

    [Display(Name = "Endereço")]
    public string? Endereco { get; set; }

    [Display(Name = "Cidade")]
    public int? IdCidade { get; set; }

    [Display(Name = "Telefone Residencial")]
    public string? TelResidencial { get; set; }

    [Display(Name = "Telefone Comercial")]
    public string? TelComercial { get; set; }

    [Display(Name = "Telefone Celular")]
    public string? TelCelular { get; set; }

    [Display(Name = "Profissão")]
    public string? Profissao { get; set; }

    [Display(Name = "Atleta?")]
    public bool? FlgAtleta { get; set; }

    [Display(Name = "Gestante?")]
    public bool? FlgGestante { get; set; }
}
