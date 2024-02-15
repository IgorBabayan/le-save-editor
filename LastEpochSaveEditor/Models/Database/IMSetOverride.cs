using Newtonsoft.Json;

namespace LastEpochSaveEditor.Models.Database
{
	internal class IMSetOverride
	{
		[JsonProperty("classID")]
		public int ClassID { get; set; }

		[JsonProperty("imSetTier")]
		public int ImSetTier { get; set; }
	}
}
