using DbUp;
using DbUp.Engine;
using Microsoft.Extensions.Logging;

namespace MeuCondominio.Infrastructure.Persistence;

/// <summary>
/// Executa migrações SQL versionadas usando DbUp.
///
/// Convenção de nomenclatura dos scripts:
///   V{número}__DescricaoSnakeCase.sql
///   Ex: V001__InitialSchema.sql
///       V002__AddIndexEncomendas.sql
///
/// Os scripts são embutidos no assembly (EmbeddedResource) e executados
/// em ordem crescente. Scripts já executados são rastreados pela tabela
/// "schemaversions" criada automaticamente pelo DbUp no banco.
///
/// Uso no Program.cs (antes de app.Run()):
///   DatabaseMigrationRunner.Executar(connectionString, logger);
/// </summary>
public static class DatabaseMigrationRunner
{
    public static void Executar(string connectionString, ILogger? logger = null)
    {
        // Garante que o banco exista (idempotente — usa IF NOT EXISTS internamente)
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            // Carrega todos os .sql embutidos neste assembly ordenados pelo nome
            .WithScriptsEmbeddedInAssembly(
                typeof(DatabaseMigrationRunner).Assembly,
                s => s.Contains("Persistence.Migrations"))
            .WithTransactionPerScript()          // Cada script roda em sua própria transação
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            logger?.LogError(
                result.Error,
                "Falha na migração do banco de dados: {Mensagem}",
                result.Error.Message);

            throw new InvalidOperationException(
                $"Falha na migração do banco de dados: {result.Error.Message}",
                result.Error);
        }

        logger?.LogInformation(
            "Migrações aplicadas com sucesso. Scripts executados: {Count}",
            result.Scripts.Count());
    }
}
