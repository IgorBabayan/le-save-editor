namespace LastEpochSaveEditor.Models;

public class SavedItem
{
	[JsonProperty("itemData")]
	public object ItemData { get; set; }

	[JsonProperty("data")]
	public List<int> Data { get; set; } = new();

	[JsonProperty("inventoryPosition")]
	public InventoryPosition InventoryPosition { get; set; }

	[JsonProperty("quantity")]
	public int Quantity { get; set; }

	[JsonProperty("containerID")]
	public int ContainerID { get; set; }

	[JsonProperty("formatVersion")]
	public int FormatVersion { get; set; }
}