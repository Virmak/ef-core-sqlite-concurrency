using EFCore.Sqlite.Concurrency.Core;
using EFCore.Sqlite.Concurrency.Core.Operation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Moq;

namespace EFCore.Sqlite.Concurrency.Tests;

#pragma warning disable EF1001 // Internal EF Core API usage.
public class SqliteConcurrencyTriggerMigrationSqlGeneratorTests
{
    private static MigrationsSqlGeneratorDependencies GetGeneratorDependencies(Mock<IRelationalCommandBuilder> relationalCommandBuilder)
    {
        var relationalCommandBuilderFactory = new Mock<IRelationalCommandBuilderFactory>();
        relationalCommandBuilderFactory.Setup(x => x.Create())
            .Returns(relationalCommandBuilder.Object);

        return new MigrationsSqlGeneratorDependencies(
                relationalCommandBuilderFactory.Object,
                Mock.Of<IUpdateSqlGenerator>(),
                Mock.Of<ISqlGenerationHelper>(),
                Mock.Of<IRelationalTypeMappingSource>(),
                Mock.Of<ICurrentDbContext>(),
                Mock.Of<IModificationCommandFactory>(),
                Mock.Of<ILoggingOptions>(),
                Mock.Of<IRelationalCommandDiagnosticsLogger>(),
                Mock.Of<IDiagnosticsLogger<DbLoggerCategory.Migrations>>()
            );
    }

    [Test]
    public void Generate_WithCreateConcurrencyTriggerOperation_GeneratesCorrectSql()
    {
        var sqlOutput = "";
        var relationalCommandBuilder = new Mock<IRelationalCommandBuilder>();
        relationalCommandBuilder
            .Setup(x => x.Append(It.IsAny<string>()))
            .Callback<string>(s => sqlOutput += s)
            .Returns(relationalCommandBuilder.Object);
        relationalCommandBuilder
            .Setup(x => x.AppendLine())
            .Callback(() => sqlOutput += "\r\n")
            .Returns(relationalCommandBuilder.Object);
        var sut = new SqliteConcurrencyTriggerMigrationSqlGenerator(
            GetGeneratorDependencies(relationalCommandBuilder),
            Mock.Of<IRelationalAnnotationProvider>()
        );
        const string tableName = "Table";
        const string versionColumn = "col";
        var expectedTriggerName = tableName.GetTriggerName();
        var expectedSql = @$"CREATE TRIGGER {expectedTriggerName}
AFTER UPDATE ON {tableName}
BEGIN
UPDATE {tableName}
SET {versionColumn} = {versionColumn} + 1
WHERE rowid = NEW.rowid;
END;
";

        sut.Generate([new CreateConcurrencyTriggerOperation(tableName, versionColumn)]);

        sqlOutput.Should().Be(expectedSql);
    }

    [Test]
    public void Generate_WithDropConcurrencyTriggerOperation_GeneratesCorrectSql()
    {
        var sqlOutput = "";
        var relationalCommandBuilder = new Mock<IRelationalCommandBuilder>();
        relationalCommandBuilder
            .Setup(x => x.Append(It.IsAny<string>()))
            .Callback<string>(s => sqlOutput += s)
            .Returns(relationalCommandBuilder.Object);
        relationalCommandBuilder
            .Setup(x => x.AppendLine())
            .Callback(() => sqlOutput += "\r\n")
            .Returns(relationalCommandBuilder.Object);
        var sut = new SqliteConcurrencyTriggerMigrationSqlGenerator(
            GetGeneratorDependencies(relationalCommandBuilder),
            Mock.Of<IRelationalAnnotationProvider>()
        );
        const string tableName = "Table";
        var expectedTriggerName = tableName.GetTriggerName();
        var expectedSql = $"DROP TRIGGER {expectedTriggerName}\r\n";

        sut.Generate([new DropConcurrencyTriggerOperation(expectedTriggerName)]);

        sqlOutput.Should().Be(expectedSql);
    }

    [Test]
    public void Generate_WithBuiltinOperation_GeneratesSql()
    {
        var relationalCommandBuilder = new Mock<IRelationalCommandBuilder>();
        relationalCommandBuilder
            .Setup(x => x.Append(It.IsAny<string>()))
            .Returns(relationalCommandBuilder.Object);
        relationalCommandBuilder
            .Setup(x => x.AppendLine())
            .Returns(relationalCommandBuilder.Object);
        var sut = new SqliteConcurrencyTriggerMigrationSqlGenerator(
            GetGeneratorDependencies(relationalCommandBuilder),
            Mock.Of<IRelationalAnnotationProvider>()
        );

        sut.Generate([new CreateTableOperation()]);

        relationalCommandBuilder
            .Verify(x => x.Append(It.IsAny<string>()));
    }
}