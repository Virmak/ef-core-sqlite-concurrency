﻿using EFCore.Sqlite.Concurrency.Core;
using EFCore.Sqlite.Concurrency.Core.Operation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Moq;

namespace EFCore.Sqlite.Concurrency.Tests;

#pragma warning disable EF1001 // Internal EF Core API usage.§§
public class CustomMigrationsModelDifferTests
{
    [Test]
    public void GetDifferences_WhenSourceIsAnnotatedWithConcurrencyTokenAndTargetIsNotAnnotated_ReturnsDropTriggerOperation()
    {
        var sut = new CustomMigrationsModelDiffer(
            Mock.Of<IRelationalTypeMappingSource>(),
            Mock.Of<IMigrationsAnnotationProvider>(),
            Mock.Of<IRelationalAnnotationProvider>(),
            Mock.Of<IRowIdentityMapFactory>(),
            GetCommandBatchPreparerDependencies());
        Mock<IRelationalModel> sourceMock = GetAnnotatedRelationalModelMock();
        var targetModel = new Mock<IModel>();
        targetModel
            .Setup(x => x.GetEntityTypes())
            .Returns([]);
        var targetMock = new Mock<IRelationalModel>();
        targetMock
            .SetupGet(x => x.Model)
            .Returns(targetModel.Object);

        var diff = sut.GetDifferences(sourceMock.Object, targetMock.Object);

        diff.Should().ContainItemsAssignableTo<DropConcurrencyTriggerOperation>();
    }

    [Test]
    public void GetDifferences_WhenTargetIsAnnotatedWithConcurrencyTokenAndSourceIsNotAnnotated_ReturnsCreateTriggerOperation()
    {
        var sut = new CustomMigrationsModelDiffer(
            Mock.Of<IRelationalTypeMappingSource>(),
            Mock.Of<IMigrationsAnnotationProvider>(),
            Mock.Of<IRelationalAnnotationProvider>(),
            Mock.Of<IRowIdentityMapFactory>(),
            GetCommandBatchPreparerDependencies());
        var targetMock = GetAnnotatedRelationalModelMock();
        var sourceModel = new Mock<IModel>();
        sourceModel
            .Setup(x => x.GetEntityTypes())
            .Returns([]);
        var sourceMock = new Mock<IRelationalModel>();
        sourceMock
            .SetupGet(x => x.Model)
            .Returns(sourceModel.Object);

        var diff = sut.GetDifferences(sourceMock.Object, targetMock.Object);

        diff.Should().ContainItemsAssignableTo<CreateConcurrencyTriggerOperation>();
    }

    private static Mock<IRelationalModel> GetAnnotatedRelationalModelMock()
    {
        var tableNameAnnotationMock = new Mock<IAnnotation>();
        tableNameAnnotationMock
            .SetupGet(x => x.Value)
            .Returns("TableName");

        var modelMock = new Mock<IModel>();
        var modelEntityType = new Mock<IEntityType>();
        modelEntityType
            .Setup(x => x.FindAnnotation(It.IsAny<string>()))
            .Returns(tableNameAnnotationMock.Object);

        var propertyMock = new Mock<IProperty>();
        propertyMock.SetupAllProperties();
        
        var declaringTypeMock = new Mock<ITypeBase>();

        declaringTypeMock.SetupGet(x => x.Model)
            .Returns(() => modelMock.Object);
        declaringTypeMock.As<IReadOnlyTypeBase>()
            .SetupGet(x => x.Model)
            .Returns(()=> modelMock.As<IReadOnlyModel>().Object);
        
        propertyMock
            .SetupGet(x => x.IsConcurrencyToken)
            .Returns(true);
        propertyMock
            .SetupGet(x => x.DeclaringType)
            .Returns(() => declaringTypeMock.Object);
        propertyMock.SetupGet(x => x.Name)
            .Returns("PropertyName");
        
        propertyMock.As<IReadOnlyProperty>()
            .SetupGet(x => x.DeclaringType)
            .Returns(() => declaringTypeMock.As<IReadOnlyTypeBase>().Object);

        modelEntityType
            .Setup(x => x.GetProperties())
            .Returns([propertyMock.Object]);
        var firstTime = true;
        modelMock
            .Setup(x => x.GetEntityTypes())
            .Returns(() =>
            {
                if (firstTime)
                {
                    firstTime = false;
                    return [modelEntityType.Object];
                }

                return [];
            });
        var sourceMock = new Mock<IRelationalModel>();
        sourceMock
            .SetupGet(x => x.Model)
            .Returns(modelMock.Object);

        return sourceMock;
    }

    private static CommandBatchPreparerDependencies GetCommandBatchPreparerDependencies()
    {
        return new CommandBatchPreparerDependencies(
            Mock.Of<IModificationCommandBatchFactory>(),
            Mock.Of<IParameterNameGeneratorFactory>(),
            Mock.Of<IComparer<IReadOnlyModificationCommand>>(),
            Mock.Of<IModificationCommandFactory>(),
            Mock.Of<ILoggingOptions>(),
            Mock.Of<IDiagnosticsLogger<Microsoft.EntityFrameworkCore.DbLoggerCategory.Update>>(),
            Mock.Of<IDbContextOptions>());
    }
}
