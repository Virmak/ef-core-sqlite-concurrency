
using ConcurLite.Example.Configuration;
using EFCore.Sqlite.Concurrency;
using Microsoft.EntityFrameworkCore;

namespace ConcurLite.Example;
public class BloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    public string DbPath { get; }

    public BloggingContext()
    {
        var path = Environment.CurrentDirectory;
        DbPath = Path.Join(path, "blogging.db");
        Console.WriteLine(DbPath);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}")
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .AddSqliteConcurrency();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        new BlogConfiguration().Configure(modelBuilder.Entity<Blog>());
    }
}

public class Blog
{
    public int BlogId { get; set; }
    public required string Url { get; set; }

    public List<Post> Posts { get; } = [];
}

public class Post
{
    public int PostId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }

    public int BlogId { get; set; }
    public Blog? Blog { get; set; }
}