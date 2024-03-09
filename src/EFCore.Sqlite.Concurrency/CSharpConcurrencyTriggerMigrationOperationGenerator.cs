using System.Diagnostics;
using EFCore.Sqlite.Concurrency.Core.Operation;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.Sqlite.Concurrency.Core;

public class CSharpConcurrencyTriggerMigrationOperationGenerator : CSharpMigrationOperationGenerator
{
    public CSharpConcurrencyTriggerMigrationOperationGenerator(
        CSharpMigrationOperationGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    protected override void Generate(MigrationOperation operation, IndentedStringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(operation, nameof(operation));        
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        switch (operation)
        {
            case CreateConcurrencyTriggerOperation create:
                Generate(create, builder);
                break;

            case DropConcurrencyTriggerOperation drop:
                Generate(drop, builder);
                break;

            default:
                base.Generate(operation, builder);
                break;
        }
    }

    private static void Generate(CreateConcurrencyTriggerOperation operation, IndentedStringBuilder builder)
    {
        builder.Append(
            $".{nameof(EFCoreSqliteConcurrencyExtensions.CreateConcurrencyTrigger)}(\"{operation.TableName}\", \"{operation.VersionColumnName}\")");
    }

    private static void Generate(DropConcurrencyTriggerOperation operation, IndentedStringBuilder builder)
    {
        builder.Append(
            $".{nameof(EFCoreSqliteConcurrencyExtensions.DropConcurrencyTrigger)}(\"{operation.TriggerName}\")");
    }
}