// Maurício Rimes Vieira
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Projeto1_IF.Models;

// Rótulos amigáveis para a tela (Display) sem alterar o model gerado pelo scaffold
// nem o design do banco. A classe parcial apenas "anexa" os metadados ao TbProfissional.
[ModelMetadataType(typeof(TbProfissionalMetadata))]
public partial class TbProfissional
{
}

public class TbProfissionalMetadata
{
    [Display(Name = "CPF")]
    public string?Cpf { get; set; }

    [Display(Name = "CRM / CRN")]
    public string?CrmCrn { get; set; }

    [Display(Name = "Endereço")]
    public string?Logradouro { get; set; }

    [Display(Name = "Número")]
    public string?Numero { get; set; }

    [Display(Name = "CEP")]
    public string?Cep { get; set; }

    [Display(Name = "DDD")]
    public string?Ddd1 { get; set; }

    [Display(Name = "Telefone")]
    public string?Telefone1 { get; set; }

    [Display(Name = "DDD (2)")]
    public string?Ddd2 { get; set; }

    [Display(Name = "Telefone (2)")]
    public string?Telefone2 { get; set; }

    [Display(Name = "Salário")]
    public decimal? Salario { get; set; }
}
