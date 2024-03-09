using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.Sqlite.Concurrency.Core.Operation;

public class CreateConcurrencyTriggerOperation : MigrationOperation
{
    public CreateConcurrencyTriggerOperation(
        string tableName,
        string versionColumnName)
    {
        TableName = tableName;
        VersionColumnName = versionColumnName;
    }

    public string TableName { get; }

    public string? VersionColumnName { get; }
}