﻿using Microsoft.CodeAnalysis;

namespace EFCore.Sqlite.Concurrency.SourceGenerator
{
    [Generator]
    public class HelloSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

            string source = $@"// <auto-generated/>
using Microsoft.EntityFrameworkCore.Design;

[assembly: DesignTimeServicesReference(
""EFCore.Sqlite.Concurrency.EFCoreSqliteConcurrencyDesignTimeServices, EFCore.Sqlite.Concurrency"",
""Microsoft.EntityFrameworkCore.Sqlite"")]
";
            var typeName = mainMethod?.ContainingType.Name ?? string.Empty;

            context.AddSource($"{typeName}_EFCoreSqliteConcurrency.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}