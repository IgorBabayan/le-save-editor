namespace LastEpochSaveEditor.Models.Database
{
	internal class TooltipDescription
    {
		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("altText")]
		public string AltText { get; set; }

		[JsonProperty("setMod")]
		public bool SetMod { get; set; }

		[JsonProperty("setRequirement")]
		public int SetRequirement { get; set; }
	}
}
