// Maurício Rimes Vieira
namespace Projeto1_IF.Authorization;

/// <summary>
/// Nomes das autorizações (Roles) usadas no sistema. Centralizados aqui para
/// evitar "strings mágicas" espalhadas pelos controllers.
/// </summary>
public static class Perfis
{
    public const string Medico = "Medico";
    public const string Nutricionista = "Nutricionista";
    public const string GerenteMedico = "GerenteMedico";
    public const string GerenteNutricionista = "GerenteNutricionista";
    public const string GerenteGeral = "GerenteGeral";

    // Grupos prontos para uso no atributo [Authorize(Roles = ...)]
    public const string Profissionais = Medico + "," + Nutricionista;
    public const string Gerentes = GerenteMedico + "," + GerenteNutricionista + "," + GerenteGeral;
}

/// <summary>
/// Nomes gravados em tbTipoProfissional. O cadastro grava o tipo do profissional
/// usando exatamente estes valores, e os gerentes filtram os profissionais por eles.
/// </summary>
public static class TiposProfissional
{
    public const string Medico = "Médico";
    public const string Nutricionista = "Nutricionista";
}
