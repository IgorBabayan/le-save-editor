using Newtonsoft.Json;

namespace LastEpochSaveEditor.Models
{
	public class InventoryPosition
	{
		[JsonProperty("x")]
		public int X { get; set; }

		[JsonProperty("y")]
		public int Y { get; set; }
	}
}
