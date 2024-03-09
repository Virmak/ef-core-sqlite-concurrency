namespace EFCore.Sqlite.Concurrency.Core;

internal static class Constants
{
    public static readonly string ConcurrencyTriggerAnnotationName = "ConcurLiteConcurrencyTrigger";

    public static readonly string DefaultVersionColumnName = "_CL__Version";
}
