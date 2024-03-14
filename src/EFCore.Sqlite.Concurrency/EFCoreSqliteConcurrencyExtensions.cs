using System.Linq.Expressions;
using EFCore.Sqlite.Concurrency.Core;
using EFCore.Sqlite.Concurrency.Core.Operation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace EFCore.Sqlite.Concurrency;

public static class EFCoreSqliteConcurrencyExtensions
{
    /// <summary>
    /// Adds support for optimistic concurrency in SQLite databases to the DbContextOptionsBuilder.
    /// </summary>
    /// <param name="options">The DbContextOptionsBuilder to configure SQLite concurrency support.</param>
    /// <remarks>
    /// Replaces the default services for migrations model differ and migrations SQL generator with custom implementations.
    /// </remarks>
    public static void AddSqliteConcurrency(this DbContextOptionsBuilder options)
    {
        options.ReplaceService<IMigrationsModelDiffer, CustomMigrationsModelDiffer>();
        options.ReplaceService<IMigrationsSqlGenerator, SqliteConcurrencyTriggerMigrationSqlGenerator>();
    }

    public static OperationBuilder<CreateConcurrencyTriggerOperation> CreateConcurrencyTrigger(
        this MigrationBuilder migrationBuilder,
        string tableName,
        string versionColumnName)
    {
        var operation = new CreateConcurrencyTriggerOperation(tableName, versionColumnName);
        migrationBuilder.Operations.Add(operation);

        return new OperationBuilder<CreateConcurrencyTriggerOperation>(operation);
    }

    public static OperationBuilder<DropConcurrencyTriggerOperation> DropConcurrencyTrigger(
        this MigrationBuilder migrationBuilder, string triggerName)
    {
        var operation = new DropConcurrencyTriggerOperation(triggerName);
        migrationBuilder.Operations.Add(operation);

        return new OperationBuilder<DropConcurrencyTriggerOperation>(operation);
    }

    private static string GetPropertyName<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        MemberExpression? memberExpression = null;

        if (propertyExpression.Body is MemberExpression)
        {
            memberExpression = (MemberExpression)propertyExpression.Body;
        }
        else if (propertyExpression.Body is UnaryExpression unaryExpression)
        {
            memberExpression = (MemberExpression)unaryExpression.Operand;
        }

        if (memberExpression == null)
        {
            throw new ArgumentException("Invalid expression. Expression should consist of a single member access expression.", nameof(propertyExpression));
        }

        return memberExpression.Member.Name;
    }
}