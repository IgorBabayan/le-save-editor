using LE.SaveEditor.DB.Data.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LE.SaveEditor.DB.Data;

/// <summary>SQLite database <c>base.db</c>: one table per node of <c>data.json</c> plus the migration history.</summary>
internal sealed class GameDbContext(string databaseFile) : DbContext
{
    public DbSet<DataMigration> DataMigrations => Set<DataMigration>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // No pooling: the file is released as soon as the context is disposed,
        // so it can be swapped/deleted right after a build.
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databaseFile,
            Pooling = false
        }.ToString();

        options.UseSqlite(connectionString);
    }

    /// <summary>
    /// Version of the table layout, stored as SQLite <c>user_version</c>. Bump it whenever an entity
    /// or table changes: Update then rebuilds the database instead of failing on the old layout.
    /// </summary>
    public const int SchemaVersion = 4;

    public async Task<int> GetSchemaVersionAsync(CancellationToken ct)
    {
        var connection = Database.GetDbConnection();
        await Database.OpenConnectionAsync(ct);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA user_version;";
            return Convert.ToInt32(await command.ExecuteScalarAsync(ct));
        }
        finally
        {
            await Database.CloseConnectionAsync();
        }
    }

    /// <summary>
    /// Leaves base.db as one self-contained file: EF Core creates SQLite databases in WAL mode,
    /// which needs writable -wal/-shm side files and can fail to open read-only from Program Files.
    /// </summary>
    public async Task SealAsync(bool compact, CancellationToken ct)
    {
        await Database.ExecuteSqlRawAsync("PRAGMA journal_mode = DELETE;", ct);
        await Database.ExecuteSqlRawAsync($"PRAGMA user_version = {SchemaVersion};", ct);
        if (compact)
            await Database.ExecuteSqlRawAsync("VACUUM;", ct);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var node in GameNode.All)
        {
            modelBuilder.Entity(node.EntityType, entity =>
            {
                entity.ToTable(node.TableName);
                entity.HasKey(nameof(GameEntity.Key));
                entity.Property(nameof(GameEntity.Hash)).IsRequired();
                entity.Property(nameof(GameEntity.Json)).IsRequired();
                entity.HasIndex(nameof(GameEntity.Name));
            });
        }

        modelBuilder.Entity<Icon>(entity =>
        {
            entity.HasIndex(i => i.Group);
            entity.HasIndex(i => i.Path);
        });

        modelBuilder.Entity<DataMigration>(entity =>
        {
            entity.ToTable("__DataMigrations");
            entity.HasKey(m => m.Id);
        });
    }
}
