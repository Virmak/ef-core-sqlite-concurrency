using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.Sqlite.Concurrency.Core;

internal class CSharpConcurrencyTriggerMigrationsGenerator : CSharpMigrationsGenerator
{
    public CSharpConcurrencyTriggerMigrationsGenerator(
        MigrationsCodeGeneratorDependencies dependencies,
        CSharpMigrationsGeneratorDependencies csharpDependencies)
        : base(dependencies, csharpDependencies)
    {
    }

    protected override IEnumerable<string> GetNamespaces(IEnumerable<MigrationOperation> operations)
        => base.GetNamespaces(operations).Concat([typeof(EFCoreSqliteConcurrencyExtensions).Namespace]);
}
