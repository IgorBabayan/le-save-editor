namespace LE.SaveEditor.DB.Data.Entities;

/// <summary>
/// One element of a node in <c>data.json</c> (one class, one affix, one unique, ...).
/// The full element is kept as JSON so nothing from the game data is lost and the schema
/// does not have to change when maxroll adds fields; <see cref="Hash"/> is what updates diff on.
/// </summary>
public abstract class GameEntity
{
    /// <summary>Stable id of the element: its property name, its id field, or its index.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Display name when the element has one (for lookups and debugging).</summary>
    public string? Name { get; set; }

    /// <summary>Upper-case hex SHA-256 of <see cref="Json"/>.</summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>The element as compact JSON.</summary>
    public string Json { get; set; } = string.Empty;
}

/// <summary><c>classes</c></summary>
public sealed class GameClass : GameEntity
{
}

/// <summary><c>affixes</c></summary>
public sealed class Affix : GameEntity
{
}

/// <summary><c>itemTypes</c></summary>
public sealed class ItemType : GameEntity
{
}

/// <summary><c>itemCategories</c></summary>
public sealed class ItemCategory : GameEntity
{
}

/// <summary><c>uniques</c></summary>
public sealed class Unique : GameEntity
{
}

/// <summary><c>properties</c>: display name and display rules of each property id (implicit / affix stat names).</summary>
public sealed class GameProperty : GameEntity
{
}

/// <summary><c>ailments</c>: ailment names, referenced by the <c>specialTag</c> of implicits and affixes.</summary>
public sealed class Ailment : GameEntity
{
}

/// <summary><c>stats</c></summary>
public sealed class Stat : GameEntity
{
}

/// <summary>
/// One image of the <c>icons</c> node. In <c>data.json</c> the node groups icon names by folder
/// (<c>"uniques": ["abacus_rod", ...]</c>); every name becomes its own row.
/// <see cref="GameEntity.Key"/> and <see cref="Path"/> are <c>{Group}/{Name}</c>.
/// </summary>
public sealed class Icon : GameEntity
{
    /// <summary>Folder of the icon, e.g. <c>uniques</c>, <c>helmets</c>, <c>1h_axes</c>.</summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>
    /// Icon path without extension, e.g. <c>uniques/spirit_xylem</c>: downloaded from
    /// <c>.../last-epoch/webp/{Path}.webp</c>, loaded by the editor from <c>Icons/{Path}.png</c>.
    /// </summary>
    public string? Path { get; set; }
}
