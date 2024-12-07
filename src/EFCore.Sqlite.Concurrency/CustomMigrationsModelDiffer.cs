using System.Diagnostics;
using EFCore.Sqlite.Concurrency.Core.Operation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace EFCore.Sqlite.Concurrency.Core;

#pragma warning disable EF1001 // Internal EF Core API usage.
internal class CustomMigrationsModelDiffer : MigrationsModelDiffer
{
    public CustomMigrationsModelDiffer(
        IRelationalTypeMappingSource typeMappingSource,
        IMigrationsAnnotationProvider migrationsAnnotationProvider,
        IRelationalAnnotationProvider relationalAnnotationProvider,
        IRowIdentityMapFactory rowIdentityMapFactory,
        CommandBatchPreparerDependencies commandBatchPreparerDependencies)
         : base(typeMappingSource,
                migrationsAnnotationProvider,
                relationalAnnotationProvider,
                rowIdentityMapFactory,
                commandBatchPreparerDependencies)
    {
    }

    public override IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel? source, IRelationalModel? target)
    {
        var sourceTypes = GetEntityTypesContainingConcurrencyAnnotation(source);
        var targetTypes = GetEntityTypesContainingConcurrencyAnnotation(target);

        var diffContext = new DiffContext();
        var concurrencyMigrationOperations = Diff(sourceTypes, targetTypes, diffContext);
        var createConcurrencyTriggerOperations = concurrencyMigrationOperations
            .OfType<CreateConcurrencyTriggerOperation>()
            .ToList();
        var dropConcurrencyTriggerOperations = concurrencyMigrationOperations
            .OfType<DropConcurrencyTriggerOperation>()
            .ToList();

        var diffs = base.GetDifferences(source, target);

        return [
            ..dropConcurrencyTriggerOperations,
            ..diffs,
            ..createConcurrencyTriggerOperations
        ];
    }

    private IEnumerable<MigrationOperation> Diff(
        IEnumerable<IEntityType> source,
        IEnumerable<IEntityType> target,
        DiffContext diffContext)
        => DiffCollection(
            source,
            target,
            diffContext,
            Diff,
            Add,
            Remove,
            (x, y, diff) => x.Name.Equals(y.Name, StringComparison.CurrentCultureIgnoreCase));

    private IEnumerable<MigrationOperation> Diff(IEntityType source, IEntityType target, DiffContext context)
    {
        if (source == target)
        {
            yield break;
        }

        var dropOperations = Remove(source, context);
        foreach (var operation in dropOperations)
        {
            yield return operation;
        }

        var addOperations = Add(target, context);
        foreach (var operation in addOperations)
        {
            yield return operation;
        }
    }

    private IEnumerable<MigrationOperation> Remove(IEntityType source, DiffContext context)
    {
        var tableName = source.GetTableName() ?? string.Empty;
        yield return new DropConcurrencyTriggerOperation(tableName.GetTriggerName());
    }

    private IEnumerable<MigrationOperation> Add(IEntityType target, DiffContext context)
    {
        var tableName = target.GetTableName() ?? string.Empty;
        var versionColumnName = target.GetProperties().FirstOrDefault(x => x.IsConcurrencyToken)?.GetColumnName()
            ?? Constants.DefaultVersionColumnName;
        yield return new CreateConcurrencyTriggerOperation(
            tableName,
            versionColumnName);
    }

    private static List<IEntityType> GetEntityTypesContainingConcurrencyAnnotation(IRelationalModel? relationalModel)
    {
        if (relationalModel == null)
        {
            return [];
        }

        return relationalModel.Model
            .GetEntityTypes()
            .Where(x => x.GetProperties().Any(x => x.IsConcurrencyToken))
            .ToList();
    }
}
