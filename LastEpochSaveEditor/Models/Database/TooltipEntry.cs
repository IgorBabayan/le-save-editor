using Newtonsoft.Json;

namespace LastEpochSaveEditor.Models.Database
{
	internal class TooltipEntry
    {
		[JsonProperty("modDisplay")]
		public int ModDisplay { get; set; }
	}
}
