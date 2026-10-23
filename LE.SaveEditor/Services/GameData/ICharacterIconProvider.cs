using System.Windows.Media;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.GameData;

/// <summary>Portrait icon of a character: its mastery (sub-class) icon, or its base class icon when no mastery is chosen.</summary>
public interface ICharacterIconProvider
{
    /// <summary>
    /// Icon of <paramref name="mastery"/> when the character has one, otherwise of <paramref name="characterClass"/>;
    /// <c>null</c> when no matching image exists in the <c>Icons</c> folder (database not generated yet).
    /// </summary>
    ImageSource? Get(CharacterClass characterClass, Mastery mastery);

    /// <summary>Re-scans the <c>Icons</c> folder on the next request (call after the database generator ran).</summary>
    void ClearCache();
}
