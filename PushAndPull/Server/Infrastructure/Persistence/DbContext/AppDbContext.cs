using Microsoft.EntityFrameworkCore;
using Server.Domain.Entity;

namespace Server.Infrastructure.Persistence.DbContext;

public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<User> Users => Set<User>();
    
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user", "user");

            entity.HasKey(x => x.SteamId);

            entity.Property(x => x.SteamId)
                .HasColumnName("steam_id");

            entity.Property(x => x.Nickname)
                .HasColumnName("nickname")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz");

            entity.Property(x => x.LastLoginAt)
                .HasColumnName("last_login_at")
                .HasColumnType("timestamptz");
        });
        
        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("room", "room");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.RoomName)
                .HasColumnName("room_name")
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
            
            entity.HasIndex(x => x.HostSteamId)
                .HasDatabaseName("idx_room_host_steam_id");
            
            entity.Property(x => x.CurrentPlayers)
                .HasColumnName("current_players")
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
                .HasConversion<string>();

            entity.HasIndex(x => x.Status)
                .HasDatabaseName("idx_room_status");

            entity.HasIndex(x => new { x.Status, x.IsPrivate })
                .HasDatabaseName("idx_room_status_private");

            entity.HasIndex(x => new { x.Status, x.CreatedAt })
                .HasDatabaseName("idx_room_status_created_at");

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz");

            entity.Property(x => x.ExpiresAt)
                .HasColumnName("expires_at")
                .HasColumnType("timestamptz");

            entity.HasIndex(x => x.ExpiresAt)
                .HasDatabaseName("idx_room_expires_at");
            
            entity.HasOne(r => r.Host)
                .WithMany()
                .HasForeignKey(r => r.HostSteamId)
                .HasPrincipalKey(u => u.SteamId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}