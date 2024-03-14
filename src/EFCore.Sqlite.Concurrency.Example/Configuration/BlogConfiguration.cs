using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EFCore.Sqlite.Concurrency.Example.Configuration;

public class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder
            .Property(x => x.RowVersion)
            .IsConcurrencyToken();

        // Specify the colomn name that will be created
        // builder
        //     .HasConcurrencyToken("RowVersion");

        // Use existing property on the model
        // builder
        //     .HasConcurrencyToken(b => b.RowVerion);

        builder
            .Property(b => b.Url)
            .IsRequired();
            
    }
}