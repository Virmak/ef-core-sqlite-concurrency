# EF Core SQLite Concurrency Extension

## Overview

This library enables the creation of concurrency tokens and generating migrations to implement optimistic concurrency in EF Core using SQLite databases.

### Motivation

SQLite is one of the providers that doesn’t support concurrency tokens out of the box in EF Core. This library addresses this limitation by providing tools to easily enable optimistic concurrency in SQLite databases, allowing developers to manage concurrent data access effectively.

## Features

- **Extension Methods:** Extension methods for EF Core EntityTypeBuilder to facilitate the creation of concurrency tokens.
- **Migration Generation:** Functionality to generate EF Core migrations that apply schema modifications required for optimistic concurrency support in SQLite.
- **Easy Integration:** Seamless integration with existing EF Core projects, enabling developers to add optimistic concurrency support with minimal effort.

## Installation

To use the EF Core SQLite Concurrency Extension in your project, you can install it via NuGet Package Manager or the .NET CLI:

```sh
dotnet add package EFCore.Sqlite.Concurrency
```

## Usage

Configure the DbContext with EFCore sqlite concurrency using `AddSqliteConcurrency`

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder options)
{
    options.UseSqlite($"Data Sorce={DbPath}u")
        .AddSqliteConcurrency();
}
```

### Adding Concurrency Tokens

There's two ways to setup a concurrency token for your models

#### Define token in DbContext.OnModelCreating

```csharp
using EFCore.Sqlite.Concurrency;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder
        .Entity<MyEntity>()
        .HasConcurrencyToken();
}
```

This will generate a new shadow property, if you prefer to use a property defined in your model, another overload is available for `HasConcurrencyToken` that takes a property expression.

#### Define token in entity configuration class

```csharp
public void Configure(EntityTypeBuilder<Blog> builder)
{
    builder.HasConcurrencyToken();
}
```

## Contribution

Contributions to this project are welcome! If you encounter any issues or have suggestions for improvements, please feel free to open an issue or submit a pull request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.




