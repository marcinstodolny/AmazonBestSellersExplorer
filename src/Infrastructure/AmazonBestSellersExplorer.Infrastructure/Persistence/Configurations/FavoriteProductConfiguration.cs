using AmazonBestSellersExplorer.Domain.Entities;
using AmazonBestSellersExplorer.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmazonBestSellersExplorer.Infrastructure.Persistence.Configurations;

public sealed class FavoriteProductConfiguration : IEntityTypeConfiguration<FavoriteProduct>
{
    private const string CaseInsensitiveCollation = "Latin1_General_100_CI_AS";

    public void Configure(EntityTypeBuilder<FavoriteProduct> builder)
    {
        builder.ToTable("favorite_products");

        builder.HasKey(favoriteProduct => favoriteProduct.Id);

        builder.Property(favoriteProduct => favoriteProduct.Id)
            .ValueGeneratedNever();

        builder.Property(favoriteProduct => favoriteProduct.UserId)
            .IsRequired();

        builder.Property(favoriteProduct => favoriteProduct.AmazonProductId)
            .HasMaxLength(FavoriteProductRules.AmazonProductIdMaxLength)
            .IsUnicode(false)
            .UseCollation(CaseInsensitiveCollation)
            .IsRequired();

        builder.Property(favoriteProduct => favoriteProduct.Title)
            .IsRequired();

        builder.Property(favoriteProduct => favoriteProduct.Price)
            .HasPrecision(18, 2);

        builder.Property(favoriteProduct => favoriteProduct.Rating)
            .IsRequired(false);

        builder.Property(favoriteProduct => favoriteProduct.ProductUrl)
            .IsRequired();

        builder.Property(favoriteProduct => favoriteProduct.ImageUrl)
            .IsRequired(false);

        builder.Property(favoriteProduct => favoriteProduct.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(favoriteProduct => new
            {
                favoriteProduct.UserId,
                favoriteProduct.AmazonProductId
            })
            .IsUnique();
    }
}
