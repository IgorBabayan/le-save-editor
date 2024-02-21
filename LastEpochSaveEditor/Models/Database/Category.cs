namespace LastEpochSaveEditor.Models.Database
{
	internal class Category
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("entries")]
		public List<int> Entries { get; set; }
	}
}
