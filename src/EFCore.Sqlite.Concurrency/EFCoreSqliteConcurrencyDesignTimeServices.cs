using EFCore.Sqlite.Concurrency.Core;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Sqlite.Concurrency;

public sealed class EFCoreSqliteConcurrencyDesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IMigrationsCodeGenerator, CSharpConcurrencyTriggerMigrationsGenerator>();
        serviceCollection.AddSingleton<ICSharpMigrationOperationGenerator, CSharpConcurrencyTriggerMigrationOperationGenerator>();
    }
}