using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PushAndPull.Domain.Auth.Entity.Config;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user", "auth");

        builder.HasKey(x => x.SteamId);

        builder.Property(x => x.SteamId)
            .HasColumnName("steam_id");

        builder.Property(x => x.Nickname)
            .HasColumnName("nickname")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.LastLoginAt)
            .HasColumnName("last_login_at")
            .HasColumnType("timestamptz");
    }
}
