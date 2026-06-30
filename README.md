# Projeto Final — Programação Web II

Sistema de gestão de **profissionais** (médicos e nutricionistas) e seus **pacientes**, desenvolvido em **ASP.NET Core MVC (.NET 10)** com **Entity Framework Core 10** e autenticação **ASP.NET Core Identity**.

---

## Tecnologias

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core 10 (Database First — banco `db_IF`)
- ASP.NET Core Identity (autenticação + autorização por Roles)
- SQL Server
- Bootstrap 5

---

## Como executar

1. Tenha o **SQL Server** com o banco `db_IF` (o mesmo passado em aula) disponível.
2. Ajuste, se necessário, a connection string `DefaultConnection` em `appsettings.json`:
   ```json
   "Server=localhost;Database=db_IF;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   ```
3. Rode o projeto:
   ```bash
   dotnet run --launch-profile https
   ```
   ou abra no Visual Studio e pressione **F5**.
4. Acesse `https://localhost:7241`.

> Na primeira execução, uma **carga inicial automática** (seeder) cria as Roles, os tipos de profissional, os planos e os 3 usuários gerentes.

---

## Usuários gerentes (criados automaticamente)

| E-mail | Senha | Perfil (Role) | Acesso |
|---|---|---|---|
| `gerente.geral@clinica.com` | `Gerente@123` | GerenteGeral | Todos os profissionais |
| `gerente.medico@clinica.com` | `Gerente@123` | GerenteMedico | Apenas médicos |
| `gerente.nutri@clinica.com` | `Gerente@123` | GerenteNutricionista | Apenas nutricionistas |

Os **profissionais** (médicos/nutricionistas) são criados pela própria tela de **Registro**.

---

## Funcionalidades

### Item 1 — Auto-registro do profissional
- Registro como **Médico** ou **Nutricionista** (Roles distintas), com escolha clara na tela de cadastro.
- Login, senha e dados do profissional preenchidos juntos no Registro.
- Após cadastrado, o profissional só acessa **os próprios dados** (Edit e Details); o **CPF não é editável**.

### Item 2 — Gerentes
- Três gerentes com Roles associadas no banco.
- Cada gerente vê apenas os profissionais do seu escopo; podem **editar, visualizar e excluir** — nunca **criar**.
- Exclusão de profissional permitida apenas quando ele **não possui pacientes**.

### Item 3 — Pacientes
- Cada profissional faz o **CRUD** dos seus pacientes (Create, Edit, Details, Delete).
- A listagem mostra **apenas** os pacientes cadastrados pelo próprio profissional, usando a tabela `tbMedico_Paciente`.

### Itens extras implementados
- **Planos por tipo:** os planos de Médico só aparecem para quem se registra como Médico, e há planos equivalentes para Nutricionista.
- **Deleção em cascata:** ao excluir um profissional com pacientes, é possível remover também os pacientes dos quais ele é o único profissional (além do contrato e do login).

---

## Segurança

- Autorização aplicada por `[Authorize(Roles = ...)]` em **todos os controllers** (não apenas escondendo links).
- Checagem de posse no servidor: um profissional não acessa dados de outro, e um gerente não acessa profissionais fora do seu escopo, mesmo via URL direta.

---

## Estrutura principal

| Caminho | Responsabilidade |
|---|---|
| `Areas/Identity/Pages/Account/Register.cshtml(.cs)` | Registro do profissional (login + dados + role) |
| `Controllers/MeuCadastroController.cs` | Auto-serviço do profissional (Details/Edit, CPF travado) |
| `Controllers/ProfissionaisController.cs` | Área dos gerentes |
| `Controllers/TbPacientesController.cs` | CRUD de pacientes por profissional |
| `Data/SeedData.cs` | Carga inicial (roles, tipos, planos, gerentes) |
| `Authorization/Perfis.cs` | Constantes das Roles |

---

Autor: **Maurício Rimes Vieira**
