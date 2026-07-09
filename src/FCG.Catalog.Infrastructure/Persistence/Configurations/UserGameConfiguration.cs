using FCG.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Catalog.Infrastructure.Persistence.Configurations;

public sealed class UserGameConfiguration : IEntityTypeConfiguration<UserGame>
{
    public void Configure(EntityTypeBuilder<UserGame> builder)
    {
        builder.ToTable("UserGames");
        builder.HasKey(userGame => userGame.Id);
        builder.HasIndex(userGame => new { userGame.UserId, userGame.GameId })
            .IsUnique();
        builder.HasIndex(userGame => userGame.OrderId);

        builder.HasOne(userGame => userGame.Game)
            .WithMany()
            .HasForeignKey(userGame => userGame.GameId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
