namespace LastEpochSaveEditor.Models.Database;

public class IMSetOverride
{
	[JsonProperty("classID")]
	public int ClassID { get; set; }

	[JsonProperty("imSetTier")]
	public int ImSetTier { get; set; }
}