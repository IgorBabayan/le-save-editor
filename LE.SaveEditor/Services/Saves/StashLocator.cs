using System.IO;
using System.Text.RegularExpressions;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

/// <summary>
/// Finds the stash file whose material storage a character uses. The game names stash files after the cycle, the stash
/// type (0 = softcore, 1 = hardcore, 2 = solo challenge) and, for a solo challenge character, its id:
/// <code>
/// STASH_CYCLE_8_0      cycle 8, softcore             STASH_0      legacy, softcore
/// STASH_CYCLE_8_1      cycle 8, hardcore             STASH_1      legacy, hardcore
/// STASH_CYCLE_8_2_1    cycle 8, solo character 1     STASH_2_1    legacy, solo character 1
/// </code>
/// Files of stash tabs (<c>..._TAB_n</c>) hold the stash items, not the materials. A character of a cycle uses the file
/// of its cycle; when there is none (legacy characters) it uses the legacy file of its type.
/// </summary>
public static partial class StashLocator
{
    /// <summary>Stash type of a softcore stash.</summary>
    public const int SoftcoreType = 0;

    /// <summary>Stash type of a hardcore stash.</summary>
    public const int HardcoreType = 1;

    /// <summary>Stash type of a solo challenge character's own stash.</summary>
    public const int SoloType = 2;

    [GeneratedRegex(@"^STASH_(?:CYCLE_(?<cycle>\d+)_)?(?<type>\d+)(?:_(?<id>\d+))?$", RegexOptions.IgnoreCase)]
    private static partial Regex StashFileName();

    /// <summary>Whether <paramref name="filePath"/> is a stash file with a material storage (not a stash tab).</summary>
    public static bool IsMainStashFile(string filePath) => StashFileName().IsMatch(Path.GetFileName(filePath));

    /// <summary>The stash of <paramref name="character"/> among <paramref name="stashes"/>; <c>null</c> when none fits.</summary>
    public static StashSave? Find(CharacterSave character, IEnumerable<StashSave> stashes)
    {
        var type = character.SoloCharacterChallenge ? SoloType : character.Hardcore ? HardcoreType : SoftcoreType;
        var id = type == SoloType ? character.Id : null;

        StashSave? legacy = null;
        foreach (var stash in stashes)
        {
            var match = StashFileName().Match(Path.GetFileName(stash.FilePath));
            if (!match.Success
                || !int.TryParse(match.Groups["type"].Value, out var stashType)
                || stashType != type
                || (id is not null && match.Groups["id"].Value != id)
                || (id is null && match.Groups["id"].Success))
                continue;

            if (!match.Groups["cycle"].Success)
                legacy ??= stash;
            else if (int.TryParse(match.Groups["cycle"].Value, out var cycle) && cycle == character.Cycle)
                return stash;
        }

        return legacy;
    }
}
