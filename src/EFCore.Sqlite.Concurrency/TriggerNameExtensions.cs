namespace EFCore.Sqlite.Concurrency.Core;

internal static class TriggerNameExtensions
{
    private static readonly string TriggerPrefix = "Update";
    private static readonly string TriggerPostfix = "Version";

    public static string GetTriggerName(this string tableName)
    {
        return $"{TriggerPrefix}{tableName}{TriggerPostfix}";
    }
}
