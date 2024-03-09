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
public class CustomMigrationsModelDiffer : MigrationsModelDiffer
{
    public CustomMigrationsModelDiffer(
        IRelationalTypeMappingSource typeMappingSource,
        IMigrationsAnnotationProvider migrationsAnnotationProvider,
        IRowIdentityMapFactory rowIdentityMapFactory,
        CommandBatchPreparerDependencies commandBatchPreparerDependencies)
         : base(typeMappingSource,
                migrationsAnnotationProvider,
                rowIdentityMapFactory,
                commandBatchPreparerDependencies)
    {
    }

    public override IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel? source, IRelationalModel? target)
    {
        var sourceTypes = GetEntityTypesContainingMergeAnnotation(source);
        var targetTypes = GetEntityTypesContainingMergeAnnotation(target);

        var diffContext = new DiffContext();
        var concurrencyMigrationOperations = Diff(sourceTypes, targetTypes, diffContext);
        var createConcurrencyTriggerOperations = concurrencyMigrationOperations
            .OfType<CreateConcurrencyTriggerOperation>();
        var dropConcurrencyTriggerOperations = concurrencyMigrationOperations
            .OfType<DropConcurrencyTriggerOperation>();

        return [
            ..dropConcurrencyTriggerOperations,
            ..base.GetDifferences(source, target),
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

    private IEnumerable<MigrationOperation> Remove(IEntityType source, DiffContext context)
    {
        var tableName = source.GetTableName() ?? string.Empty;
        yield return new DropConcurrencyTriggerOperation(tableName.GetTriggerName());
    }

    private IEnumerable<MigrationOperation> Add(IEntityType target, DiffContext context)
    {
        var tableName = target.GetTableName() ?? string.Empty;
        var versionColumnName = target.GetAnnotation(Constants.ConcurrencyTriggerAnnotationName).Value as string
            ?? Constants.DefaultVersionColumnName;
        yield return new CreateConcurrencyTriggerOperation(
            tableName,
            versionColumnName);
    }

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

    private static List<IEntityType> GetEntityTypesContainingMergeAnnotation(IRelationalModel? relationalModel)
    {
        if (relationalModel == null)
        {
            return [];
        }

        return relationalModel.Model
            .GetEntityTypes()
            .Where(x => x.GetAnnotations().Any(annotation => annotation.Name.Equals(Constants.ConcurrencyTriggerAnnotationName)))
            .ToList();
    }
}
