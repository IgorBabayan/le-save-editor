using System.Text.Json;
using LE.SaveEditor.DB.Data;
using LE.SaveEditor.DB.Data.Entities;
using LE.SaveEditor.DB.Infrastructure;

namespace LE.SaveEditor.DB.Import;

/// <summary>Creates <c>base.db</c> from scratch.</summary>
internal sealed class DatabaseBuilder(ConfigPaths paths)
{
    public const int BatchSize = 1000;

    public async Task BuildAsync(ParsedGameData data, CancellationToken ct)
    {
        Directory.CreateDirectory(paths.ConfigDirectory);

        // Build next to the real file and swap at the end: a failed or cancelled build
        // leaves the previous base.db untouched.
        var buildFile = paths.DatabaseFile + ".building";
        DeleteDatabase(buildFile);

        try
        {
            await using (var db = new GameDbContext(buildFile))
            {
                await db.Database.EnsureCreatedAsync(ct);

                using var bar = new ConsoleProgressBar("Creating database");
                var total = data.TotalRecords;
                var done = 0;

                await using var transaction = await db.Database.BeginTransactionAsync(ct);

                foreach (var node in GameNode.All)
                {
                    var records = data.Records[node];
                    foreach (var batch in records.Chunk(BatchSize))
                    {
                        await node.InsertAsync(db, batch, ct);
                        done += batch.Length;
                        bar.Report(done, total, $"{node.TableName} ({done}/{total})");
                    }
                }

                db.DataMigrations.Add(new DataMigration
                {
                    Name = MigrationName("Build"),
                    AppliedAtUtc = DateTime.UtcNow,
                    SourceSha256 = data.SourceSha256,
                    Added = total,
                    Changes = JsonSerializer.Serialize(
                        GameNode.All.ToDictionary(n => n.TableName, n => new { added = data.Records[n].Count }))
                });
                await db.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);
                await db.SealAsync(compact: true, ct);
                bar.Complete($"{total} records in {GameNode.All.Count} tables");
            }

            DeleteDatabase(paths.DatabaseFile);
            File.Move(buildFile, paths.DatabaseFile);
        }
        catch
        {
            DeleteDatabase(buildFile);
            throw;
        }
    }

    public static string MigrationName(string kind) => $"{DateTime.UtcNow:yyyyMMddHHmmss}_{kind}";

    /// <summary>Deletes a SQLite file together with its journal / WAL side files.</summary>
    public static void DeleteDatabase(string file)
    {
        foreach (var suffix in new[] { "", "-journal", "-wal", "-shm" })
        {
            if (File.Exists(file + suffix))
                File.Delete(file + suffix);
        }
    }
}
