using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCore.Sqlite.Concurrency.Core;

internal class CreateConcurrencyAnnotation : IAnnotation
{
    public string Name => Constants.ConcurrencyTriggerAnnotationName;

    public object? Value { get; }
}