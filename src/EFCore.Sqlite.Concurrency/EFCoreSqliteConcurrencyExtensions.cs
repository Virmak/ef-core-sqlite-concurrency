using System.Linq.Expressions;
using EFCore.Sqlite.Concurrency.Core;
using EFCore.Sqlite.Concurrency.Core.Operation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

    /// <summary>
    /// Configures the specified property of the entity to act as a concurrency token for optimistic concurrency control.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TProperty">The type of the property to be used as the concurrency token.</typeparam>
    /// <param name="builder">The EntityTypeBuilder for the entity being configured.</param>
    /// <param name="propertyExpression">An expression specifying the property to be used as the concurrency token.</param>
    /// <remarks>
    /// Configures the specified property as a concurrency token with row versioning.
    /// </remarks>
    public static void HasConcurrencyToken<TEntity, TProperty>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, TProperty>> propertyExpression)
        where TEntity : class
    {
        var columnName = GetPropertyName(propertyExpression);
        builder.HasAnnotation(Constants.ConcurrencyTriggerAnnotationName, columnName);

        builder
            .Property(propertyExpression)
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasDefaultValue(0);
    }

    /// <summary>
    /// Configures the specified entity to include a concurrency token for optimistic concurrency control.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="builder">The EntityTypeBuilder for the entity being configured.</param>
    /// <param name="columnName">Optional. The name of the column to use for the concurrency token.</param>
    /// <remarks>
    /// Configures the specified column (or default column) as a concurrency token with row versioning.
    /// </remarks>
    public static void HasConcurrencyToken<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        string? columnName = null)
        where TEntity : class
    {
        builder.HasAnnotation(Constants.ConcurrencyTriggerAnnotationName, columnName);

        builder
            .Property<int>(columnName ?? Constants.DefaultVersionColumnName)
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasDefaultValue(0);
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