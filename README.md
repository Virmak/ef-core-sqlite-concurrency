# EF Core SQLite Concurrency Extension

## Overview

The EF Core SQLite Concurrency Extension is a C# library designed to extend Entity Framework Core (EF Core) to support optimistic concurrency with SQLite databases. This library provides extension methods for EF Core DbContext or DbSet, enabling the creation of concurrency tokens and generating migrations to implement optimistic concurrency in SQLite databases.

### Motivation

SQLite is one of the providers that doesn’t support concurrency tokens out of the box in EF Core. This library addresses this limitation by providing tools to easily enable optimistic concurrency in SQLite databases, allowing developers to manage concurrent data access effectively.

## Features

- **Extension Methods:** Extension methods for EF Core DbContext or DbSet to facilitate the creation of concurrency tokens.
- **Migration Generation:** Functionality to generate EF Core migrations that apply schema modifications required for optimistic concurrency support in SQLite.
- **Easy Integration:** Seamless integration with existing EF Core projects, enabling developers to add optimistic concurrency support with minimal effort.

## Installation

To use the EF Core SQLite Concurrency Extension in your project, you can install it via NuGet Package Manager or the .NET CLI:

```sh
dotnet add package EFCore.Sqlite.Concurrency
