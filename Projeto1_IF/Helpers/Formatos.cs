// Maurício Rimes Vieira
namespace Projeto1_IF.Helpers;

/// <summary>
/// Converte os valores "crus" do banco (códigos numéricos, flags, sigla de sexo)
/// em textos amigáveis para exibição nas telas de detalhe/lista.
/// </summary>
public static class Formatos
{
    public static string Etnia(int valor) => valor switch
    {
        1 => "Branca",
        2 => "Preta",
        3 => "Parda",
        4 => "Amarela",
        5 => "Indígena",
        _ => "Não informado"
    };

    public static string Sexo(string? valor) => valor switch
    {
        "M" => "Masculino",
        "F" => "Feminino",
        _ => valor ?? ""
    };

    public static string SimNao(bool? valor) => valor switch
    {
        true => "Sim",
        false => "Não",
        _ => "Não informado"
    };
}
