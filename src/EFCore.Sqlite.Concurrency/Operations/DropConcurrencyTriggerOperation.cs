using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.Sqlite.Concurrency.Core.Operation;

public class DropConcurrencyTriggerOperation : MigrationOperation
{
    public DropConcurrencyTriggerOperation(string triggerName)
    {
        TriggerName = triggerName;
    }

    public string TriggerName { get; }
}