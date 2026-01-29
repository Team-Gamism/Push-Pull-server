using Microsoft.EntityFrameworkCore;
using Server.Domain.Entity;

namespace Server.Infrastructure.Persistence.DbContext;

public class RoomContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<Room> Rooms => Set<Room>();
    
    public RoomContext(DbContextOptions<RoomContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("room", "room");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired();

            entity.Property(x => x.RoomCode)
                .HasColumnName("room_code")
                .HasMaxLength(20)
                .IsRequired();

            entity.HasIndex(x => x.RoomCode)
                .IsUnique()
                .HasDatabaseName("idx_room_room_code");

            entity.Property(x => x.SteamLobbyId)
                .HasColumnName("steam_lobby_id")
                .IsRequired();

            entity.Property(x => x.HostSteamId)
                .HasColumnName("host_steam_id")
                .IsRequired();

            entity.Property(x => x.MaxPlayers)
                .HasColumnName("max_players")
                .IsRequired();

            entity.Property(x => x.IsPrivate)
                .HasColumnName("is_private")
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(x => x.PasswordHash)
                .HasColumnName("password_hash");

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("ACTIVE");

            entity.HasIndex(x => x.Status)
                .HasDatabaseName("idx_room_status");

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()");

            entity.Property(x => x.ExpiresAt)
                .HasColumnName("expires_at")
                .HasColumnType("timestamptz");

            entity.HasIndex(x => x.ExpiresAt)
                .HasDatabaseName("idx_room_expires_at");
        });
    }
}