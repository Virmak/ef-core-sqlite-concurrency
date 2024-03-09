using EFCore.Sqlite.Concurrency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcurLite.Example.Configuration;

public class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder
            .HasConcurrencyToken();
        builder
            .Property(b => b.Url)
            .IsRequired();

        // builder.Property<string>("Shadow");
    }
}