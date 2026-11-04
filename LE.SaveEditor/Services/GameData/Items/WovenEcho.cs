namespace LE.SaveEditor.Services.GameData.Items;

/// <summary>
/// A woven echo from the <c>itemTypes</c> node of <c>base.db</c>: a sub item of item type <see cref="BaseTypeId"/>
/// ("Woven Echo"), e.g. <c>{"name":"False Temple","subTypeID":2,"cannotDrop":false,"description":"A temple from a
/// forgotten timeline, filled with shrines."}</c>. The stash keeps the woven echoes of the game's "Woven Echoes" tab in
/// <c>wovenEchoesList</c> (see <c>WovenEchoData</c> for how an echo is stored).
/// </summary>
/// <param name="Id">The echo's <c>subTypeID</c> in the game data (2 = False Temple).</param>
/// <param name="Name">Shown name, e.g. "Champions' Colosseum".</param>
/// <param name="Description">What the echo holds, without the game's rich-text tags; empty when the data has none.</param>
/// <param name="CannotDrop">The game does not drop the echo (placeholders like Fading Echo, or retired echoes).</param>
/// <param name="IconPath">Icon path for <see cref="IIconProvider"/> (<c>woven_echo/false_temple</c>), or <c>null</c>.</param>
public sealed record WovenEcho(int Id, string Name, string Description, bool CannotDrop, string? IconPath)
{
    /// <summary>Item type (<c>baseTypeID</c>) of woven echoes.</summary>
    public const int BaseTypeId = 107;
}
