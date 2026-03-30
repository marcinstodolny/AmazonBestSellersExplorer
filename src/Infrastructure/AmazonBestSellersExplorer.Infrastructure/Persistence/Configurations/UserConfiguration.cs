using AmazonBestSellersExplorer.Domain.Entities;
using AmazonBestSellersExplorer.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AmazonBestSellersExplorer.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    private static readonly ValueConverter<Username, string> UsernameConverter = new(
        username => username.Value,
        value => Username.FromPersistence(value));

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.Property(user => user.Username)
            .HasConversion(UsernameConverter)
            .HasMaxLength(Username.MaxLength)
            .IsRequired();

        builder.HasIndex(user => user.Username)
            .IsUnique();

        builder.Property(user => user.PasswordHash)
            .IsRequired();

        builder.Property(user => user.CreatedAtUtc)
            .IsRequired();

        builder.HasMany(user => user.FavoriteProducts)
            .WithOne()
            .HasForeignKey(favoriteProduct => favoriteProduct.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(user => user.FavoriteProducts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
