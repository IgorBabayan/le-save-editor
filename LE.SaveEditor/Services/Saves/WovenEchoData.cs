using System.Text.Json.Nodes;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Services.Saves;

/// <summary>
/// Decodes and encodes the woven echoes of the game's "Woven Echoes" stash tab: the entries of the stash's
/// <c>wovenEchoesList</c>.
/// </summary>
/// <remarks>
/// Unlike other items, the byte after the item type is always 0 and the echo itself (its <c>subTypeID</c> in the game
/// data) is the byte after it; the entry sits in container 99 at x 0, y = the echo, with the amount in <c>quantity</c>:
/// <code>
/// {"itemData":null,"data":[6, id, id, id, id, 107, 0, 29],"inventoryPosition":{"x":0,"y":29},"quantity":1,
///  "containerID":99,"formatVersion":2}                                                  = Collapsing Horizon
/// </code>
/// An echo written like other materials (<c>[2, 107, 2]</c> or <c>[6, id, 107, 2, 0]</c>) is read by the game as the
/// placeholder echo 0 ("Fading Echo", shown as "ERROR") or dropped.
/// </remarks>
public static class WovenEchoData
{
    /// <summary>Container of the entries of <c>wovenEchoesList</c>.</summary>
    public const int ContainerId = 99;

    /// <summary>
    /// The echo (<c>subTypeID</c> of item type 107) stored in <paramref name="data"/>; <c>null</c> when it is another item
    /// or the format is unknown.
    /// </summary>
    public static int? Read(IReadOnlyList<byte> data)
    {
        var header = data.Count == 0
            ? -1
            : data[0] switch
            {
                ItemDataReader.Version2 => 1,
                ItemDataReader.Version6 => 1 + ItemDataReader.InstanceIdLength,
                _ => -1
            };

        // Item type, the 0 byte, then the echo.
        return header >= 0 && data.Count >= header + 3 && data[header] == WovenEcho.BaseTypeId
            ? data[header + 2]
            : null;
    }

    /// <summary>The echo of a <c>wovenEchoesList</c> entry; <c>null</c> for anything else.</summary>
    public static int? Read(JsonNode? entry)
    {
        if (entry is not JsonObject item || item["data"] is not JsonArray data)
            return null;

        var bytes = new List<byte>(data.Count);
        foreach (var value in data)
        {
            if (value is not JsonValue number || !number.TryGetValue<int>(out var b) || b is < 0 or > 255)
                return null;
            bytes.Add((byte)b);
        }

        return Read(bytes);
    }

    /// <summary>
    /// A <c>wovenEchoesList</c> entry as the game writes it: format 6 with a new instance id, container 99 and the slot
    /// of the echo (x 0, y = echo).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="echoId"/> does not fit one byte.</exception>
    public static JsonObject CreateEntry(int echoId, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(echoId);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(echoId, byte.MaxValue);

        var id = new byte[ItemDataReader.InstanceIdLength];
        Random.Shared.NextBytes(id);
        id[0] &= 0x7F;

        var data = new JsonArray(ItemDataReader.Version6);
        foreach (var b in id)
            data.Add((int)b);
        data.Add(WovenEcho.BaseTypeId);
        data.Add(0);
        data.Add(echoId);

        return new JsonObject
        {
            ["itemData"] = null,
            ["data"] = data,
            ["inventoryPosition"] = new JsonObject { ["x"] = 0, ["y"] = echoId },
            ["quantity"] = quantity,
            ["containerID"] = ContainerId,
            ["formatVersion"] = 2
        };
    }
}
