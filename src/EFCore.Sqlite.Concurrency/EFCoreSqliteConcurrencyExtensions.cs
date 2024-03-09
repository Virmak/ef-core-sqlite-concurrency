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
    public static void AddSqliteConcurrency(this DbContextOptionsBuilder options)
    {
        options.ReplaceService<IMigrationsModelDiffer, CustomMigrationsModelDiffer>();
        options.ReplaceService<IMigrationsSqlGenerator, SqliteConcurrencyTriggerMigrationSqlGenerator>();
    }

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
            .IsConcurrencyToken();
    }

    public static void HasConcurrencyToken<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        string? columnName = null)
        where TEntity : class
    {
        builder.HasAnnotation(Constants.ConcurrencyTriggerAnnotationName, columnName);

        builder
            .Property<int>(columnName ?? Constants.DefaultVersionColumnName)
            .IsRowVersion()
            .IsConcurrencyToken();
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