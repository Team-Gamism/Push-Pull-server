using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PushAndPull.Domain.Room.Entity.Config;

public class RoomConfig : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("room", "room");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.RoomName)
            .HasColumnName("room_name")
            .IsRequired();

        builder.Property(x => x.RoomCode)
            .HasColumnName("room_code")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => x.RoomCode)
            .IsUnique()
            .HasDatabaseName("idx_room_room_code");

        builder.Property(x => x.SteamLobbyId)
            .HasColumnName("steam_lobby_id")
            .IsRequired();

        builder.Property(x => x.HostSteamId)
            .HasColumnName("host_steam_id")
            .IsRequired();

        builder.HasIndex(x => x.HostSteamId)
            .HasDatabaseName("idx_room_host_steam_id");

        builder.Property(x => x.CurrentPlayers)
            .HasColumnName("current_players")
            .IsRequired();

        builder.Property(x => x.MaxPlayers)
            .HasColumnName("max_players")
            .IsRequired();

        builder.Property(x => x.IsPrivate)
            .HasColumnName("is_private")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion<string>();

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("idx_room_status");

        builder.HasIndex(x => new { x.Status, x.IsPrivate })
            .HasDatabaseName("idx_room_status_private");

        builder.HasIndex(x => new { x.Status, x.CreatedAt })
            .HasDatabaseName("idx_room_status_created_at");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz");

        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("idx_room_expires_at");

        builder.HasOne(r => r.Host)
            .WithMany()
            .HasForeignKey(r => r.HostSteamId)
            .HasPrincipalKey(u => u.SteamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
