using EFCore.Sqlite.Concurrency.Core.Operation;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.Sqlite.Concurrency.Core;

internal class SqliteConcurrencyTriggerMigrationSqlGenerator : SqliteMigrationsSqlGenerator
{
    public SqliteConcurrencyTriggerMigrationSqlGenerator(
        MigrationsSqlGeneratorDependencies dependencies,
        IRelationalAnnotationProvider migrationsAnnotations)
        : base(dependencies, migrationsAnnotations)
    {
    }

    protected override void Generate(
        MigrationOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        switch (operation)
        {
            case CreateConcurrencyTriggerOperation createOperation:
                Generate(createOperation, builder, Dependencies.TypeMappingSource);
                break;

            case DropConcurrencyTriggerOperation dropOperation:
                Generate(dropOperation, builder);
                break;

            default:
                base.Generate(operation, model, builder);
                break;
        }
    }

    private void Generate(DropConcurrencyTriggerOperation operation, MigrationCommandListBuilder builder)
    {
        builder.AppendLine($"DROP TRIGGER {operation.TriggerName}");
        builder.EndCommand();
    }

    private void Generate(CreateConcurrencyTriggerOperation operation,
        MigrationCommandListBuilder builder, IRelationalTypeMappingSource typeMappingSource)
    {
        // CREATE TRIGGER UpdateCustomerVersion
        // AFTER UPDATE ON Customers
        // BEGIN
        //     UPDATE Customers
        //     SET Version = Version + 1
        //     WHERE rowid = NEW.rowid;
        // END;

        builder.AppendLine($"CREATE TRIGGER {operation.TableName.GetTriggerName()}");
        builder.AppendLine($"AFTER UPDATE ON {operation.TableName}");
        builder.AppendLine("BEGIN");

        builder.AppendLine($"UPDATE {operation.TableName}");
        builder.AppendLine($"SET {operation.VersionColumnName} = {operation.VersionColumnName} + 1");
        builder.AppendLine("WHERE rowid = NEW.rowid;");

        builder.AppendLine("END;");

        builder.EndCommand();
    }
}