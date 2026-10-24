namespace LE.SaveEditor.Services.GameData;

/// <summary>
/// Runs <c>LE.SaveEditor.DB</c> (shipped in <c>Tools\LE.SaveEditor.DB</c> next to the editor) in the background,
/// without a console window, to generate or update <c>base.db</c> and the icons.
/// </summary>
public interface IDatabaseGenerator
{
    /// <summary>Full path of <c>LE.SaveEditor.DB.exe</c>.</summary>
    string ExecutablePath { get; }

    /// <summary>
    /// Builds the database if it does not exist yet, otherwise updates it, and waits until the generator exits.
    /// Never throws for generator problems: they are returned as a <see cref="DatabaseGenerationStatus.Failed"/> result.
    /// </summary>
    /// <param name="progress">
    /// Receives every progress bar and text line the generator reports, in order.
    /// Create it on the UI thread to get the reports there.
    /// </param>
    /// <param name="cancellationToken">Stops the generator process.</param>
    Task<DatabaseGenerationResult> RunAsync(
        IProgress<DatabaseGeneratorEvent>? progress,
        CancellationToken cancellationToken = default);
}
